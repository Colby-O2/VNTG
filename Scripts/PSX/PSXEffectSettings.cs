using UnityEngine;
using UnityEngine.Rendering;

using ColbyO.VNTG.ColorPalette;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectSettings.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    [System.Serializable, VolumeComponentMenu("VNTG/PSX Effect Settings")]
    public class PSXEffectSettings : VolumeComponent, IPostProcessComponent
    {
        // Settings
        public BoolParameter Enabled = new BoolParameter(true, overrideState: true);
        public BoolParameter ShowInSceneView = new BoolParameter(false, overrideState: true);

        // Lighting
        public ColorParameter AmbientColor = new ColorParameter(Color.black, overrideState: true);

        // Pixelation
        public BoolParameter EnablePixelation = new BoolParameter(true, overrideState: true);
        public BoolParameter EnableLetterbox = new BoolParameter(false, overrideState: true);
        public Vector2Parameter PixelResolution = new Vector2Parameter(new Vector2(256.0f, 256.0f), overrideState: true);

        // Color Precision
        public BoolParameter EnableColorPrecision = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter ColorPrecision = new ClampedFloatParameter(32f, 1f, 256f, overrideState: true);

        // Dither
        public BoolParameter EnableDither = new BoolParameter(true);
        public EnumParameter<PSXDitherMode> DitherMode = new EnumParameter<PSXDitherMode>(PSXDitherMode.Additive, overrideState: true);
        public ClampedIntParameter DitherPattern = new ClampedIntParameter(1, 0, 10, overrideState: true);
        public EnumParameter<PSXDitherDisplayMode> DitherDisplayMode = new EnumParameter<PSXDitherDisplayMode>(PSXDitherDisplayMode.AbsoluteScale, overrideState: true);
        public Vector2Parameter DitherReferenceRes = new Vector2Parameter(new Vector2(1920, 1080), overrideState: true);
        public ClampedFloatParameter DitherScale = new ClampedFloatParameter(0.2f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter DitherThreshold = new ClampedFloatParameter(0.1f, 0f, 1f, overrideState: true);

        // Color Palette 
        public BoolParameter EnableColorPalette  = new BoolParameter(false, overrideState: true);
        public BoolParameter PreserveLighting = new BoolParameter(false, overrideState: true);
        public BoolParameter NormalizeLuminanceBeforeSampling = new BoolParameter(false, overrideState: true);
        public PaletteAssetParameter PaletteAsset = new PaletteAssetParameter(null, overrideState: true);
        
        // Fog
        public BoolParameter EnableFog = new BoolParameter(false, overrideState: true);
        public BoolParameter IgnoreSkybox = new BoolParameter(false, overrideState: true);
        public ColorParameter FogColor = new ColorParameter(Color.black, overrideState: true);
        public ClampedFloatParameter FogDensity = new ClampedFloatParameter(1.0f, 0.0f, 1.0f, overrideState: true);
        public ClampedFloatParameter FogNoiseStrength = new ClampedFloatParameter(0.1f, 0.0f, 1.0f, overrideState: true);
        public ClampedFloatParameter FogEdgeSmoothness = new ClampedFloatParameter(0.5f, 0.01f, 1.0f, overrideState: true);
        public ClampedFloatParameter FogNoiseScale = new ClampedFloatParameter(0.5f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter FogNoiseStart = new ClampedFloatParameter(0.7f, 0f, 1f, overrideState: true);

        public bool IsActive() => Enabled.value;
        public bool IsTileCompatible() => false;

        private void OnValidate()
        {
            if (Enabled != null) Enabled.overrideState = true;
            if (ShowInSceneView != null) ShowInSceneView.overrideState = true;

            if (EnablePixelation != null) EnablePixelation.overrideState = true;
            if (EnableLetterbox != null) EnableLetterbox.overrideState = true;
            if (PixelResolution != null) PixelResolution.overrideState = true;

            if (EnableColorPrecision != null) EnableColorPrecision.overrideState = true;
            if (ColorPrecision != null) ColorPrecision.overrideState = true;

            if (EnableDither != null) EnableDither.overrideState = true;
            if (DitherMode != null) DitherMode.overrideState = true;
            if (DitherPattern != null) DitherPattern.overrideState = true;
            if (DitherDisplayMode != null) DitherDisplayMode.overrideState = true;
            if (DitherReferenceRes != null) DitherReferenceRes.overrideState = true;
            if (DitherScale != null) DitherScale.overrideState = true;
            if (DitherThreshold != null) DitherThreshold.overrideState = true;

            if (EnableColorPalette != null) EnableColorPalette.overrideState = true;
            if (PreserveLighting != null) PreserveLighting.overrideState = true;
            if (NormalizeLuminanceBeforeSampling != null) NormalizeLuminanceBeforeSampling.overrideState = true;
            if (PaletteAsset != null) PaletteAsset.overrideState = true;

            if (EnableFog != null) EnableFog.overrideState = true;
            if (IgnoreSkybox != null) IgnoreSkybox.overrideState = true;
            if (FogColor != null) FogColor.overrideState = true;
            if (FogDensity != null) FogDensity.overrideState = true;
            if (FogNoiseStrength != null) FogNoiseStrength.overrideState = true;
            if (FogEdgeSmoothness != null) FogEdgeSmoothness.overrideState = true;
            if (FogNoiseScale != null) FogNoiseScale.overrideState = true;
            if (FogNoiseStart != null) FogNoiseStart.overrideState = true;
        }
    }
}