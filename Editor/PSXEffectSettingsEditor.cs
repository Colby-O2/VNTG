using UnityEditor;
using UnityEditor.Rendering;

using ColbyO.VNTG.PSX;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectSettingsEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Editor
{
    [CustomEditor(typeof(PSXEffectSettings))]
    public class PSXEffectSettingsEditor : VolumeComponentEditor
    {
        private SerializedDataParameter _Enabled;
        private SerializedDataParameter _ShowInSceneView;

        private SerializedDataParameter _PixelResolution;
        private SerializedDataParameter _ColorPrecision;

        private SerializedDataParameter _EnableDither;
        private SerializedDataParameter _DitherMode;
        private SerializedDataParameter _DitherPattern;
        private SerializedDataParameter _DitherPixelPerfect;
        private SerializedDataParameter _DitherScale;
        private SerializedDataParameter _DitherThreshold;

        private SerializedDataParameter _EnableFog;
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

            _PixelResolution = Unpack(o.Find(x => x.PixelResolution));
            _ColorPrecision = Unpack(o.Find(x => x.ColorPrecision));

            _EnableDither = Unpack(o.Find(x => x.EnableDither));
            _DitherMode = Unpack(o.Find(x => x.DitherMode));
            _DitherPattern = Unpack(o.Find(x => x.DitherPattern));
            _DitherPixelPerfect = Unpack(o.Find(x => x.DitherPixelPerfect));
            _DitherScale = Unpack(o.Find(x => x.DitherScale));
            _DitherThreshold = Unpack(o.Find(x => x.DitherThreshold));

            _EnableFog = Unpack(o.Find(x => x.EnableFog));
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
            PropertyField(_PixelResolution);
            PropertyField(_ColorPrecision);

            EditorGUILayout.Space();
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
            PropertyField(_EnableFog);

            if (_EnableFog.value.boolValue)
            {
                EditorGUI.indentLevel++;
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