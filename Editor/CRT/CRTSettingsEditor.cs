using UnityEditor;
using UnityEditor.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTSettingsEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT.Editor
{
    [CustomEditor(typeof(CRTSettings))]
    public class CRTSettingsEditor : VolumeComponentEditor
    {
        // Settings
        private SerializedDataParameter _Enabled;
        private SerializedDataParameter _ShowInSceneView;

        // Screen
        private SerializedDataParameter _ScreenResolution;
        private SerializedDataParameter _UseMaxFPS;
        private SerializedDataParameter _RefreshRate;
        private SerializedDataParameter _DecayRate;
        private SerializedDataParameter _EnableInterlacedRendering;

        // Screen Shape
        private SerializedDataParameter _EnableScreenBend;
        private SerializedDataParameter _ScreenBend;
        private SerializedDataParameter _ScreenRoundness;
        private SerializedDataParameter _VignetteOpacity;

        // Scanlines
        private SerializedDataParameter _ScanLineVerticalOpacity;
        private SerializedDataParameter _ScanLineHorizontalOpacity;
        private SerializedDataParameter _ScanLineVerticalSpeed;
        private SerializedDataParameter _ScanLineHorizontalSpeed;
        private SerializedDataParameter _ScanLineStrength;

        // Noise
        private SerializedDataParameter _NoiseScale;
        private SerializedDataParameter _NoiseRBGOffsetX;
        private SerializedDataParameter _NoiseRBGOffsetY;
        private SerializedDataParameter _NoiseSpeed;
        private SerializedDataParameter _NoiseFade;

        // VHS
        private SerializedDataParameter _VhsSmear;

        // Tracking
        private SerializedDataParameter _EnableTrackerLine;
        private SerializedDataParameter _TrackingSpeed;
        private SerializedDataParameter _TrackingJitter;
        private SerializedDataParameter _GlitchChance;
        private SerializedDataParameter _GlitchLength;

        // Signal Interference
        private SerializedDataParameter _EnableSignalInterference;
        private SerializedDataParameter _InterferenceFrequency;
        private SerializedDataParameter _InterferenceAmplitude;

        // Subpixels / Chromatic Aberration
        private SerializedDataParameter _ChromaticOffset;
        private SerializedDataParameter _ChromaticOffsetSpeed;
        private SerializedDataParameter _SubPixelMode;
        private SerializedDataParameter _SubPixelDensity;

        // Post-Process
        private SerializedDataParameter _UnsharpAmount;
        private SerializedDataParameter _UnsharpRadius;
        private SerializedDataParameter _UnsharpThreshold;
        private SerializedDataParameter _ClampBlack;
        private SerializedDataParameter _ClampWhite;
        private SerializedDataParameter _ShadowTint;

        // Color Adjustment
        private SerializedDataParameter _Gamma;
        private SerializedDataParameter _Brightness;
        private SerializedDataParameter _Contrast;
        private SerializedDataParameter _Saturation;
        private SerializedDataParameter _Hue;
        private SerializedDataParameter _RedShift;
        private SerializedDataParameter _BlueShift;
        private SerializedDataParameter _GreenShift;
        private SerializedDataParameter _IsMonochrome;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<CRTSettings>(serializedObject);

            _Enabled = Unpack(o.Find(x => x.Enabled));
            _ShowInSceneView = Unpack(o.Find(x => x.ShowInSceneView));

            _ScreenResolution = Unpack(o.Find(x => x.ScreenResolution));
            _UseMaxFPS = Unpack(o.Find(x => x.UseMaxFPS));
            _RefreshRate = Unpack(o.Find(x => x.RefreshRate));
            _DecayRate = Unpack(o.Find(x => x.DecayRate));
            _EnableInterlacedRendering = Unpack(o.Find(x => x.EnableInterlacedRendering));

            _EnableScreenBend = Unpack(o.Find(x => x.EnableScreenBend));
            _ScreenBend = Unpack(o.Find(x => x.ScreenBend));
            _ScreenRoundness = Unpack(o.Find(x => x.ScreenRoundness));
            _VignetteOpacity = Unpack(o.Find(x => x.VignetteOpacity));

            _ScanLineVerticalOpacity = Unpack(o.Find(x => x.ScanLineVerticalOpacity));
            _ScanLineHorizontalOpacity = Unpack(o.Find(x => x.ScanLineHorizontalOpacity));
            _ScanLineVerticalSpeed = Unpack(o.Find(x => x.ScanLineVerticalSpeed));
            _ScanLineHorizontalSpeed = Unpack(o.Find(x => x.ScanLineHorizontalSpeed));
            _ScanLineStrength = Unpack(o.Find(x => x.ScanLineStrength));

            _NoiseScale = Unpack(o.Find(x => x.NoiseScale));
            _NoiseRBGOffsetX = Unpack(o.Find(x => x.NoiseRBGOffsetX));
            _NoiseRBGOffsetY = Unpack(o.Find(x => x.NoiseRBGOffsetY));
            _NoiseSpeed = Unpack(o.Find(x => x.NoiseSpeed));
            _NoiseFade = Unpack(o.Find(x => x.NoiseFade));

            _VhsSmear = Unpack(o.Find(x => x.VhsSmear));

            _EnableTrackerLine = Unpack(o.Find(x => x.EnableTrackerLine));
            _TrackingSpeed = Unpack(o.Find(x => x.TrackingSpeed));
            _TrackingJitter = Unpack(o.Find(x => x.TrackingJitter));
            _GlitchChance = Unpack(o.Find(x => x.GlitchChance));
            _GlitchLength = Unpack(o.Find(x => x.GlitchLength));

            _EnableSignalInterference = Unpack(o.Find(x => x.EnableSignalInterference));
            _InterferenceFrequency = Unpack(o.Find(x => x.InterferenceFrequency));
            _InterferenceAmplitude = Unpack(o.Find(x => x.InterferenceAmplitude));

            _ChromaticOffset = Unpack(o.Find(x => x.ChromaticOffset));
            _ChromaticOffsetSpeed = Unpack(o.Find(x => x.ChromaticOffsetSpeed));
            _SubPixelMode = Unpack(o.Find(x => x.SubPixelMode));
            _SubPixelDensity = Unpack(o.Find(x => x.SubPixelDensity));

            _UnsharpAmount = Unpack(o.Find(x => x.UnsharpAmount));
            _UnsharpRadius = Unpack(o.Find(x => x.UnsharpRadius));
            _UnsharpThreshold = Unpack(o.Find(x => x.UnsharpThreshold));
            _ClampBlack = Unpack(o.Find(x => x.ClampBlack));
            _ClampWhite = Unpack(o.Find(x => x.ClampWhite));
            _ShadowTint = Unpack(o.Find(x => x.ShadowTint));

            _Gamma = Unpack(o.Find(x => x.Gamma));
            _Brightness = Unpack(o.Find(x => x.Brightness));
            _Contrast = Unpack(o.Find(x => x.Contrast));
            _Saturation = Unpack(o.Find(x => x.Saturation));
            _Hue = Unpack(o.Find(x => x.Hue));
            _RedShift = Unpack(o.Find(x => x.RedShift));
            _BlueShift = Unpack(o.Find(x => x.BlueShift));
            _GreenShift = Unpack(o.Find(x => x.GreenShift));
            _IsMonochrome = Unpack(o.Find(x => x.IsMonochrome));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_Enabled);
            PropertyField(_ShowInSceneView);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Screen", EditorStyles.boldLabel);
            PropertyField(_ScreenResolution);
            PropertyField(_UseMaxFPS);

            if (!_UseMaxFPS.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_RefreshRate);
                EditorGUI.indentLevel--;
            }

            PropertyField(_DecayRate);
            PropertyField(_EnableInterlacedRendering);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Screen Shape", EditorStyles.boldLabel);
            PropertyField(_EnableScreenBend);
            if (_EnableScreenBend.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_ScreenBend);
                PropertyField(_ScreenRoundness);
                EditorGUI.indentLevel--;
            }
            PropertyField(_VignetteOpacity);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scanlines", EditorStyles.boldLabel);
            PropertyField(_ScanLineVerticalOpacity);
            PropertyField(_ScanLineHorizontalOpacity);
            PropertyField(_ScanLineVerticalSpeed);
            PropertyField(_ScanLineHorizontalSpeed);
            PropertyField(_ScanLineStrength);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Noise", EditorStyles.boldLabel);
            PropertyField(_NoiseScale);
            PropertyField(_NoiseRBGOffsetX);
            PropertyField(_NoiseRBGOffsetY);
            PropertyField(_NoiseSpeed);
            PropertyField(_NoiseFade);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("VHS", EditorStyles.boldLabel);
            PropertyField(_VhsSmear);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tracking", EditorStyles.boldLabel);
            PropertyField(_EnableTrackerLine);
            if (_EnableTrackerLine.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_TrackingSpeed);
                PropertyField(_TrackingJitter);
                EditorGUI.indentLevel--;
            }
            PropertyField(_GlitchChance);
            PropertyField(_GlitchLength);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Signal Interference", EditorStyles.boldLabel);
            PropertyField(_EnableSignalInterference);
            if (_EnableSignalInterference.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_InterferenceFrequency);
                PropertyField(_InterferenceAmplitude);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Subpixels", EditorStyles.boldLabel);
            PropertyField(_ChromaticOffset);
            PropertyField(_ChromaticOffsetSpeed);
            PropertyField(_SubPixelMode);
            PropertyField(_SubPixelDensity);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Post-Process", EditorStyles.boldLabel);
            PropertyField(_UnsharpAmount);
            PropertyField(_UnsharpRadius);
            PropertyField(_UnsharpThreshold);
            PropertyField(_ClampBlack);
            PropertyField(_ClampWhite);
            PropertyField(_ShadowTint);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Adjustment", EditorStyles.boldLabel);
            PropertyField(_Gamma);
            PropertyField(_Brightness);
            PropertyField(_Contrast);
            PropertyField(_Saturation);
            PropertyField(_Hue);
            PropertyField(_RedShift);
            PropertyField(_BlueShift);
            PropertyField(_GreenShift);
            PropertyField(_IsMonochrome);
        }
    }
}