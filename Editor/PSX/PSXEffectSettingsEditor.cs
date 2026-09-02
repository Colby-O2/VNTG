using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectSettingsEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX.Editor
{
    [CustomEditor(typeof(PSXEffectSettings))]
    public class PSXEffectSettingsEditor : VolumeComponentEditor
    {
        private static readonly string[] _ditherModeOptions = new string[] { "Mult Lum", "Mult RGB", "Additive" };
        private static readonly string[] _ditherDisplayModeOptions = new string[] { "Abs", "Rel", "Pixel Rel" };

        private bool _showLighting = true;
        private bool _showPixelation = true;
        private bool _showColorPrecision = true;
        private bool _showDither = true;
        private bool _showColorPalette = true;
        private bool _showFog = true;

        private SerializedDataParameter _Enabled;
        private SerializedDataParameter _ShowInSceneView;
        private SerializedDataParameter _AmbientColor;
        private SerializedDataParameter _EnablePixelation;
        private SerializedDataParameter _EnableLetterbox;
        private SerializedDataParameter _PixelResolution;
        private SerializedDataParameter _EnableColorPrecision;
        private SerializedDataParameter _ColorPrecision;
        private SerializedDataParameter _EnableDither;
        private SerializedDataParameter _DitherMode;
        private SerializedDataParameter _DitherPattern;
        private SerializedDataParameter _DitherDisplayMode;
        private SerializedDataParameter _DitherReferenceResolution;
        private SerializedDataParameter _DitherScale;
        private SerializedDataParameter _DitherThreshold;
        private SerializedDataParameter _EnableColorPalette;
        private SerializedDataParameter _NormalizeLuminanceBeforeSampling;
        private SerializedDataParameter _PreserveLighting;
        private SerializedDataParameter _PaletteFileAsset;
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
            if (target == null || targets == null || targets.Length == 0) return;

            base.OnEnable();

            var o = new PropertyFetcher<PSXEffectSettings>(serializedObject);

            _Enabled = Unpack(o.Find(x => x.Enabled));
            _ShowInSceneView = Unpack(o.Find(x => x.ShowInSceneView));
            _AmbientColor = Unpack(o.Find(x => x.AmbientColor));
            _EnablePixelation = Unpack(o.Find(x => x.EnablePixelation));
            _EnableLetterbox = Unpack(o.Find(x => x.EnableLetterbox));
            _PixelResolution = Unpack(o.Find(x => x.PixelResolution));
            _EnableColorPrecision = Unpack(o.Find(x => x.EnableColorPrecision));
            _ColorPrecision = Unpack(o.Find(x => x.ColorPrecision));
            _EnableDither = Unpack(o.Find(x => x.EnableDither));
            _DitherMode = Unpack(o.Find(x => x.DitherMode));
            _DitherPattern = Unpack(o.Find(x => x.DitherPattern));
            _DitherDisplayMode = Unpack(o.Find(x => x.DitherDisplayMode));
            _DitherReferenceResolution = Unpack(o.Find(x => x.DitherReferenceRes));
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
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            DrawSettingsSection();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);

            _showLighting = VNTGEditorGUIUtils.DrawSectionHeaderWithOverrideState(_showLighting, "Lighting", _AmbientColor, ResetLightingSection);
            if (_showLighting && _AmbientColor.overrideState.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawLightingSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showPixelation = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(_showPixelation, "Pixelation", _EnablePixelation, ResetPixelationSection);
            if (_showPixelation && _EnablePixelation.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawPixelationSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showColorPrecision = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(_showColorPrecision, "Color Precision", _EnableColorPrecision, ResetColorPrecisionSection);
            if (_showColorPrecision && _EnableColorPrecision.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawColorPrecisionSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showDither = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(_showDither, "Dither", _EnableDither, ResetDitherSection);
            if (_showDither && _EnableDither.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawDitherSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showColorPalette = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(_showColorPalette, "Color Palette", _EnableColorPalette, ResetColorPaletteSection);
            if (_showColorPalette && _EnableColorPalette.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawColorPaletteSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showFog = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(_showFog, "Fog", _EnableFog, ResetFogSection);
            if (_showFog && _EnableFog.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawFogSection();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            VNTGEditorGUIUtils.DrawToggleButton(_Enabled, "Enabled", inline: true);
            GUILayout.Space(10);
            VNTGEditorGUIUtils.DrawToggleButton(_ShowInSceneView, "Scene View", inline: true);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        private void ResetLightingSection()
        {
            var def = ScriptableObject.CreateInstance<PSXEffectSettings>();
            _AmbientColor.value.colorValue = def.AmbientColor.value;
            _AmbientColor.overrideState.boolValue = def.AmbientColor.overrideState;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetPixelationSection()
        {
            var def = ScriptableObject.CreateInstance<PSXEffectSettings>();
            _EnablePixelation.value.boolValue = def.EnablePixelation.value;
            _EnableLetterbox.value.boolValue = def.EnableLetterbox.value;
            _PixelResolution.value.vector2Value = def.PixelResolution.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetColorPrecisionSection()
        {
            var def = ScriptableObject.CreateInstance<PSXEffectSettings>();
            _EnableColorPrecision.value.boolValue = def.EnableColorPrecision.value;
            _ColorPrecision.value.floatValue = def.ColorPrecision.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetDitherSection()
        {
            var def = ScriptableObject.CreateInstance<PSXEffectSettings>();
            _EnableDither.value.boolValue = def.EnableDither.value;
            _DitherMode.value.enumValueIndex = System.Convert.ToInt32(def.DitherMode.value);
            _DitherPattern.value.intValue = def.DitherPattern.value;
            _DitherDisplayMode.value.enumValueIndex = System.Convert.ToInt32(def.DitherDisplayMode.value);
            _DitherReferenceResolution.value.vector2Value = def.DitherReferenceRes.value;
            _DitherScale.value.floatValue = def.DitherScale.value;
            _DitherThreshold.value.floatValue = def.DitherThreshold.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetColorPaletteSection()
        {
            var def = ScriptableObject.CreateInstance<PSXEffectSettings>();
            _EnableColorPalette.value.boolValue = def.EnableColorPalette.value;
            _NormalizeLuminanceBeforeSampling.value.boolValue = def.NormalizeLuminanceBeforeSampling.value;
            _PreserveLighting.value.boolValue = def.PreserveLighting.value;
            _PaletteFileAsset.value.objectReferenceValue = def.PaletteAsset.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetFogSection()
        {
            var def = ScriptableObject.CreateInstance<PSXEffectSettings>();
            _EnableFog.value.boolValue = def.EnableFog.value;
            _IgnoreSkybox.value.boolValue = def.IgnoreSkybox.value;
            _FogColor.value.colorValue = def.FogColor.value;
            _FogDensity.value.floatValue = def.FogDensity.value;
            _FogNoiseStrength.value.floatValue = def.FogNoiseStrength.value;
            _FogEdgeSmoothness.value.floatValue = def.FogEdgeSmoothness.value;
            _FogNoiseScale.value.floatValue = def.FogNoiseScale.value;
            _FogNoiseStart.value.floatValue = def.FogNoiseStart.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLightingSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);
            VNTGEditorGUIUtils.DrawParamField(_AmbientColor);
            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawPixelationSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawInlineVector2Param(_PixelResolution);

            EditorGUI.indentLevel++;
            EditorGUILayout.Space(1);
            VNTGEditorGUIUtils.DrawToggleButton(_EnableLetterbox, "Keep Aspect Ratio");
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawColorPrecisionSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_ColorPrecision, 1.0f, 256.0f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawDitherSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            DrawDitherDisplayModeToolbar(_DitherDisplayMode, "Dither Display Mode", _EnablePixelation.value.boolValue);
            EditorGUI.indentLevel++;
            VNTGEditorGUIUtils.DrawClampedSlider(_DitherScale, 0.0f, 1.0f);
            if (_DitherDisplayMode.value.intValue == (int)PSXDitherDisplayMode.RelativeScale)
            {
                VNTGEditorGUIUtils.DrawInlineVector2Param(_DitherReferenceResolution);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawEnumToolbarParameter<PSXDitherMode>(_DitherMode, "Dither Mode", _ditherModeOptions);
            EditorGUI.indentLevel++;
            VNTGEditorGUIUtils.DrawClampedSlider(_DitherPattern, 0, 10);
            VNTGEditorGUIUtils.DrawClampedSlider(_DitherThreshold, 0.0f, 1.0f);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawColorPaletteSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawParamField(_PaletteFileAsset);
            VNTGEditorGUIUtils.DrawToggleButton(_PreserveLighting, "Preserve Lighting");
            VNTGEditorGUIUtils.DrawToggleButton(_NormalizeLuminanceBeforeSampling, "Normalize Luminance");

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawFogSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawToggleButton(_IgnoreSkybox, "Ignore Skybox");
            VNTGEditorGUIUtils.DrawParamField(_FogColor);
            VNTGEditorGUIUtils.DrawClampedSlider(_FogDensity, 0.0f, 1.0f);
            VNTGEditorGUIUtils.DrawClampedSlider(_FogNoiseStrength, 0.0f, 1.0f);
            VNTGEditorGUIUtils.DrawClampedSlider(_FogEdgeSmoothness, 0.01f, 1.0f);
            VNTGEditorGUIUtils.DrawClampedSlider(_FogNoiseScale, 0.0f, 1.0f);
            VNTGEditorGUIUtils.DrawClampedSlider(_FogNoiseStart, 0.0f, 1.0f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }
        private void DrawDitherDisplayModeToolbar(SerializedDataParameter param, string label, bool enablePixelation)
        {
            if (param == null) return;

            SerializedProperty enumProp = param.value;

            if (!enablePixelation && enumProp.intValue == (int)PSXDitherDisplayMode.PixelResolutionRelative)
            {
                enumProp.intValue = (int)PSXDitherDisplayMode.RelativeScale;
            }

            VNTGEditorGUIUtils.DrawEnumToolbarParameter<PSXDitherDisplayMode>(
                param,
                label,
                _ditherDisplayModeOptions,
                index => index == (int)PSXDitherDisplayMode.PixelResolutionRelative && !enablePixelation
            );
        }
    }
}