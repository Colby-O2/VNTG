Shader "Hidden/ColbyO/VNTG/UnlitWhite"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "InteractionNormalPass" = "Enabled"}
        Pass
        {
            ZTest LEqual
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS   : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 posCS    : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            struct FragmentOutput {
                float4 mask   : SV_Target0;
                float4 normal : SV_Target1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            FragmentOutput frag(Varyings IN) : SV_Target
            {
                FragmentOutput output;
                output.mask = float4(1, 1, 1, 1);
                
                float3 normal = normalize(IN.normalWS);
                output.normal = float4(normal * 0.5 + 0.5, 1.0); 
                
                return output;
            }
            ENDHLSL
        }
    }
}