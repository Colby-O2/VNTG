using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTSettings.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT
{
    [System.Serializable, VolumeComponentMenu("VNTG/CRT Settings")]
    public class CRTSettings : VolumeComponent, IPostProcessComponent
    {
        // Settings
        public BoolParameter Enabled = new BoolParameter(true, overrideState: true);
        public BoolParameter ShowInSceneView = new BoolParameter(false, overrideState: true);

        // Screen
        public BoolParameter EnableScreenSettings = new BoolParameter(true, overrideState: true);
        public Vector2Parameter ScreenResolution = new Vector2Parameter(new Vector2(640f, 480f), overrideState: true);
        public BoolParameter UseMaxFPS = new BoolParameter(false, overrideState: true);
        public ClampedIntParameter RefreshRate = new ClampedIntParameter(50, 0, 360, overrideState: true);
        public ClampedFloatParameter DecayRate = new ClampedFloatParameter(0.9f, 0f, 1f, overrideState: true);
        public BoolParameter EnableInterlacedRendering = new BoolParameter(true, overrideState: true);

        // Screen Shape
        public BoolParameter EnableScreenBend = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter ScreenBend = new ClampedFloatParameter(4f, 0f, 100f, overrideState: true);

        // Vignette
        public BoolParameter EnableVignette = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter ScreenRoundness = new ClampedFloatParameter(1f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter VignetteOpacity = new ClampedFloatParameter(1f, 0f, 1f, overrideState: true);

        // Scanlines
        public BoolParameter EnableScanlines = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter ScanLineVerticalOpacity = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter ScanLineHorizontalOpacity = new ClampedFloatParameter(1f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter ScanLineVerticalSpeed = new ClampedFloatParameter(0.1f, -1f, 1f, overrideState: true);
        public ClampedFloatParameter ScanLineHorizontalSpeed = new ClampedFloatParameter(-0.2f, -1f, 1f, overrideState: true);
        public ClampedFloatParameter ScanLineStrength = new ClampedFloatParameter(0.2f, 0f, 1f, overrideState: true);

        // Noise
        public BoolParameter EnableNoise = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter NoiseScale = new ClampedFloatParameter(0.9f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter NoiseRBGOffsetX = new ClampedFloatParameter(0.4f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter NoiseRBGOffsetY = new ClampedFloatParameter(0.7f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter NoiseSpeed = new ClampedFloatParameter(0.1f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter NoiseFade = new ClampedFloatParameter(0.55f, 0f, 1f, overrideState: true);

        // VHS
        public BoolParameter EnableVhs = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter VhsSmear = new ClampedFloatParameter(0.95f, 0.01f, 1f, overrideState: true);

        // Tracking
        public BoolParameter EnableTrackerLine = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter TrackingSpeed = new ClampedFloatParameter(4f, 1f, 20f, overrideState: true);
        public ClampedFloatParameter TrackingJitter = new ClampedFloatParameter(4f, 0f, 50f, overrideState: true);

        // Glitch
        public BoolParameter EnableGlitch = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter GlitchChance = new ClampedFloatParameter(0.2f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter GlitchLength = new ClampedFloatParameter(15f, 0f, 30f, overrideState: true);

        // Signal Interference
        public BoolParameter EnableSignalInterference = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter InterferenceFrequency = new ClampedFloatParameter(70f, 0f, 200f, overrideState: true);
        public ClampedFloatParameter InterferenceAmplitude = new ClampedFloatParameter(0.25f, 0f, 1f, overrideState: true);

        // Chromatic Aberration
        public BoolParameter EnableChromaticAberration = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter ChromaticOffset = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter ChromaticOffsetSpeed = new ClampedFloatParameter(0.1f, 0f, 1f, overrideState: true);

        // Subpixels
        public BoolParameter EnableSubPixels = new BoolParameter(true, overrideState: true);
        public EnumParameter<CRTSubPixelMode> SubPixelMode = new EnumParameter<CRTSubPixelMode>(CRTSubPixelMode.None, overrideState: true);
        public ClampedFloatParameter SubPixelDensity = new ClampedFloatParameter(100f, 100f, 400f, overrideState: true);

        // Unsharp
        public BoolParameter EnableUnsharp = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter UnsharpAmount = new ClampedFloatParameter(1.5f, 0f, 5f, overrideState: true);
        public ClampedFloatParameter UnsharpRadius = new ClampedFloatParameter(1f, 0f, 5f, overrideState: true);
        public ClampedFloatParameter UnsharpThreshold = new ClampedFloatParameter(0.02f, 0f, 0.5f, overrideState: true);

        // Color Clamp
        public BoolParameter EnableClampColor = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter ClampBlack = new ClampedFloatParameter(0.05f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter ClampWhite = new ClampedFloatParameter(0.95f, 0f, 1f, overrideState: true);
        public ColorParameter ShadowTint = new ColorParameter(new Color(0.7f, 0.6f, 0.9f), overrideState: true);

        // Color Adjustment
        public BoolParameter EnableColorAdjustment = new BoolParameter(true, overrideState: true);
        public ClampedFloatParameter Gamma = new ClampedFloatParameter(0.374f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter Brightness = new ClampedFloatParameter(0.5f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter Contrast = new ClampedFloatParameter(0.5f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter Saturation = new ClampedFloatParameter(0.5f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter Hue = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter RedShift = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter BlueShift = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);
        public ClampedFloatParameter GreenShift = new ClampedFloatParameter(0f, 0f, 1f, overrideState: true);
        public BoolParameter IsMonochrome = new BoolParameter(false, overrideState: true);

        public bool IsActive() => Enabled.value;
        public bool IsTileCompatible() => false;

        private void OnValidate()
        {
            if (Enabled != null) Enabled.overrideState = true;
            if (ShowInSceneView != null) ShowInSceneView.overrideState = true;

            if (EnableScreenSettings != null) EnableScreenSettings.overrideState = true;
            if (ScreenResolution != null) ScreenResolution.overrideState = true;
            if (UseMaxFPS != null) UseMaxFPS.overrideState = true;
            if (RefreshRate != null) RefreshRate.overrideState = true;
            if (DecayRate != null) DecayRate.overrideState = true;
            if (EnableInterlacedRendering != null) EnableInterlacedRendering.overrideState = true;

            if (EnableScreenBend != null) EnableScreenBend.overrideState = true;
            if (ScreenBend != null) ScreenBend.overrideState = true;

            if (EnableVignette != null) EnableVignette.overrideState = true;
            if (ScreenRoundness != null) ScreenRoundness.overrideState = true;
            if (VignetteOpacity != null) VignetteOpacity.overrideState = true;

            if (EnableScanlines != null) EnableScanlines.overrideState = true;
            if (ScanLineVerticalOpacity != null) ScanLineVerticalOpacity.overrideState = true;
            if (ScanLineHorizontalOpacity != null) ScanLineHorizontalOpacity.overrideState = true;
            if (ScanLineVerticalSpeed != null) ScanLineVerticalSpeed.overrideState = true;
            if (ScanLineHorizontalSpeed != null) ScanLineHorizontalSpeed.overrideState = true;
            if (ScanLineStrength != null) ScanLineStrength.overrideState = true;

            if (EnableNoise != null) EnableNoise.overrideState = true;
            if (NoiseScale != null) NoiseScale.overrideState = true;
            if (NoiseRBGOffsetX != null) NoiseRBGOffsetX.overrideState = true;
            if (NoiseRBGOffsetY != null) NoiseRBGOffsetY.overrideState = true;
            if (NoiseSpeed != null) NoiseSpeed.overrideState = true;
            if (NoiseFade != null) NoiseFade.overrideState = true;

            if (EnableVhs != null) EnableVhs.overrideState = true;
            if (VhsSmear != null) VhsSmear.overrideState = true;

            if (EnableTrackerLine != null) EnableTrackerLine.overrideState = true;
            if (TrackingSpeed != null) TrackingSpeed.overrideState = true;
            if (TrackingJitter != null) TrackingJitter.overrideState = true;

            if (EnableGlitch != null) EnableGlitch.overrideState = true;
            if (GlitchChance != null) GlitchChance.overrideState = true;
            if (GlitchLength != null) GlitchLength.overrideState = true;

            if (EnableSignalInterference != null) EnableSignalInterference.overrideState = true;
            if (InterferenceFrequency != null) InterferenceFrequency.overrideState = true;
            if (InterferenceAmplitude != null) InterferenceAmplitude.overrideState = true;

            if (EnableChromaticAberration != null) EnableChromaticAberration.overrideState = true;
            if (ChromaticOffset != null) ChromaticOffset.overrideState = true;
            if (ChromaticOffsetSpeed != null) ChromaticOffsetSpeed.overrideState = true;

            if (EnableSubPixels != null) EnableSubPixels.overrideState = true;
            if (SubPixelMode != null) SubPixelMode.overrideState = true;
            if (SubPixelDensity != null) SubPixelDensity.overrideState = true;

            if (EnableUnsharp != null) EnableUnsharp.overrideState = true;
            if (UnsharpAmount != null) UnsharpAmount.overrideState = true;
            if (UnsharpRadius != null) UnsharpRadius.overrideState = true;
            if (UnsharpThreshold != null) UnsharpThreshold.overrideState = true;

            if (EnableClampColor != null) EnableClampColor.overrideState = true;
            if (ClampBlack != null) ClampBlack.overrideState = true;
            if (ClampWhite != null) ClampWhite.overrideState = true;
            if (ShadowTint != null) ShadowTint.overrideState = true;

            if (EnableColorAdjustment != null) EnableColorAdjustment.overrideState = true;
            if (Gamma != null) Gamma.overrideState = true;
            if (Brightness != null) Brightness.overrideState = true;
            if (Contrast != null) Contrast.overrideState = true;
            if (Saturation != null) Saturation.overrideState = true;
            if (Hue != null) Hue.overrideState = true;
            if (RedShift != null) RedShift.overrideState = true;
            if (BlueShift != null) BlueShift.overrideState = true;
            if (GreenShift != null) GreenShift.overrideState = true;
            if (IsMonochrome != null) IsMonochrome.overrideState = true;
        }
    }
}
