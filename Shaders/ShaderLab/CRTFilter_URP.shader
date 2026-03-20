//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTFilter_URP.shader
//-----------------------------------------------------------------------

Shader "Hidden/CRTFilter_URP"
{
    Properties
    {
        [HideInInspector] _MainTex("InputTex", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "CRTFilterPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_PrevFrameTex);

            int _InterlaceOffset;
            float _DeltaTime;
            float _iTime;

            int _Refresh;
            float _RefreshRate;
            float _DecayRate;
            int _EnableInterlacedRendering;
            int _IsMonochrome;
            
            float _NoiseSpeed, _NoiseScale, _NoiseFade;
            float2 _NoiseRGBOffset;
            int _EnableScreenBend;
            float _ScreenBend, _ScreenRoundness, _VignetteOpacity;
            float2 _ScreenResolution;
            float2 _ScanLineOpacity, _ScanLineSpeed;
            float _ScanLineStrength;
            float _Brightness, _Contrast, _Saturation, _Gamma, _Hue;
            float _RedShift, _BlueShift, _GreenShift;
            float _ChromaticOffset, _ChromaticSpeed;

            float _VHSSmear;          
            float _UnsharpAmount;     
            float _UnsharpRadius;     
            float _UnsharpThreshold;  
            float _ClampBlack;        
            float _ClampWhite;        
            float3 _TintShadowsColor; 
            
            int _EnableTrackerLine;
            float _TrackingSpeed;     
            float _TrackingJitter; 
            int _EnableSignalInterference;
            float _InterferenceFrequency;   
            float _InterferenceAmplitude;

            int _SubPixelMode;
            float _SubPixelDesnity; 

            float _GlitchChance;
            float _GlitchLength;

            /**
            * Pushes UV coordinates outward towards the endge of the screen base on _ScreenBend factor.
            *
            * @param uv  The input coordinate..
            * @return    The distorted UV coordinates.          
            */
            float2 CurveRemap(float2 uv) 
            {
                // Remaps uvs from [0, 1] -> [-1, 1]
                uv = uv * 2.0 - 1.0f;

                // Calculates distortion offset
                float2 offset = abs(uv.yx) / _ScreenBend;
                uv = uv + uv * offset * offset;

                // Remaps uvs from [-1, 1] -> [0, 1]
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            /**
            * Creates a vignette mask based on the screen uv and _VignetteOpacity and _ScreenRoundness.
            *
            * @param uv  The input coordinate.
            * @return    The intensity of the vignette          
            */
            float GetVignetteMask(float2 uv) 
            {
                // Calculates the distance from the edge
                uv *= 1.0 - uv.yx;

                // Scales the distance by opacity
                float vig = abs(uv.x * uv.y * 100.0 * _VignetteOpacity);

                // Apply power function for roundness/falloff control
                return clamp(pow(vig, _ScreenRoundness), 0.0, 1.0);
            } 

            /**
            * Generates a pseudo-random white noise value based on coordinates.
            *
            * @param p      The input coordinate.
            * @param phase  A constant to offset the noise pattern.
            * @param seed   Changes the generated noise map.
            * @return       A random float value in the range [0.0, 1.0].     
            */
            float WhiteNoise(float2 p, float phase, float seed)
            {
                float2 magic = float2(12.9898 + seed, 78.233 + seed);
                return frac(sin(dot(p + phase, magic)) * 43758.5453);
            }

            /**
            * Generates Value Noise by interpolating random values at grid corners.
            *
            * @param uv      The input coordinate.
            * @return        A random valaue in the range [0.0, 1.0].
            */
            float ValueNoise(float2 uv) 
            {
                // Generate noise map seed based on _NoiseSpeed and _RefreshRate
                float seed = fmod(floor(_iTime * _NoiseSpeed * _RefreshRate) / _RefreshRate, 100.0);

                // Fetch current grid cell.
                float2 ip = floor(uv);
                float2 fp = frac(uv);

                // Get random values for the four corners of the current grid cell.
                float a = WhiteNoise(ip, 0, seed);
                float b = WhiteNoise(ip + float2(1.0, 0.0), 0, seed);
                float c = WhiteNoise(ip + float2(0.0, 1.0), 0, seed);
                float d = WhiteNoise(ip + float2(1.0, 1.0), 0, seed);

                // Blends the 4 corner values together
                float2 e = smoothstep(0.0, 1.0, fp);
                return lerp(
                    lerp(a, b, e.x),
                    lerp(c, d, e.x),
                    e.y
                );
            }

            /**
            * Generates a 3-channel chromatic noise effect by offsetting samples per color channel.
            *
            * @param uv      The input coordinate.
            * @return        A float3 containing randomized values for R, G, and B.
            */
            inline float3 GetChromaticNoise(float2 uv) 
            {
                // Scale the coordinates
                float2 noiseUV = uv * 250.0 * _NoiseScale;

                // Shift the UVs for the Red and Blue channels
                float r = ValueNoise(noiseUV + _NoiseRGBOffset);
                float g = ValueNoise(noiseUV);
                float b = ValueNoise(noiseUV - _NoiseRGBOffset);

                float3 finalNoise = float3(r, g, b);

                return finalNoise;
            }

            /**
            * Generates a scanline modulation factor based on screen coordinates and time.
            *
            * @param uv      The input coordinate.
            * @return        A scaling factor used to darken or brighten pixels to simulate CRT scanlines.
            */
            inline float GetScanlineFactor(float2 uv) 
            {
                // Calculates a value from a 2D sin wave between [-1, 1]. 
                float2 intensity = float2(
                    sin(uv.x * _ScreenResolution.x + _iTime * _ScanLineSpeed.x * 100.0), 
                    cos(uv.y * _ScreenResolution.y + _iTime * _ScanLineSpeed.y * 100.0)
                ) * _ScanLineStrength;

                // Remaps value from [-1, 1] -> [0, 1]
                intensity = (0.5 * intensity) + 0.5;

                //  Weighs the individual scanline contribution 
                float scanVal = dot(intensity, _ScanLineOpacity);

                // Normalization factor
                float weight = 0.5 * (_ScanLineOpacity.x + _ScanLineOpacity.y);

                return (weight > 0.0) ? scanVal / weight : 1.0;
            }

            /**
             * Determines whether the current pixel should be updated.
             *
             * @param uv    The input coordinate.
             * @return      True if the pixel should be refreshed; false if it should retain previous data.
             */
            inline bool ShouldRefresh(float2 uv) 
            {
                // Identify if the current row is even or odd, adjusted by a frame-toggled offset
                float lin = floor(uv.y * _ScreenResolution.y);
                bool isInteralcedRow = ((int(lin) + _InterlaceOffset) & 1) == 0;

                // Refresh if: 
                // 1. Interlacing is OFF and it is the active row
                // 2. CRT is in a refresh frame (_Refresh = 1)
                return !((isInteralcedRow && _EnableInterlacedRendering) || _Refresh == 0);
            }

            /**
             * Retrieves the color from the previous frame and applies a time-based 
             * exponential decay to simulate phosphorus persistence.
             *
             * @param uv    The input coordinate.
             * @return      The decayed RGB color value from the previous frame.
             */
            inline float3 GetLastFrameColor(float2 uv) 
            {
                float3 prevFrame = SAMPLE_TEXTURE2D(_PrevFrameTex, sampler_LinearClamp, uv).rgb;
                float decay = pow(abs(_DecayRate), _DeltaTime);
                return prevFrame * decay;
            }
            
            /**
             * Calculates a horizontal displacement to simulate signal interference.
             *
             * @param uv      The input coordinate.
             * @return        A horizontal offset value for the UV coordinates.
             */
            inline float CalculateInterference(float2 uv) 
            {
                // Divide the screen into discrete horizontal bands based on frequency.
                float phaseNumber = floor(uv.y * _InterferenceFrequency / _ScreenResolution.y);
                
                // Generate a random value for each band.
                float offsetNoiseModifier = WhiteNoise(float2(phaseNumber + 1, phaseNumber), 0.0, 10.0);
                
                // Calculate the offset.
                float offsetUV = sin((uv.y + frac(_iTime * 0.05)) * 6.28318 * _InterferenceFrequency) * (0.002 * offsetNoiseModifier);
                
                // Scales the offset. 
                return offsetUV * _InterferenceAmplitude * _EnableSignalInterference;
            }

            /**
             * Resamples UV coordinates to snap them to the nearest pixel grid.
             *
             * @param rawUV The original UV coordinate.
             * @return      The UV coordinate snapped to the screen resolution.
             */
            inline float2 CalculateResampledUVs(float2 rawUV) 
            {
                return floor(rawUV * _ScreenResolution) / _ScreenResolution;
            }

            /**
             * Calculates a horizontal UV offset to simulate analog VCR tracking errors.
             *
             * @param uv    The input coordinate.
             * @return      A horizontal offset value for the UV coordinates.
             */
            inline float CalculateScanlineShift(float2 uv)
            {
                // Create a normalized time factor (0 to 1)
                float t = 1.0 - fmod(_iTime, _TrackingSpeed) / _TrackingSpeed;
                
                // Map the time factor to a specific vertical pixel position on the screen
                float trackingStart = fmod(t * _ScreenResolution.y, _ScreenResolution.y);
                
                // Add high-frequency randomness to the bar's position
                float trackingJitter = WhiteNoise(float2(5000.0, 5000.0), 0.0, frac(_iTime)) * _TrackingJitter;
                trackingStart += trackingJitter;

                // Determine if the current pixel's vertical position is below the tracking line
                float isBelow = step(trackingStart, uv.y * _ScreenResolution.y);
                
                float offset = 0.003;
                return (isBelow * offset) * _EnableTrackerLine;
            }

            /**
             * Generates a adding bright, noisy blocks at random vertical positions.
             *
             * @param originalSceneColor  The current pixel's color.
             * @param uv                  The input coordinate.
             * @return                    The modified scene color with potential glitch artifacts.
             */
            inline float3 CalculateGlitchArtifact(float3 originalSceneColor, float2 uv) 
            {
                float3 scene = originalSceneColor;

                // Determine if a glitch line should trigger for this frame
                if(WhiteNoise(float2(600.0, 500.0), 0.0, frac(_iTime * 20.0)) > 1.0 - _GlitchChance)
                {
                    // Pick a random y coordinate for the line to appear
                    float randomY = WhiteNoise(float2(800.0, 50.0), 0.0, frac(_iTime));
                    float lineStart = floor(randomY * _ScreenResolution.y);
        
                    float thickness = 2.0; 
                    float currentPixelY = uv.y * _ScreenResolution.y;

                    // Check if the current pixel falls within the vertical thickness of the glitch line
                    if(currentPixelY >= lineStart && currentPixelY < lineStart + thickness)
                    {
                        // Calculate randomized horizontal block scaling
                        float lengthVar = WhiteNoise(float2(lineStart, 123.45), 0.0, frac(_iTime * 2.0));
                        float horizontalScale = _GlitchLength * lerp(0.5, 1.0, lengthVar);
                        float xBlock = floor(uv.x * horizontalScale);
            
                        // Create blocky noise patterns along the horizontal line
                        float blockNoise = WhiteNoise(float2(xBlock, lineStart), 0.0, frac(_iTime * 5.0));
            
                        if(blockNoise > 0.8) 
                        {
                            // Apply high frequency grit to the block
                            float xSeed = floor(uv.x * _ScreenResolution.x);
                            float grit = WhiteNoise(float2(xSeed, lineStart), 0.0, frac(_iTime * 15.0));

                            // Applies the artifact
                            scene.rgb += max(0.8 - grit, 0.0) * 2.0; 
                        }
                    }
                }

                return scene;
            }

            /**
             * Rotates the hue of an RGB color using the Rodrigues' rotation formula.
             *
             * @param aColor  The input RGB color.
             * @param aHue    The rotation angle in degrees.
             * @return        The color with the shifted hue.
             */
            inline half3 ApplyHue(half3 aColor, float aHue)
            {
                half angle = radians(aHue);

                // The normalized vector (1,1,1) / sqrt(3)
                half3 k = half3(0.57735, 0.57735, 0.57735);
                
                half cosAngle = cos(angle);

                // Rodrigues' rotation formula: Vrot = V*cos(th) + (K x V)*sin(th) + K*(K . V)*(1 - cos(th))
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1.0 - cosAngle);
            }

            /**
             * Applies a Hue, Saturation, Brightness and 
             * grading effects including Contrast, Gamma, and Saturation.
             *
             * @param startColor  The input RGB color.
             * @return            The final processed color.
             */
            inline half3 ApplyHSBEffect(half3 startColor)
            {
                // Remap normalized input parameters
                float hue = 360.0 * _Hue;
                float brightness = _Brightness * 2.0 - 1.0; 
                float contrast = _Contrast * 2.0;           
                float saturation = _Saturation * 2.0;       
                float gamma = (_Gamma < 0.5) 
                    ? lerp(0.1, 1.0, _Gamma * 2.0) 
                    : lerp(1.0, 4.0, (_Gamma - 0.5) * 2.0);

                // Shift Hue using Rodrigues' rotation formula
                half3 outputColor = ApplyHue(startColor, hue);

                // Adjust Contrast (scales around the 0.5 middle point)
                outputColor = (outputColor - 0.5f) * contrast + 0.5f;

                // Adjust Brightness
                outputColor = outputColor + brightness;

                // Apply Gamma Correction
                outputColor = pow(max(outputColor, 0.0), 1.0 / max(gamma, 0.01));

                // Adjust Saturation by interpolating between grayscale and color
                half3 intensity = dot(outputColor, half3(0.299, 0.587, 0.114));
                outputColor = lerp(intensity, outputColor, saturation);

                return saturate(outputColor);
            }

            /**
             * Enhances image sharpness by isolating high-frequency details and amplifying them.
             *
             * @param col        The input RGB color.
             * @param uv         The input coordinate.
             * @param amount     The strength of the sharpening effect.
             * @param radius     The distance (in pixels) used to sample the surrounding blur.
             * @param threshold  The minimum luminance difference required to apply sharpening.
             * @return           The sharpened color.
             */
            inline float3 UnsharpMask(float3 col, float2 uv, float amount, float radius, float threshold)
            {
                // Calculate the size of a single pixel relative to the screen resolution
                float2 texel = 1.0 / _ScreenResolution;
                
                // Create a simple 4-tap box blur by sampling corners around the pixel
                float3 blur = 0;
                blur += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(-radius, -radius) * texel).rgb;
                blur += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2( radius, -radius) * texel).rgb;
                blur += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(-radius,  radius) * texel).rgb;
                blur += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2( radius,  radius) * texel).rgb;
                blur *= 0.25; // Average the four samples

                // Isolate the detail
                float3 detail = col - blur;

                // Apply a noise threshold so subtle gradients aren't over-sharpened
                float lumDiff = abs(dot(detail, float3(0.299, 0.587, 0.114)));
                if(lumDiff < threshold) detail = float3(0, 0, 0);

                // Add the weighted detail back to the original image
                col += detail * amount;

                return saturate(col);
            }

            /**
             * Restricts the color components to a specified range.
             *
             * @param col         The input RGB color.
             * @param blackLevel  The minimum allowable value.
             * @param whiteLevel  The maximum allowable value.
             * @return            The color restricted to the [blackLevel, whiteLevel].
             */
            inline float3 ClampLevels(float3 col, float blackLevel, float whiteLevel)
            {
                col.rgb = clamp(col, blackLevel, whiteLevel);
                return col;
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
             * Applies a color tint to the darker regions of the image.
             *
             * @param col          The input RGB color.
             * @param shadowColor  The color to be multiplied into the shadow regions.
             * @return             The color with tinted shadows.
             */
            inline float3 TintShadows(float3 col, float3 shadowColor)
            {
                float lum = GetLuminance(col);
                float shadowMask = smoothstep(0.0, 0.3, 0.3 - lum); 
                col = lerp(col, col * shadowColor, shadowMask);
                return col;
            }

            /**
             * Simulates VHS style horizontal blurring and signal degradation.
             *
             * @param uv       The original texture coordinates.
             * @param ratio    The resolution reduction factor.
             * @param xOffset  Horizontal shift for the signal.
             * @return         The color at the shifted, quantized, and interpolated coordinate.
             */
            inline float3 GetVHSColor(float2 uv, float ratio, float xOffset)
            {
                // Calculate the width of a single band in UV space
                float bandWidth = 1.0 / max(_ScreenResolution.x * ratio, 1e-5);

                // Apply the horizontal offset to the current coordinate
                float2 shiftedUV = float2(uv.x + xOffset, uv.y);

                // Quantize the X coordinate to find the pixel boundaries
                float quantizedX = floor(shiftedUV.x / bandWidth) * bandWidth;
                float2 uvA = float2(quantizedX, shiftedUV.y);
                float2 uvB = float2(quantizedX + bandWidth, shiftedUV.y);

                // Calculate the interpolation weight between the two bands
                float t = (shiftedUV.x - quantizedX) / bandWidth;

                // Sample the texture at the two nearest horizontal quantized points
                float3 colA = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvA).rgb;
                float3 colB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvB).rgb;

                return lerp(colA, colB, t);
            }

            /**
             * Clamps out-of-range colors while preserving the original hue and luminance.
             *
             * @param    The input RGB color.
             * @return   The color remapped into the valid range.
             */
            inline float3 ClipColor(float3 c) {
                float l = GetLuminance(c);
                float n = min(min(c.r, c.g), c.b);
                float x = max(max(c.r, c.g), c.b);
                if (n < 0.0) c = l + (((c - l) * l) / (l - n));
                if (x > 1.0) c = l + (((c - l) * (1.0 - l)) / (x - l));
                return c;
            }

            /**
             * Adjusts the color to a specific target luminance while preserving hue and saturation.
             *
             * @param c  The input RGB color.
             * @param l  The target luminance value.
             * @return   A color with the new luminance.
             */
            inline float3 SetLum(float3 c, float l) {
                float d = l - GetLuminance(c);
                c += d;
                return ClipColor(c);
            }

            /**
             * Combining smear effects with chromatic aberration.
             *
             * @param scene  The current scene color.
             * @param uv     The input coordinates.
             * @return       A reconstructed color where RGB channels are horizontally misaligned.
             */
            inline float3 CalculateVHSDamage(float3 scene, float2 uv)
            {
                float chromOffset = _ChromaticOffset * cos(_iTime * _ChromaticSpeed) / 10.0;

                float colVHSR = GetVHSColor(uv, _VHSSmear, -chromOffset).r;
                float colVHSG = GetVHSColor(uv, _VHSSmear,  chromOffset).g;
                float colVHSB = GetVHSColor(uv, _VHSSmear, 0.0).b;
                return float3(colVHSR, colVHSG, colVHSB);
            }

            /**
             * Performs a color shift while preserving luminance.
             *
             * @param scene  The input RGB color.
             * @return       The color with shifted tones.
             */
            inline float3 ApplyColorShift(float3 scene)
            {
                float3 shiftedCol = scene;
                shiftedCol.r *= (1.0 + _RedShift);
                shiftedCol.g *= (1.0 + _GreenShift);
                shiftedCol.b *= (1.0 + _BlueShift);

                float originalLuminance = GetLuminance(scene);

                return SetLum(shiftedCol, originalLuminance);
            }

            /**
             * Simulates the physical subpixel structure of various CRT monitors.
             *
             * @param uv       The input coordinate.
             * @param col      The input RGB color.
             * @param density  The frequency of the mask.
             * @param mode     The CRT type: 1 = Shadow Mask, 2 = Aperture Grille, 3 = Slot Mask.
             * @return         The color multiplied by the simulated phosphor grid.
             */
            inline float3 ApplyCRTSubPixels(float2 uv, float3 col, float density, int mode)
            {
                float aspect = _ScreenResolution.x / _ScreenResolution.y;
                float3 finalColor = col;

                if (mode == 1) 
                {
                    // Shadow Mask

                    // Creates a staggered grid of circular phosphor dots.
                    float2 gv = uv * float2(aspect * density, density * 1.732);
                    float row = floor(gv.y);
                    
                    // Stagger every other row to create the hexagonal triad pattern
                    gv.x += (frac(row * 0.5) > 0.0) ? 0.5 : 0.0;

                    float2 f = frac(gv);
                    // Determine if the current dot is Red, Green, or Blue
                    int index = int(fmod(floor(gv.x) + floor(gv.y) * 1.5, 3.0));
                    float3 maskColor = float3(index == 0, index == 1, index == 2);

                    // Create the circular shape for the phosphor dot
                    float dist = distance(f, float2(0.5, 0.5));
                    float dot = smoothstep(0.45, 0.35, dist);

                    finalColor = col * maskColor * dot * 2.0;
                }
                else if (mode == 2) 
                {
                    // Aperture Grille

                    float2 gv = uv * float2(aspect * density, density);
                    int index = int(fmod(floor(gv.x), 3.0));
                    float3 maskColor = float3(index == 0, index == 1, index == 2);

                    finalColor = col * maskColor;
                }
                else if (mode == 3) 
                {
                    // Slot Mask

                    float2 gv = uv * float2(aspect * density, density * 0.5); 
        
                    float row = floor(gv.y);

                    // Stagger the columns per row
                    gv.x += (frac(row * 0.5) > 0.0) ? 0.5 : 0.0;

                    float2 f = frac(gv);
                    int index = int(fmod(floor(gv.x) + floor(gv.y), 3.0));
                    float3 maskColor = float3(index == 0, index == 1, index == 2);

                    // Shape the mask into elongated vertical rectangles
                    float2 distScale = float2(1.5, 1.0); 
                    float dist = length((f - 0.5) * distScale);
                    float brick = smoothstep(0.45, 0.35, dist);

                    finalColor = col * maskColor * brick * 3.0;
                }

                return finalColor;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                if (!ShouldRefresh(IN.texcoord)) 
                {
                    return float4(GetLastFrameColor(IN.texcoord), 1.0);
                }

                float2 uv = (_EnableScreenBend == 1) ? CurveRemap(IN.texcoord) : IN.texcoord;

                uv.x += CalculateInterference(uv);
                uv.x += CalculateScanlineShift(uv);
                uv = CalculateResampledUVs(uv);

                if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0) 
                {
                    return float4(0.0, 0.0, 0.0, 1.0);
                } 

                float3 scene = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;

                float3 combinedVHS = CalculateVHSDamage(scene, uv);

                float3 unsharpCol = UnsharpMask(scene, uv, _UnsharpAmount, _UnsharpRadius, _UnsharpThreshold).rgb;

                float3 col =  SetLum(combinedVHS, GetLuminance(unsharpCol));

                col = ApplyHSBEffect(col);
                col = ClampLevels(col, _ClampBlack, _ClampWhite);
                col = TintShadows(col, _TintShadowsColor);

                col = CalculateGlitchArtifact(col, uv);
                col = lerp(col, col * GetChromaticNoise(uv), _NoiseFade);

                if (_IsMonochrome == 1) 
                {
                    col.rgb = dot(col.rgb, float3(0.2125, 0.7154, 0.0721));
                }

                col = ApplyColorShift(col);

                col = ApplyCRTSubPixels(uv, col, _SubPixelDesnity, _SubPixelMode);

                // TODO: Should be in curved coordinates but using uv causes Moire artifacting.
                col *= GetScanlineFactor(IN.texcoord);

                col *= GetVignetteMask(uv);

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}