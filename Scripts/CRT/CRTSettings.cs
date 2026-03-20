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
        [Header("Settings")]
        public BoolParameter Enabled = new BoolParameter(true);
        public BoolParameter ShowInSceneView = new BoolParameter(false);

        [Header("Screen")]
        public Vector2Parameter ScreenResolution = new Vector2Parameter(new Vector2(640f, 480f));
        public ClampedIntParameter RefreshRate = new ClampedIntParameter(50, 0, 360);
        public ClampedFloatParameter DecayRate = new ClampedFloatParameter(0.9f, 0f, 1f);
        public BoolParameter EnableInterlacedRendering = new BoolParameter(true);

        [Header("Screen Shape")]
        public BoolParameter EnableScreenBend = new BoolParameter(true);
        public ClampedFloatParameter ScreenBend = new ClampedFloatParameter(4f, 0f, 100f);
        public ClampedFloatParameter ScreenRoundness = new ClampedFloatParameter(1f, 0f, 1f);
        public ClampedFloatParameter VignetteOpacity = new ClampedFloatParameter(1f, 0f, 1f);

        [Header("Scanlines")]
        public ClampedFloatParameter ScanLineVerticalOpacity = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter ScanLineHorizontalOpacity = new ClampedFloatParameter(1f, 0f, 1f);
        public ClampedFloatParameter ScanLineVerticalSpeed = new ClampedFloatParameter(0.1f, -1f, 1f);
        public ClampedFloatParameter ScanLineHorizontalSpeed = new ClampedFloatParameter(-0.2f, -1f, 1f);
        public ClampedFloatParameter ScanLineStrength = new ClampedFloatParameter(0.2f, 0f, 1f);

        [Header("Noise")]
        public ClampedFloatParameter NoiseScale = new ClampedFloatParameter(0.9f, 0f, 1f);
        public ClampedFloatParameter NoiseRBGOffsetX = new ClampedFloatParameter(0.4f, 0f, 1f);
        public ClampedFloatParameter NoiseRBGOffsetY = new ClampedFloatParameter(0.7f, 0f, 1f);
        public ClampedFloatParameter NoiseSpeed = new ClampedFloatParameter(0.1f, 0f, 1f);
        public ClampedFloatParameter NoiseFade = new ClampedFloatParameter(0.55f, 0f, 1f);

        [Header("VHS")]
        public ClampedFloatParameter VhsSmear = new ClampedFloatParameter(0.95f, 0.01f, 1f);

        [Header("Tracking")]
        public BoolParameter EnableTrackerLine = new BoolParameter(true);
        public ClampedFloatParameter TrackingSpeed = new ClampedFloatParameter(4f, 1f, 20f);
        public ClampedFloatParameter TrackingJitter = new ClampedFloatParameter(4f, 0f, 50f);
        public ClampedFloatParameter GlitchChance = new ClampedFloatParameter(0.2f, 0f, 1f);
        public ClampedFloatParameter GlitchLength = new ClampedFloatParameter(15f, 0f, 30f);

        [Header("Signal Interference")]
        public BoolParameter EnableSignalInterference = new BoolParameter(true);
        public ClampedFloatParameter InterferenceFrequency = new ClampedFloatParameter(70f, 0f, 200f);
        public ClampedFloatParameter InterferenceAmplitude = new ClampedFloatParameter(0.25f, 0f, 1f);

        [Header("Subpixels")]
        public ClampedFloatParameter ChromaticOffset = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter ChromaticOffsetSpeed = new ClampedFloatParameter(0.1f, 0f, 1f);

        [Header("Chromatic Aberration")]
        public EnumParameter<CRTSubPixelMode> SubPixelMode = new EnumParameter<CRTSubPixelMode>(CRTSubPixelMode.None);
        public ClampedFloatParameter SubPixelDensity = new ClampedFloatParameter(100f, 100f, 400f);

        [Header("Post-Process")]
        public ClampedFloatParameter UnsharpAmount = new ClampedFloatParameter(1.5f, 0f, 5f);
        public ClampedFloatParameter UnsharpRadius = new ClampedFloatParameter(1f, 0f, 5f);
        public ClampedFloatParameter UnsharpThreshold = new ClampedFloatParameter(0.02f, 0f, 0.5f);
        public ClampedFloatParameter ClampBlack = new ClampedFloatParameter(0.05f, 0f, 1f);
        public ClampedFloatParameter ClampWhite = new ClampedFloatParameter(0.95f, 0f, 1f);
        public ColorParameter ShadowTint = new ColorParameter(new Color(0.7f, 0.6f, 0.9f));

        [Header("Color Adjustment")]
        public ClampedFloatParameter Gamma = new ClampedFloatParameter(0.374f, 0f, 1f);
        public ClampedFloatParameter Brightness = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter Contrast = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter Saturation = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter Hue = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter RedShift = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter BlueShift = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter GreenShift = new ClampedFloatParameter(0f, 0f, 1f);
        public BoolParameter IsMonochrome = new BoolParameter(false);

        public bool IsActive() => Enabled.value;
        public bool IsTileCompatible() => false;
    }
}
