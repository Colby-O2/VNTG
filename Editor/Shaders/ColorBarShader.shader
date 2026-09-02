Shader "Hidden/ColbyO/VNTG/Editor/ColorBar"
{
    Properties
    {
        _InColor ("Input Color", Vector) = (1,1,1,1)

        _Dimensions ("Rect Width (X) Height (Y)", Vector) = (1,1,0,0)
        _Radius ("Corner Radius", Float) = 4.0
        
        _SliderMode ("Slider Mode (0-2=RGB, 3-6=HSV, 7=Lightness From HSV, 8-10=CIELAB, 11-13=HSL)", Int) = 0
        _IsHorizontal ("Is Horizontal Toggle (0=False, 1=True)", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Assets/ColbyO/VNTG/Shaders/HLSL/ColorSpace.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _InColor;

            float4 _Dimensions;
            float _Radius;
            int _SliderMode;
            int _IsHorizontal;

            float RoundedRectangleAlpha(float2 uv, float2 size, float radius)
            {
                float2 pixelPos = uv * size;
                float2 halfSize = size * 0.5;
                float2 d = abs(pixelPos - halfSize) - (halfSize - radius);
                float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
                return 1.0 - smoothstep(radius - 1.0, radius, dist);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float alpha = RoundedRectangleAlpha(input.uv, _Dimensions.xy, _Radius);
                if (alpha <= 0.001) discard;

                float evaluationCoordinate = (_IsHorizontal == 1) ? input.uv.x : input.uv.y;
                float t = clamp(evaluationCoordinate, 0.0001, 0.9999);
                
                float3 rgbOutput = float3(0, 0, 0);

                if (_SliderMode == 0)
                {
                    rgbOutput = float3(t, _InColor.g, _InColor.b);
                }
                else if (_SliderMode == 1)
                {
                    rgbOutput = float3(_InColor.r, t, _InColor.b);
                }
                else if (_SliderMode == 2)
                {
                    rgbOutput = float3(_InColor.r, _InColor.g, t);
                }
                else if (_SliderMode == 3)
                {
                    rgbOutput = HSVToRGB(float3(_InColor.x, _InColor.y, t));
                }
                else if (_SliderMode == 4)
                {
                    rgbOutput = HSVToRGB(float3(t, _InColor.y, _InColor.z));
                }
                else if (_SliderMode == 5)
                {
                    rgbOutput = HSVToRGB(float3(_InColor.x, t, _InColor.z));
                }
                else if (_SliderMode == 6)
                {
                    rgbOutput = HSVToRGB(float3(_InColor.x, _InColor.y, t));
                }
                else if (_SliderMode == 7)
                {
                    float3 currentHsl = HSVToHSL(_InColor.xyz);
                    currentHsl.z = t; 

                    float3 mappedHsv = HSLToHSV(currentHsl);
                    rgbOutput = HSVToRGB(mappedHsv);
                }
                else if (_SliderMode == 8)
                {
                    rgbOutput = LabToRGB(t * 100.0, _InColor.g, _InColor.b);

                    if (input.uv.y < 0.25 && (rgbOutput.r > 1 || rgbOutput.r < 0 || rgbOutput.g > 1 || rgbOutput.g < 0 || rgbOutput.b > 1 || rgbOutput.b < 0))
                    {
                        rgbOutput = float3(1, 0, 0);
                    }
                }
                else if (_SliderMode == 9)
                {
                    float targetA = lerp(-128.0, 127.0, t);
                    rgbOutput = LabToRGB(_InColor.r, targetA, _InColor.b);

                    if (input.uv.y < 0.25 && (rgbOutput.r > 1 || rgbOutput.r < 0 || rgbOutput.g > 1 || rgbOutput.g < 0 || rgbOutput.b > 1 || rgbOutput.b < 0))
                    {
                        rgbOutput = float3(1, 0, 0);
                    }
                }
                else if (_SliderMode == 10)
                {
                    float targetB = lerp(-128.0, 127.0, t);
                    rgbOutput = LabToRGB(_InColor.r, _InColor.g, targetB);

                    if (input.uv.y < 0.25 && (rgbOutput.r > 1 || rgbOutput.r < 0 || rgbOutput.g > 1 || rgbOutput.g < 0 || rgbOutput.b > 1 || rgbOutput.b < 0))
                    {
                        rgbOutput = float3(1, 0, 0);
                    }
                }
                else if (_SliderMode == 11)
                {
                    rgbOutput = HSLToRGB(float3(t, _InColor.g, _InColor.b));
                }
                else if (_SliderMode == 12)
                {
                    rgbOutput = HSLToRGB(float3(_InColor.r, t, _InColor.b));
                }
                else if (_SliderMode == 13)
                {
                    rgbOutput = HSLToRGB(float3(_InColor.r, _InColor.g, t));
                }

                #ifndef UNITY_COLORSPACE_GAMMA
                rgbOutput = SRGBToLinear(rgbOutput);
                #endif

                return float4(rgbOutput, alpha);
            }
            ENDHLSL
        }
    }
}