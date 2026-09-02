Shader "Hidden/ColbyO/VNTG/Editor/ColorWheel"
{
    Properties
    {
        _WheelType ("Wheel Type", Float) = 0
        _Modifier ("Fixed Channel Value", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha
        OneMinusSrcAlpha
        ZWrite
        Off

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

            float _WheelType;
            float _Modifier;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uvDir = input.uv - 0.5;
                float dist = length(uvDir);

                float radius = 0.5;
                float aaWidth = 0.005;
                float alpha = smoothstep(radius, radius - aaWidth, dist);

                if (alpha <= 0.0) return float4(0, 0, 0, 0);

                float angle = atan2(uvDir.y, uvDir.x);
                float hue = angle / (2.0 * 3.14159265);
                if (hue < 0.0) hue += 1.0;

                float saturation = 0.0;
                float brightness = 0.0;

                if (_WheelType < 0.5)
                {
                    saturation = clamp(dist / radius, 0.0, 1.0);
                    brightness = _Modifier;
                }
                else
                {
                    saturation = _Modifier;
                    brightness = clamp(dist / radius, 0.0, 1.0);
                }

                float3 rgb = HSVToRGB(float3(hue, saturation, brightness));

                #ifndef UNITY_COLORSPACE_GAMMA
                rgb = SRGBToLinear(rgb);
                #endif

                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}