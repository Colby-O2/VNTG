using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    [CustomEditor(typeof(Palette))]
    public class PaletteEditor : UnityEditor.Editor
    {
#if UNITY_6000_4_OR_NEWER
        private static readonly HashSet<EntityId> PendingAutoBakes = new HashSet<EntityId>();
#else
        private static readonly HashSet<int> PendingAutoBakes = new HashSet<int>();
#endif

        private VisualElement _root;
        private UIPaletteColorPicker _colorWheelUI;
        private UIColorControlElement _colorControlUI;
        private UIPaletteGridElement _paletteGridUI;
        private UIPaletteView _paletteViewUI;
        private Palette _palette;

        private string _initialDiskSnapshotStr = string.Empty;
        private bool _hasUnbakedChanges;
        private bool _isBaking;

        private Button _bakeButton;
        private Button _revertButton;
        private Button _exportButton;

        private SelectionState _masterSelectedColorsList = new SelectionState();

        public void OnEnable()
        {
            _palette = target as Palette;
            if (_palette == null) return;

            for (int i = 0; i < _palette.colors.Count; i++)
            {
                _masterSelectedColorsList.Add(i);
            }

            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            if (!CheckIfImportedPalette(_palette))
            {
                EditorApplication.delayCall += () =>
                {
                    if (_palette != null && !CheckIfImportedPalette(_palette))
                    {
                        CheckAndBakeMissingLUT(_palette);
                        CachePristineState(_palette);
                        _hasUnbakedChanges = false;
                    }
                };
            }

            CachePristineState(_palette);
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            _colorWheelUI?.Cleanup();
            _colorControlUI?.Cleanup();

            if (_palette != null && _hasUnbakedChanges && !_isBaking)
            {
#if UNITY_6000_4_OR_NEWER
                EntityId paletteId = _palette.GetEntityId();
#else
                int paletteId = _palette.GetInstanceID();
#endif

                if (!PendingAutoBakes.Contains(paletteId))
                {
                    PendingAutoBakes.Add(paletteId);
                    _hasUnbakedChanges = false;

                    var targetPalette = _palette;

                    EditorApplication.delayCall += () =>
                    {
                        PendingAutoBakes.Remove(paletteId);

                        if (targetPalette != null)
                        {
                            BakeInternalForPalette(targetPalette);
                        }
                    };
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            if (_palette == null) return _root;

            if (CheckIfImportedPalette(_palette))
            {
                _root.Add(new IMGUIContainer(() => DrawDefaultInspector()));
                return _root;
            }

#if !UNITY_6000_3_OR_NEWER
            // Some part of the UIToolkit is causing the color wheel to break on Unity version 6.2 or lower (unsupported feature). 
            BuildHeaderAndProperties();
            var colorsProp = serializedObject.FindProperty("colors");
            if (colorsProp != null)
            {
                var propertyField = new PropertyField(colorsProp);
                propertyField.style.marginBottom = 15;
                propertyField.Bind(serializedObject);

                _root.TrackPropertyValue(colorsProp, (prop) =>
                {
                    serializedObject.ApplyModifiedProperties();
                    EvaluateChangeState();
                });

                _root.Add(propertyField);
            }
            BuildControlFooter();
            BuildPreview();
            return _root;
#else

            BuildHeaderAndProperties();
            InitializeSubElements();
            BindEventPipelines();
            BuildControlFooter();
            BuildPreview();

            return _root;
#endif
        }

        private void BuildHeaderAndProperties()
        {
            var distanceMetricProp = serializedObject.FindProperty("distanceMetric");
            if (distanceMetricProp != null)
            {
                var propertyField = new PropertyField(distanceMetricProp);
                propertyField.style.marginBottom = 15;
                propertyField.Bind(serializedObject);

                propertyField.RegisterValueChangeCallback((evt) =>
                {
                    serializedObject.ApplyModifiedProperties();
                    EvaluateChangeState();
                    if (_hasUnbakedChanges)
                    {
                        BakeInternal();
                    }
                });

                _root.Add(propertyField);
            }
        }

        private void InitializeSubElements()
        {
            _colorWheelUI = new UIPaletteColorPicker { Selected = _masterSelectedColorsList };
            _colorWheelUI.BindPalette(_palette);
            _root.Add(_colorWheelUI);

            _colorControlUI = new UIColorControlElement();
            _colorControlUI.BindPalette(_palette, _masterSelectedColorsList.GetPrimarySelection());
            _root.Add(_colorControlUI);

            _paletteGridUI = new UIPaletteGridElement();
            _paletteGridUI.BindPalette(_palette, _masterSelectedColorsList);
            _root.Add(_paletteGridUI);
        }

        private void BindEventPipelines()
        {
            _paletteGridUI.OnSelectionChanged += (newIndex) =>
            {
                _colorWheelUI.RefreshCacheAndTextures();
                _colorControlUI.BindPalette(_palette, newIndex);
            };

            _paletteGridUI.OnSwatchHoverChanged += (hoveredIndex) =>
            {
                if (_colorWheelUI.HoveringIndex != hoveredIndex)
                {
                    _colorWheelUI.HoveringIndex = hoveredIndex;
                    _colorWheelUI.MarkDirtyRepaint();
                }
            };

            _paletteGridUI.OnPaletteUpdated += () => SyncStateChanges(refreshWheel: true, refreshGrid: false, refreshControls: true);
            _colorWheelUI.OnPaletteUpdated += () => SyncStateChanges(refreshWheel: false, refreshGrid: true, refreshControls: true);
            _colorControlUI.OnPaletteUpdated += () => SyncStateChanges(refreshWheel: true, refreshGrid: true, refreshControls: false);
        }

        private void SyncStateChanges(bool refreshWheel, bool refreshGrid, bool refreshControls)
        {
            serializedObject.Update();
            EvaluateChangeState();

            if (refreshWheel) _colorWheelUI.RefreshCacheAndTextures();
            if (refreshGrid) _paletteGridUI.BindPalette(_palette, _masterSelectedColorsList);
            if (refreshControls) _colorControlUI.BindPalette(_palette, _masterSelectedColorsList.GetPrimarySelection());
        }

        private void BuildControlFooter()
        {
            _bakeButton = new Button(BakePaletteData)
            {
                text = "Bake Palette",
                style = { height = 30, marginTop = 15 }
            };
            _root.Add(_bakeButton);

            _revertButton = new Button(RevertPaletteData)
            {
                text = "Revert Palette",
                style = { height = 30, marginTop = 4, backgroundColor = new Color(0.75f, 0.35f, 0.35f, 1f) }
            };
            _root.Add(_revertButton);

            _exportButton = new Button(ExportPaletteData)
            {
                text = "Export Palette",
                style = { height = 30, marginTop = 4 }
            };
            _root.Add(_exportButton);

            UpdateActionButtonVisuals();
        }

        private void BuildPreview()
        {
            VisualElement divider = new VisualElement();
            divider.style.height = 1;
            divider.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            divider.style.marginTop = 8;
            divider.style.marginBottom = 8;

            _root.Add(divider);

            _paletteViewUI = new UIPaletteView();
            _paletteViewUI.BindPaletteContext(_palette);
            _root.Add(_paletteViewUI);
        }

        private void ExportPaletteData()
        {
            if (_palette == null || _palette.colors == null || _palette.colors.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "The current palette contains no color data to export.", "OK");
                return;
            }

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Raw Hex Text (*.hex)"), false, () => OpenSavePanel("hex"));
            menu.AddItem(new GUIContent("Paint.NET Palette (*.txt)"), false, () => OpenSavePanel("txt"));
            menu.AddItem(new GUIContent("GIMP Palette (*.gpl)"), false, () => OpenSavePanel("gpl"));
            menu.AddItem(new GUIContent("Adobe Swatch Exchange (*.ase)"), false, () => OpenSavePanel("ase"));
            menu.AddItem(new GUIContent("JASC PAL (*.pal)"), false, () => OpenSavePanel("pal"));

            menu.ShowAsContext();
        }

        private void OpenSavePanel(string extension)
        {
            string chosenPath = EditorUtility.SaveFilePanel(
              $"Export Palette as {extension.ToUpper()}",
              Application.dataPath,
              _palette.name,
              extension
            );

            if (string.IsNullOrEmpty(chosenPath)) return;

            try
            {
                switch (extension)
                {
                    case "hex":
                        PaletteWriter.WriteHex(chosenPath, _palette.colors);
                        break;
                    case "txt":
                        PaletteWriter.WritePaintNet(chosenPath, _palette.name, _palette.colors);
                        break;
                    case "gpl":
                        PaletteWriter.WriteGpl(chosenPath, _palette.name, _palette.colors);
                        break;
                    case "ase":
                        PaletteWriter.WriteAse(chosenPath, _palette.colors);
                        break;
                    case "pal":
                        PaletteWriter.WriteJascPal(chosenPath, _palette.colors);
                        break;
                }

                Debug.Log($"<color=#5fc976><b>[VNTG]</b></color> Successfully exported palette down to: {chosenPath}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Export Failed", $"An error occurred while saving the palette:\n{ex.Message}", "OK");
            }
        }

        private void EvaluateChangeState()
        {
            string currentStateStr = JsonUtility.ToJson(_palette);
            _hasUnbakedChanges = (currentStateStr != _initialDiskSnapshotStr);
            UpdateActionButtonVisuals();
        }

        private void UpdateActionButtonVisuals()
        {
            if (_bakeButton == null || _revertButton == null) return;

            if (_hasUnbakedChanges)
            {
                _bakeButton.text = "Bake Palette (Unsaved Changes)";
                _bakeButton.style.backgroundColor = new Color(0.35f, 0.75f, 0.35f, 1f);
                _revertButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _bakeButton.text = "Bake Palette";
                _bakeButton.style.backgroundColor = StyleKeyword.Null;
                _revertButton.style.display = DisplayStyle.None;
            }
        }

        private void BakePaletteData()
        {
            if (_palette == null) return;

            BakeInternalForPalette(_palette);
            UpdateActionButtonVisuals();
        }

        private void BakeInternal()
        {
            BakeInternalForPalette(_palette);
        }

        private void BakeInternalForPalette(Palette palette)
        {
            if (_isBaking || palette == null)
                return;

            try
            {
                _isBaking = true;
                _hasUnbakedChanges = false;

                EditorUtility.SetDirty(palette);
                LUTBaker.Rebake(palette);

                EditorApplication.delayCall += () =>
                {
                    if (palette != null)
                    {
                        AssetDatabase.SaveAssetIfDirty(palette);
                    }
                };

                CachePristineState(palette);
                UpdateActionButtonVisuals();
            }
            finally
            {
                _isBaking = false;
            }
        }

        private void RevertPaletteData()
        {
            if (_palette == null) return;

            Undo.RegisterCompleteObjectUndo(_palette, "Revert Palette");
            JsonUtility.FromJsonOverwrite(_initialDiskSnapshotStr, _palette);

            BakeInternal();

            CachePristineState(_palette);
            _hasUnbakedChanges = false;

            serializedObject.Update();
            RefreshInspectorLayout();
            UpdateActionButtonVisuals();
        }

        private void RefreshInspectorLayout()
        {
            if (_palette == null) return;

            int activeIndex = _colorWheelUI != null ? _colorWheelUI.Selected.GetPrimarySelection() : 0;
            if (activeIndex >= _palette.colors.Count) activeIndex = _palette.colors.Count - 1;

            _colorWheelUI?.RefreshCacheAndTextures();
            _colorWheelUI?.MarkDirtyRepaint();

            _paletteGridUI?.BindPalette(_palette, _masterSelectedColorsList);
            _colorControlUI?.BindPalette(_palette, _masterSelectedColorsList.GetPrimarySelection());
        }

        private void OnUndoRedoPerformed()
        {
            EditorApplication.delayCall += () =>
            {
                if (_palette == null) return;
                serializedObject.Update();
                EvaluateChangeState();
                RefreshInspectorLayout();
            };
        }

        private void CachePristineState(Palette palette)
        {
            if (palette != null)
            {
                _initialDiskSnapshotStr = JsonUtility.ToJson(palette);
            }
        }

        private bool CheckIfImportedPalette(Palette palette)
        {
            string assetPath = AssetDatabase.GetAssetPath(palette);
            if (string.IsNullOrEmpty(assetPath)) return false;

            string ext = Path.GetExtension(assetPath).ToLower().Replace(".", "");
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            return AssetDatabase.IsSubAsset(palette) || importer is ScriptedImporter || ext != "asset";
        }

        private void CheckAndBakeMissingLUT(Palette palette)
        {
            string path = AssetDatabase.GetAssetPath(palette);
            if ((!string.IsNullOrEmpty(path) || EditorUtility.IsPersistent(palette)) && !palette.HasLUT())
            {
                LUTBaker.Rebake(palette);
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (target is not Palette palette) return null;

            IconSettings iconSettings = new IconSettings(width, height);
            int totalPixels = width * height;

            Texture2D previewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            NativeArray<Color32> previewPixelBuffer = new NativeArray<Color32>(totalPixels, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            Vector2 center = new Vector2(width, height) * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = y * width + x;
                    Vector2 delta = new Vector2(x + 0.5f, y + 0.5f) - center;
                    float distFromCenter = delta.magnitude;

                    float sdf = TextureGenerationUtility.ChamferedRectSDF(
                      delta,
                      iconSettings.CardHalfSize,
                      iconSettings.CardCornerRadius,
                      iconSettings.CardFoldSize
                    );

                    if (sdf <= 0)
                    {
                        previewPixelBuffer[index] = (sdf > -iconSettings.OutlineSize)
                          ? iconSettings.OutlineColor
                          : TextureGenerationUtility.SamplePreviewColorWheel(
                            delta, distFromCenter, iconSettings.WheelRadius, iconSettings.OutlineSize,
                            iconSettings.OutlineColor, iconSettings.FileColor, palette.colors
                          );
                    }
                    else
                    {
                        previewPixelBuffer[index] = iconSettings.BackgroundColor;
                    }
                }
            }

            previewTexture.SetPixelData(previewPixelBuffer, 0);
            previewPixelBuffer.Dispose();

            previewTexture.Apply();
            return previewTexture;
        }

        private struct IconSettings
        {
            public Color FileColor;
            public Color OutlineColor;
            public Color BackgroundColor;
            public float OutlineSize;
            public float WheelRadius;
            public Vector2 CardHalfSize;
            public float CardCornerRadius;
            public float CardFoldSize;

            public IconSettings(int width, int height)
            {
                FileColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                OutlineColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                OutlineSize = 2.5f;
                WheelRadius = width * 0.28f;
                CardHalfSize = new Vector2(width * 0.38f, height * 0.42f);
                CardCornerRadius = width * 0.06f;
                CardFoldSize = width * 0.22f;
            }
        }
    }
}