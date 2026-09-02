Shader "Hidden/ColbyO/VNTG/Editor/PalettePreviewGenerator"
{
    Properties
    {
        _WheelRadius ("Wheel Radius", Float) = 35.8
        _OutlineSize ("Outline Size", Float) = 2.5
        _CardFoldSize ("Card Fold Size", Float) = 28.16
        _CardCornerRadius ("Card Corner Radius", Float) = 7.68
        
        _OutlineColor ("Outline Color", Color) = (0.1, 0.1, 0.1, 1.0)
        _FileColor ("File Color", Color) = (0.2, 0.2, 0.2, 1.0)
        _BackgroundColor ("Background Color", Color) = (0.0, 0.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };

            float _Width;
            float _Height;
            float _WheelRadius;
            float _OutlineSize;
            float2 _CardHalfSize;
            float _CardCornerRadius;
            float _CardFoldSize;

            float4 _OutlineColor;
            float4 _FileColor;
            float4 _BackgroundColor;

            int _PaletteCount;
            float4 _PaletteColors[256];

            Varyings vert (Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float ChamferedRectSDF(float2 p, float2 size, float radius, float cutSize)
            {
                float2 d = max(abs(p) - size + radius, 0.0);
                float boxSDF = length(d) - radius;
                
                float cutPlane = (p.x - (size.x - cutSize)) + (p.y - (size.y - cutSize)) - cutSize;
                return max(boxSDF, cutPlane);
            }

            float GetAngleAt(float2 delta)
            {
                float angle = atan2(delta.y, delta.x) * 57.2957795;
                if (angle < 0.0) angle += 360.0;
                return angle;
            }

            float4 GetSliceColor(float2 delta, float distFromCenter)
            {
                if (_PaletteCount <= 0) return _FileColor;
                if (_PaletteCount == 1) return _PaletteColors[0];

                float angle = GetAngleAt(delta);
                float sliceSize = 360.0 / (float)_PaletteCount;
                
                float angularPixelWidth = (1.0 / (distFromCenter + 0.001)) * 57.2957795;

                float remainder = fmod(angle, sliceSize);
                float distToEdge = min(remainder, sliceSize - remainder);

                int currentIndex = min((int)floor(angle / sliceSize), _PaletteCount - 1);

                if (distToEdge < angularPixelWidth * 0.5)
                {
                    float t = clamp((distToEdge + (angularPixelWidth * 0.5)) / angularPixelWidth, 0.0, 1.0);
                    
                    int neighborIndex = (remainder < sliceSize * 0.5)
                        ? (currentIndex - 1 + _PaletteCount) % _PaletteCount
                        : (currentIndex + 1) % _PaletteCount;

                    return lerp(_PaletteColors[neighborIndex], _PaletteColors[currentIndex], t);
                }

                return _PaletteColors[currentIndex];
            }

            float4 SamplePreviewColorWheel(float2 delta, float distFromCenter)
            {
                float aaWidth = 1.0;
                float innerEdge = _WheelRadius;
                float outerEdge = _WheelRadius + _OutlineSize;

                if (distFromCenter >= outerEdge + aaWidth) return _FileColor;

                float4 wheelColor = GetSliceColor(delta, distFromCenter);

                float toOutline = clamp((distFromCenter - (innerEdge - aaWidth * 0.5)) / aaWidth, 0.0, 1.0);
                float toBackground = clamp((distFromCenter - (outerEdge - aaWidth * 0.5)) / aaWidth, 0.0, 1.0);

                float4 mixedColor = lerp(wheelColor, _OutlineColor, toOutline);
                return lerp(mixedColor, _FileColor, toBackground);
            }

            float4 frag (Varyings input) : SV_Target
            {
                float2 pixelPos = input.uv * float2(_Width, _Height);
                float2 center = float2(_Width, _Height) * 0.5;
                float2 delta = pixelPos - center;
                float distFromCenter = length(delta);

                float sdf = ChamferedRectSDF(delta, _CardHalfSize, _CardCornerRadius, _CardFoldSize);

                if (sdf <= 0.0)
                {
                    if (sdf > -_OutlineSize)
                    {
                        return _OutlineColor;
                    }
                    else
                    {
                        return SamplePreviewColorWheel(delta, distFromCenter);
                    }
                }
                
                return _BackgroundColor;
            }
            ENDHLSL
        }
    }
}