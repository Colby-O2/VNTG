using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectSettings.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    [System.Serializable, VolumeComponentMenu("VNTG/PSX Effect Settings")]
    public class PSXEffectSettings : VolumeComponent, IPostProcessComponent
    {
        [Header("Settings")]
        public BoolParameter Enabled = new BoolParameter(true);
        public BoolParameter ShowInSceneView = new BoolParameter(false);

        [Header("Pixelation")]
        public Vector2Parameter PixelResolution = new Vector2Parameter(new Vector2(256.0f, 256.0f));
        public ClampedFloatParameter ColorPrecision = new ClampedFloatParameter(32f, 0f, 256f);

        [Header("Dither")]
        public BoolParameter EnableDither = new BoolParameter(true);
        public EnumParameter<PSXDitherMode> DitherMode = new EnumParameter<PSXDitherMode>(PSXDitherMode.Additive);
        public ClampedIntParameter DitherPattern = new ClampedIntParameter(1, 0, 10);
        public BoolParameter DitherPixelPerfect = new BoolParameter(false);
        public ClampedFloatParameter DitherScale = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter DitherThreshold = new ClampedFloatParameter(0.1f, 0f, 1f);

        [Header("Fog")]
        public BoolParameter EnableFog = new BoolParameter(false);
        public ColorParameter FogColor = new ColorParameter(Color.black);
        public ClampedFloatParameter FogDensity = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);
        public ClampedFloatParameter FogNoiseStrength = new ClampedFloatParameter(0.1f, 0.0f, 1.0f);
        public ClampedFloatParameter FogEdgeSmoothness = new ClampedFloatParameter(0.5f, 0.01f, 1.0f);
        public ClampedFloatParameter FogNoiseScale = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter FogNoiseStart = new ClampedFloatParameter(0.7f, 0f, 1f);

        public bool IsActive() => Enabled.value;
        public bool IsTileCompatible() => false;
    }
}