//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXMaster_URP.shader
//-----------------------------------------------------------------------

Shader "Hidden/PSXMaster_URP"
{
    Properties
    {
        [Header(Resolution)]
        _PixelResolution ("Pixelation Resolution", Vector) = (256, 256, 0, 0)
        
        [Header(Color and Dither)]
        _ColorPrecision ("Color Steps", Float) = 32
        _DitherPattern ("Dither Pattern", Float) = 1.0
        _DitherPixelPerfect ("Use Pixel Perfect Dither", Float) = 1.0
        _DitherScale ("Dither Pattern Scale", Float) = 1.0
        _DitherThreshold ("Dither Sensitivity", Float) = 1.0
        
        [Header(Fog)]
        _EnableFog ("Enable Fog", Float) = 0
        _FogDensity ("Fog Density", Float) = 1.0
        _FogColor ("Fog Color", Color) = (0, 0, 0, 1)
        _FogNoiseStrength ("Fog Noise Strength", Float) = 0.1
        _FogNoiseScale ("Fog Noise Scale", Float) = 10.0
        _FogNoiseStart ("Fog Noise Start", Float) = 0.7
        _FogEdgeSmoothness ("Fog Edge Smoothness", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float2 _PixelResolution;
            float _ColorPrecision;
            int _DitherPattern;
            int _DitherPixelPerfect;
            float _DitherScale;
            float _DitherThreshold;
            
            int _EnableFog;
            float4 _FogColor;
            float _FogDensity;
            float _FogEdgeSmoothness;
            float _FogNoiseStrength;
            float _FogNoiseScale;
            float _FogNoiseStart;

            static const float4x4 DITHER_PATTERNS[11] = {\
                float4x4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                float4x4(0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0),
                float4x4(0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5) / 16.0,
                float4x4(2.0, -2.0, -2.0,  2.0, -2.0,  4.0,  4.0, -2.0, -2.0,  4.0,  4.0, -2.0, 2.0, -2.0, -2.0,  2.0) / 4.0,
                float4x4(0, 2, 0, 2, 3, 1, 3, 1, 0, 2, 0, 2, 3, 1, 3, 1) / 4.0,
                float4x4(0, 4, 8, 12, 0, 4, 8, 12, 0, 4, 8, 12, 0, 4, 8, 12) / 16.0,
                float4x4(0, 0, 0, 0, 8, 8, 8, 8, 15, 15, 15, 15, 8, 8, 8, 8) / 16.0,
                float4x4(3, 6, 9, 12, 6, 9, 12, 3, 9, 12, 3, 6, 12, 3, 6, 9) / 16.0,
                float4x4(13, 10, 11, 14, 6, 1, 2, 7, 5, 0, 3, 8, 12, 9, 4, 15) / 16.0,
                float4x4(0.1, 0.7, 0.3, 0.9, 0.5, 0.2, 0.8, 0.4, 0.9, 0.3, 0.7, 0.1, 0.4, 0.8, 0.2, 0.5),
                float4x4(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            };

            /**
            * Generates a random value from a 3D vector.
            *
            * @param p  The input 3D coordinates.
            * @return   A random float value in the range [0.0, 1.0].
            */
            inline float Hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            /**
            * Calculates a andom gradient vector for a given 3D point.
            *
            * @param p  The input 3D coordinates.
            * @return   A gradient direction.
            */
            inline float3 Gradient(float3 p)
            {
                float h = Hash31(p) * 6.2831853;
                return float3(cos(h), sin(h), cos(h * 0.5));
            }

            /**
            * Computes the quintic smoothing for Perlin noise.
            *
            * @param t  The distance [0, 1] within a cell.
            * @return   The smoothed weight.
            */
            inline float3 Fade(float3 t)
            {
                return t * t * t * (t * (t * 6 - 15) + 10);
            }

            /**
            * Generates 3D Perlin Noise for a given coordinate.
            *
            * @param p  The input 3D coordinates.
            * @return   A noise value in the range [0.0, 1.0].
            */
            inline float Perlin3D(float3 p)
            {
                float3 pi = floor(p);
                float3 pf = frac(p);

                float3 f = Fade(pf);

                // Calculate dot products between corner gradients and distance vectors
                float n000 = dot(Gradient(pi + float3(0,0,0)), pf - float3(0,0,0));
                float n100 = dot(Gradient(pi + float3(1,0,0)), pf - float3(1,0,0));
                float n010 = dot(Gradient(pi + float3(0,1,0)), pf - float3(0,1,0));
                float n110 = dot(Gradient(pi + float3(1,1,0)), pf - float3(1,1,0));
                float n001 = dot(Gradient(pi + float3(0,0,1)), pf - float3(0,0,1));
                float n101 = dot(Gradient(pi + float3(1,0,1)), pf - float3(1,0,1));
                float n011 = dot(Gradient(pi + float3(0,1,1)), pf - float3(0,1,1));
                float n111 = dot(Gradient(pi + float3(1,1,1)), pf - float3(1,1,1));

                // Trilinear interpolation
                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);

                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);

                return lerp(nxy0, nxy1, f.z) * 0.5 + 0.5;
            }

             /**
             * Calculates the perceived brightness (Luminance) of an RGB color
             *
             * @param c  The input RGB color.
             * @return   The brightness of the color.
             */
            inline float GetLuminance(float3 c)
            {
                return dot(c, float3(0.299, 0.587, 0.114));
            }

            /**
            * Retrieves a predefined 4x4 dither matrix from a constant array.
            *
            * @param index  The index of the desired pattern in DITHER_PATTERNS.
            * @return       A 4x4 matrix containing the dither pattern.
            */
            inline float4x4 GetDitherPattern(int index) {
                return DITHER_PATTERNS[index];
            }

            /**
            * Performs a binary dither check against a 4x4 matrix based on screen coordinates.
            * Compares the luminance of the scene color against a pattern threshold to decide visibility.
            *
            * @param uv       The input coordinate.
            * @param scene    The input RGB color.
            * @param pattern  A 4x4 dither matrix.
            * @return         1.0 if the scaled luminance exceeds the threshold, otherwise 0.0.
            */
            inline float GetDitherValue(uint2 uv, float3 scene, float4x4 pattern) 
            {
                uint x = uv.x % 4;
                uint y = uv.y % 4;
                
                float threshold = pattern[x][y];
                return (GetLuminance(scene) * _DitherThreshold > threshold) ? 1.0 : 0.0;
            }

            /**
            * Reduces the color depth of an RGB color into a discrete number of levels.
            *
            * @param color  The input RGB color.
            * @param steps  The number of color levels per channel.
            * @return       The quantized color vector.
            */
            inline float3 Quantize(float3 color, float steps)
            {
                return floor(color * steps) / steps;
            }

            /**
            * Converts screen space coordinates and depth buffer values back into 3D world space.
            *
            * @param uv        The input coordinate.
            * @param rawDepth  The raw depth value from the Depth Texture.
            * @return          The reconstructed 3D position.
            */
            inline float3 ReconstructWorldPosition(float2 uv, float rawDepth)
            {
                float2 screenPos = uv * _ScreenSize.xy;
                float3 positionRWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                float3 absoluteWS = GetAbsolutePositionWS(positionRWS);
                return absoluteWS; 
            }

            /**
            * Computes the scene color with fog at a coordiante.
            *
            * @param uv     The input coordinate.
            * @param scene  The input RGB color.
            * @return       The final RGB color with fog.
            */
            inline float3 CalculateFog(float2 uv, float3 scene) 
            {
                float3 fogColor = scene;
                if(_EnableFog) 
                {
                    // Convert raw depth to linear eye space distance
                    float rawDepth = SampleSceneDepth(uv);
                    float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                    // Exponential squared fog calculation
                    float fogFactor = abs(1.0 - exp2(-linearDepth * _FogDensity));

                    // 3D noise based on world position
                    float3 worldPos = ReconstructWorldPosition(uv, rawDepth);
                    float noise = Perlin3D(worldPos * _FogNoiseScale);

                    // Generate masks to control where noise appears
                    float noiseMask = pow(fogFactor, _FogNoiseStart);
                    float edgeMask = abs(fogFactor * (1.0 - fogFactor) * 4.0); 
                    float speckledMask = pow(edgeMask, _FogEdgeSmoothness); 

                    // Combine fog with noise
                    float dynamicFog = fogFactor + ((noise - 0.5) * _FogNoiseStrength * speckledMask * noiseMask);

                    fogColor = lerp(scene, _FogColor.rgb, saturate(dynamicFog));
                }

                return fogColor;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                float2 pixel = floor(uv * _PixelResolution);
                float2 downsampledUV = (pixel + 0.5) / _PixelResolution;

                float4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, downsampledUV);
                
                uint2 ditherCoord = (uint2)(uv * ((_DitherPixelPerfect == 1) ? _PixelResolution : _ScreenParams.xy / max(1.0, _DitherScale)));
                float4x4 pattern = GetDitherPattern(_DitherPattern);
                float dither = GetDitherValue(ditherCoord, scene.rgb, pattern);

                float3 finalCol = scene.rgb * dither;
                finalCol = Quantize(finalCol, _ColorPrecision);
                finalCol = CalculateFog(downsampledUV, finalCol);

                return float4(finalCol, scene.a);
            }
            ENDHLSL
        }
    }
}