using UnityEditor;
using UnityEditor.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectSettingsEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX.Editor
{
    [CustomEditor(typeof(PSXEffectSettings))]
    public class PSXEffectSettingsEditor : VolumeComponentEditor
    {
        // Settings
        private SerializedDataParameter _Enabled;
        private SerializedDataParameter _ShowInSceneView;

        // Lighting
        private SerializedDataParameter _AmbientColor;

        // Pixelation
        private SerializedDataParameter _EnablePixelation;
        private SerializedDataParameter _PixelResolution;

        // Color Precision
        private SerializedDataParameter _EnableColorPrecision;
        private SerializedDataParameter _ColorPrecision;

        // Dither
        private SerializedDataParameter _EnableDither;
        private SerializedDataParameter _DitherMode;
        private SerializedDataParameter _DitherPattern;
        private SerializedDataParameter _DitherPixelPerfect;
        private SerializedDataParameter _DitherScale;
        private SerializedDataParameter _DitherThreshold;

        // Color Palette 
        private SerializedDataParameter _EnableColorPalette;
        private SerializedDataParameter _NormalizeLuminanceBeforeSampling;
        private SerializedDataParameter _PreserveLighting;
        private SerializedDataParameter _PaletteFileAsset;

        // Fog
        private SerializedDataParameter _EnableFog;
        private SerializedDataParameter _IgnoreSkybox;
        private SerializedDataParameter _FogColor;
        private SerializedDataParameter _FogDensity;
        private SerializedDataParameter _FogNoiseStrength;
        private SerializedDataParameter _FogEdgeSmoothness;
        private SerializedDataParameter _FogNoiseScale;
        private SerializedDataParameter _FogNoiseStart;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<PSXEffectSettings>(serializedObject);

            _Enabled = Unpack(o.Find(x => x.Enabled));
            _ShowInSceneView = Unpack(o.Find(x => x.ShowInSceneView));

            _AmbientColor = Unpack(o.Find(x => x.AmbientColor));

            _EnablePixelation = Unpack(o.Find(x => x.EnablePixelation));
            _PixelResolution = Unpack(o.Find(x => x.PixelResolution));

            _EnableColorPrecision = Unpack(o.Find(x => x.EnableColorPrecision));
            _ColorPrecision = Unpack(o.Find(x => x.ColorPrecision));

            _EnableDither = Unpack(o.Find(x => x.EnableDither));
            _DitherMode = Unpack(o.Find(x => x.DitherMode));
            _DitherPattern = Unpack(o.Find(x => x.DitherPattern));
            _DitherPixelPerfect = Unpack(o.Find(x => x.DitherPixelPerfect));
            _DitherScale = Unpack(o.Find(x => x.DitherScale));
            _DitherThreshold = Unpack(o.Find(x => x.DitherThreshold));

            _EnableColorPalette = Unpack(o.Find(x => x.EnableColorPalette));
            _NormalizeLuminanceBeforeSampling = Unpack(o.Find(x => x.NormalizeLuminanceBeforeSampling));
            _PreserveLighting = Unpack(o.Find(x => x.PreserveLighting));
            _PaletteFileAsset = Unpack(o.Find(x => x.PaletteAsset));

            _EnableFog = Unpack(o.Find(x => x.EnableFog));
            _IgnoreSkybox = Unpack(o.Find(x => x.IgnoreSkybox));
            _FogColor = Unpack(o.Find(x => x.FogColor));
            _FogDensity = Unpack(o.Find(x => x.FogDensity));
            _FogNoiseStrength = Unpack(o.Find(x => x.FogNoiseStrength));
            _FogEdgeSmoothness = Unpack(o.Find(x => x.FogEdgeSmoothness));
            _FogNoiseScale = Unpack(o.Find(x => x.FogNoiseScale));
            _FogNoiseStart = Unpack(o.Find(x => x.FogNoiseStart));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_Enabled);
            PropertyField(_ShowInSceneView);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
            PropertyField(_AmbientColor);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pixelation", EditorStyles.boldLabel);
            PropertyField(_EnablePixelation);

            if (_EnablePixelation.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_PixelResolution);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Precision", EditorStyles.boldLabel);
            PropertyField(_EnableColorPrecision);

            if (_EnableColorPrecision.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_ColorPrecision);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dither", EditorStyles.boldLabel);
            PropertyField(_EnableDither);

            if (_EnableDither.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_DitherMode);
                PropertyField(_DitherPattern);
                PropertyField(_DitherPixelPerfect);

                if (!_DitherPixelPerfect.value.boolValue)
                {
                    PropertyField(_DitherScale);
                }

                PropertyField(_DitherThreshold);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Palette ", EditorStyles.boldLabel);
            PropertyField(_EnableColorPalette);

            if (_EnableColorPalette.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_PreserveLighting);
                PropertyField(_NormalizeLuminanceBeforeSampling);
                PropertyField(_PaletteFileAsset);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fog", EditorStyles.boldLabel);
            PropertyField(_EnableFog);

            if (_EnableFog.value.boolValue)
            {
                EditorGUI.indentLevel++;
                PropertyField(_IgnoreSkybox);
                PropertyField(_FogColor);
                PropertyField(_FogDensity);
                PropertyField(_FogNoiseStrength);
                PropertyField(_FogEdgeSmoothness);
                PropertyField(_FogNoiseScale);
                PropertyField(_FogNoiseStart);
                EditorGUI.indentLevel--;
            }
        }
    }
}