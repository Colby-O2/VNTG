using ColbyO.VNTG.PSX.Editor;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTSettingsEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT.Editor
{
    [CustomEditor(typeof(CRTSettings))]
    public class CRTSettingsEditor : VolumeComponentEditor
    {
        private static readonly string[] _subPixelModeOptions = new string[] { "None", "Shadow", "Grille", "Spot" };

        private bool _showScreen = true;
        private bool _showScreenShape = true;
        private bool _showVignette = true;
        private bool _showScanlines = true;
        private bool _showNoise = true;
        private bool _showVhs = true;
        private bool _showTracking = true;
        private bool _showGlitches = true;
        private bool _showSignalInterference = true;
        private bool _showChromaticAberration = true;
        private bool _showSubpixels = true;
        private bool _showUnsharp = true;
        private bool _showClampColor = true;
        private bool _showColorAdjustment = true;

        // Settings
        private SerializedDataParameter _Enabled;
        private SerializedDataParameter _ShowInSceneView;

        // Screen
        private SerializedDataParameter _EnableScreenSettings;
        private SerializedDataParameter _ScreenResolution;
        private SerializedDataParameter _UseMaxFPS;
        private SerializedDataParameter _RefreshRate;
        private SerializedDataParameter _DecayRate;
        private SerializedDataParameter _EnableInterlacedRendering;

        // Screen Shape
        private SerializedDataParameter _EnableScreenBend;
        private SerializedDataParameter _ScreenBend;

        // Vignette
        private SerializedDataParameter _EnableVignette;
        private SerializedDataParameter _ScreenRoundness;
        private SerializedDataParameter _VignetteOpacity;


        // Scanlines
        private SerializedDataParameter _EnableScanlines;
        private SerializedDataParameter _ScanLineVerticalOpacity;
        private SerializedDataParameter _ScanLineHorizontalOpacity;
        private SerializedDataParameter _ScanLineVerticalSpeed;
        private SerializedDataParameter _ScanLineHorizontalSpeed;
        private SerializedDataParameter _ScanLineStrength;

        // Noise
        private SerializedDataParameter _EnableNoise;
        private SerializedDataParameter _NoiseScale;
        private SerializedDataParameter _NoiseRBGOffsetX;
        private SerializedDataParameter _NoiseRBGOffsetY;
        private SerializedDataParameter _NoiseSpeed;
        private SerializedDataParameter _NoiseFade;

        // VHS
        private SerializedDataParameter _EnableVhs;
        private SerializedDataParameter _VhsSmear;

        // Tracking
        private SerializedDataParameter _EnableTrackerLine;
        private SerializedDataParameter _TrackingSpeed;
        private SerializedDataParameter _TrackingJitter;

        // Glitches
        private SerializedDataParameter _EnableGlitch;
        private SerializedDataParameter _GlitchChance;
        private SerializedDataParameter _GlitchLength;

        // Signal Interference
        private SerializedDataParameter _EnableSignalInterference;
        private SerializedDataParameter _InterferenceFrequency;
        private SerializedDataParameter _InterferenceAmplitude;

        // Chromatic Aberration
        private SerializedDataParameter _EnableChromaticAberration;
        private SerializedDataParameter _ChromaticOffset;
        private SerializedDataParameter _ChromaticOffsetSpeed;

        // Subpixels
        private SerializedDataParameter _EnableSubPixels;
        private SerializedDataParameter _SubPixelMode;
        private SerializedDataParameter _SubPixelDensity;

        // Unsharp
        private SerializedDataParameter _EnableUnsharp;
        private SerializedDataParameter _UnsharpAmount;
        private SerializedDataParameter _UnsharpRadius;
        private SerializedDataParameter _UnsharpThreshold;

        // Color Clamp
        private SerializedDataParameter _EnableClampColor;
        private SerializedDataParameter _ClampBlack;
        private SerializedDataParameter _ClampWhite;
        private SerializedDataParameter _ShadowTint;

        // Color Adjustment
        private SerializedDataParameter _EnableColorAdjustment;
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
            if (target == null || targets == null || targets.Length == 0) return;

            base.OnEnable();

            var o = new PropertyFetcher<CRTSettings>(serializedObject);

            _Enabled = Unpack(o.Find(x => x.Enabled));
            _ShowInSceneView = Unpack(o.Find(x => x.ShowInSceneView));

            _EnableScreenSettings = Unpack(o.Find(x => x.EnableScreenSettings));
            _ScreenResolution = Unpack(o.Find(x => x.ScreenResolution));
            _UseMaxFPS = Unpack(o.Find(x => x.UseMaxFPS));
            _RefreshRate = Unpack(o.Find(x => x.RefreshRate));
            _DecayRate = Unpack(o.Find(x => x.DecayRate));
            _EnableInterlacedRendering = Unpack(o.Find(x => x.EnableInterlacedRendering));

            _EnableScreenBend = Unpack(o.Find(x => x.EnableScreenBend));
            _ScreenBend = Unpack(o.Find(x => x.ScreenBend));

            _EnableVignette = Unpack(o.Find(x => x.EnableVignette));
            _VignetteOpacity = Unpack(o.Find(x => x.VignetteOpacity));
            _ScreenRoundness = Unpack(o.Find(x => x.ScreenRoundness));

            _EnableScanlines = Unpack(o.Find(x => x.EnableScanlines));
            _ScanLineVerticalOpacity = Unpack(o.Find(x => x.ScanLineVerticalOpacity));
            _ScanLineHorizontalOpacity = Unpack(o.Find(x => x.ScanLineHorizontalOpacity));
            _ScanLineVerticalSpeed = Unpack(o.Find(x => x.ScanLineVerticalSpeed));
            _ScanLineHorizontalSpeed = Unpack(o.Find(x => x.ScanLineHorizontalSpeed));
            _ScanLineStrength = Unpack(o.Find(x => x.ScanLineStrength));

            _EnableNoise = Unpack(o.Find(x => x.EnableNoise));
            _NoiseScale = Unpack(o.Find(x => x.NoiseScale));
            _NoiseRBGOffsetX = Unpack(o.Find(x => x.NoiseRBGOffsetX));
            _NoiseRBGOffsetY = Unpack(o.Find(x => x.NoiseRBGOffsetY));
            _NoiseSpeed = Unpack(o.Find(x => x.NoiseSpeed));
            _NoiseFade = Unpack(o.Find(x => x.NoiseFade));

            _EnableVhs = Unpack(o.Find(x => x.EnableVhs));
            _VhsSmear = Unpack(o.Find(x => x.VhsSmear));

            _EnableTrackerLine = Unpack(o.Find(x => x.EnableTrackerLine));
            _TrackingSpeed = Unpack(o.Find(x => x.TrackingSpeed));
            _TrackingJitter = Unpack(o.Find(x => x.TrackingJitter));

            _EnableGlitch = Unpack(o.Find(x => x.EnableGlitch));
            _GlitchChance = Unpack(o.Find(x => x.GlitchChance));
            _GlitchLength = Unpack(o.Find(x => x.GlitchLength));

            _EnableSignalInterference = Unpack(o.Find(x => x.EnableSignalInterference));
            _InterferenceFrequency = Unpack(o.Find(x => x.InterferenceFrequency));
            _InterferenceAmplitude = Unpack(o.Find(x => x.InterferenceAmplitude));

            _EnableChromaticAberration = Unpack(o.Find(x => x.EnableChromaticAberration));
            _ChromaticOffset = Unpack(o.Find(x => x.ChromaticOffset));
            _ChromaticOffsetSpeed = Unpack(o.Find(x => x.ChromaticOffsetSpeed));

            _EnableSubPixels = Unpack(o.Find(x => x.EnableSubPixels));
            _SubPixelMode = Unpack(o.Find(x => x.SubPixelMode));
            _SubPixelDensity = Unpack(o.Find(x => x.SubPixelDensity));

            _EnableUnsharp = Unpack(o.Find(x => x.EnableUnsharp));
            _UnsharpAmount = Unpack(o.Find(x => x.UnsharpAmount));
            _UnsharpRadius = Unpack(o.Find(x => x.UnsharpRadius));
            _UnsharpThreshold = Unpack(o.Find(x => x.UnsharpThreshold));

            _EnableClampColor = Unpack(o.Find(x => x.EnableClampColor));
            _ClampBlack = Unpack(o.Find(x => x.ClampBlack));
            _ClampWhite = Unpack(o.Find(x => x.ClampWhite));
            _ShadowTint = Unpack(o.Find(x => x.ShadowTint));

            _EnableColorAdjustment = Unpack(o.Find(x => x.EnableColorAdjustment));
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
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            DrawSettingsSection();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);

            _showScreen = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showScreen,
                "Screen",
                _EnableScreenSettings,
                ResetScreenSection
            );
            if (_showScreen && _EnableScreenSettings.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawScreenSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showScreenShape = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showScreenShape, 
                "Screen Shape", 
                _EnableScreenBend, 
                ResetScreenShapeSection
            );
            if (_showScreenShape && _EnableScreenBend.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawScreenShapeSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showVignette = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showVignette,
                "Vignette",
                _EnableVignette,
                ResetVignetteSection
            );
            if (_showVignette && _EnableVignette.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawVignetteSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showScanlines = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showScanlines,
                "Scanlines",
                _EnableScanlines,
                ResetScanlinesSection
            );
            if (_showScanlines && _EnableScanlines.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawScanlinesSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showNoise = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showNoise,
                "Noise",
                _EnableNoise,
                ResetNoiseSection
            );
            if (_showNoise && _EnableNoise.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawNoiseSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showVhs = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showVhs,
                "VHS",
                _EnableVhs,
                ResetVhsSection
            );
            if (_showVhs && _EnableVhs.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawVhsSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showTracking = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showTracking, 
                "Tracking", 
                _EnableTrackerLine, 
                ResetTrackingSection
            );
            if (_showTracking && _EnableTrackerLine.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawTrackingSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showGlitches = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showGlitches,
                "Glitches",
                _EnableGlitch,
                ResetGlitchesSection
            );
            if (_showGlitches && _EnableGlitch.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawGlitchesSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showSignalInterference = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showSignalInterference, 
                "Interference", 
                _EnableSignalInterference, 
                ResetSignalInterferenceSection
            );
            if (_showSignalInterference && _EnableSignalInterference.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawSignalInterferenceSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showSubpixels = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showChromaticAberration,
                "Aberration",
                _EnableChromaticAberration,
                ResetChromaticAberrationSection
            );
            if (_showChromaticAberration && _EnableChromaticAberration.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawChromaticAberrationSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showSubpixels = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                _showSubpixels,
                "Subpixels",
                _EnableSubPixels,
                ResetSubpixelsSection
            );
            if (_showSubpixels && _EnableSubPixels.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawSubpixelsSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showUnsharp = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                 _showUnsharp,
                 "Unsharp",
                 _EnableUnsharp,
                 ResetUnsharpSection
            );
            if (_showUnsharp && _EnableUnsharp.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawUnsharpSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showClampColor = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                 _showClampColor,
                 "Color Clamping",
                 _EnableClampColor,
                 ResetClampColorSection
            );
            if (_showClampColor && _EnableClampColor.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawClampColorSection();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            _showColorAdjustment = VNTGEditorGUIUtils.DrawSectionHeaderWithToggle(
                 _showColorAdjustment,
                 "Color Grading",
                 _EnableColorAdjustment,
                 ResetColorAdjustmentSection
            );
            if (_showColorAdjustment && _EnableColorAdjustment.value.boolValue)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                DrawColorAdjustmentSection();
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

        private void ResetScreenSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _ScreenResolution.value.vector2Value = def.ScreenResolution.value;
            _UseMaxFPS.value.boolValue = def.UseMaxFPS.value;
            _RefreshRate.value.intValue = def.RefreshRate.value;
            _DecayRate.value.floatValue = def.DecayRate.value;
            _EnableInterlacedRendering.value.boolValue = def.EnableInterlacedRendering.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetScreenShapeSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _EnableScreenBend.value.boolValue = def.EnableScreenBend.value;
            _ScreenBend.value.floatValue = def.ScreenBend.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetVignetteSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _VignetteOpacity.value.floatValue = def.VignetteOpacity.value;
            _ScreenRoundness.value.floatValue = def.ScreenRoundness.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetScanlinesSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _ScanLineVerticalOpacity.value.floatValue = def.ScanLineVerticalOpacity.value;
            _ScanLineHorizontalOpacity.value.floatValue = def.ScanLineHorizontalOpacity.value;
            _ScanLineVerticalSpeed.value.floatValue = def.ScanLineVerticalSpeed.value;
            _ScanLineHorizontalSpeed.value.floatValue = def.ScanLineVerticalSpeed.value;
            _ScanLineStrength.value.floatValue = def.ScanLineStrength.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetNoiseSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _NoiseScale.value.floatValue = def.NoiseScale.value;
            _NoiseRBGOffsetX.value.floatValue = def.NoiseRBGOffsetX.value;
            _NoiseRBGOffsetY.value.floatValue = def.NoiseRBGOffsetY.value;
            _NoiseSpeed.value.floatValue = def.NoiseSpeed.value;
            _NoiseFade.value.floatValue = def.NoiseFade.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetVhsSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _VhsSmear.value.floatValue = def.VhsSmear.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetTrackingSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _EnableTrackerLine.value.boolValue = def.EnableTrackerLine.value;
            _TrackingSpeed.value.floatValue = def.TrackingSpeed.value;
            _TrackingJitter.value.floatValue = def.TrackingJitter.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetGlitchesSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _GlitchChance.value.floatValue = def.GlitchChance.value;
            _GlitchLength.value.floatValue = def.GlitchLength.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetSignalInterferenceSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _EnableSignalInterference.value.boolValue = def.EnableSignalInterference.value;
            _InterferenceFrequency.value.floatValue = def.InterferenceFrequency.value;
            _InterferenceAmplitude.value.floatValue = def.InterferenceAmplitude.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }
        

        private void ResetChromaticAberrationSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _ChromaticOffset.value.floatValue = def.ChromaticOffset.value;
            _ChromaticOffsetSpeed.value.floatValue = def.ChromaticOffsetSpeed.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetSubpixelsSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _SubPixelMode.value.enumValueIndex = System.Convert.ToInt32(def.SubPixelMode.value);
            _SubPixelDensity.value.floatValue = def.SubPixelDensity.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetUnsharpSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _UnsharpAmount.value.floatValue = def.UnsharpAmount.value;
            _UnsharpRadius.value.floatValue = def.UnsharpRadius.value;
            _UnsharpThreshold.value.floatValue = def.UnsharpThreshold.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetClampColorSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _ClampBlack.value.floatValue = def.ClampBlack.value;
            _ClampWhite.value.floatValue = def.ClampWhite.value;
            _ShadowTint.value.colorValue = def.ShadowTint.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetColorAdjustmentSection()
        {
            var def = ScriptableObject.CreateInstance<CRTSettings>();
            _Gamma.value.floatValue = def.Gamma.value;
            _Brightness.value.floatValue = def.Brightness.value;
            _Contrast.value.floatValue = def.Contrast.value;
            _Saturation.value.floatValue = def.Saturation.value;
            _Hue.value.floatValue = def.Hue.value;
            _RedShift.value.floatValue = def.RedShift.value;
            _BlueShift.value.floatValue = def.BlueShift.value;
            _GreenShift.value.floatValue = def.GreenShift.value;
            _IsMonochrome.value.boolValue = def.IsMonochrome.value;
            DestroyImmediate(def);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScreenSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawInlineVector2Param(_ScreenResolution);
            VNTGEditorGUIUtils.DrawToggleButton(_UseMaxFPS, "Use Max FPS");

            if (!_UseMaxFPS.value.boolValue)
            {
                EditorGUI.indentLevel++;
                VNTGEditorGUIUtils.DrawClampedSlider(_RefreshRate, 0, 360);
                EditorGUI.indentLevel--;
            }

            VNTGEditorGUIUtils.DrawClampedSlider(_DecayRate, 0f, 1f);
            VNTGEditorGUIUtils.DrawToggleButton(_EnableInterlacedRendering, "Interlaced Rendering");

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawScreenShapeSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_ScreenBend, 0f, 100f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawVignetteSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_VignetteOpacity, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ScreenRoundness, 0f, 1f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawScanlinesSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_ScanLineVerticalOpacity, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ScanLineHorizontalOpacity, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ScanLineVerticalSpeed, -1f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ScanLineHorizontalSpeed, -1f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ScanLineStrength, 0f, 1f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawNoiseSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_NoiseScale, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_NoiseRBGOffsetX, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_NoiseRBGOffsetY, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_NoiseSpeed, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_NoiseFade, 0f, 1f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawVhsSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_VhsSmear, 0.01f, 1f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawTrackingSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_TrackingSpeed, 1f, 20f);
            VNTGEditorGUIUtils.DrawClampedSlider(_TrackingJitter, 0f, 50f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawGlitchesSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_GlitchChance, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_GlitchLength, 0f, 30f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawSignalInterferenceSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_InterferenceFrequency, 0f, 200f);
            VNTGEditorGUIUtils.DrawClampedSlider(_InterferenceAmplitude, 0f, 1f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawChromaticAberrationSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_ChromaticOffset, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ChromaticOffsetSpeed, 0f, 1f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawSubpixelsSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawEnumToolbarParameter<CRTSubPixelMode>(_SubPixelMode, "Sub Pixel Mode", _subPixelModeOptions);
            VNTGEditorGUIUtils.DrawClampedSlider(_SubPixelDensity, 100f, 400f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawUnsharpSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_UnsharpAmount, 0f, 5f);
            VNTGEditorGUIUtils.DrawClampedSlider(_UnsharpRadius, 0f, 5f);
            VNTGEditorGUIUtils.DrawClampedSlider(_UnsharpThreshold, 0f, 0.5f);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawClampColorSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_ClampBlack, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_ClampWhite, 0f, 1f);
            VNTGEditorGUIUtils.DrawParamField(_ShadowTint);

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

        private void DrawColorAdjustmentSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawClampedSlider(_Gamma, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_Brightness, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_Contrast, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_Saturation, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_Hue, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_RedShift, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_BlueShift, 0f, 1f);
            VNTGEditorGUIUtils.DrawClampedSlider(_GreenShift, 0f, 1f);
            VNTGEditorGUIUtils.DrawToggleButton(_IsMonochrome, "Is Monochrome");

            EditorGUILayout.Space(4);
            EditorGUI.indentLevel--;
        }
    }
}