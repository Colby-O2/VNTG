using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIPaletteColorPicker.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    [UxmlElement]
    public partial class UIPaletteColorPicker : VisualElement
    {
        private readonly UIBaseColorWheel _satWheel;
        private readonly UIColorBar _lightnessBar;
        private readonly UIBaseColorWheel _valWheel;

        private Palette _palette;
        private readonly ColorWheelCache _cache = new ColorWheelCache();
        private static StyleSheet _cachedStyleSheet;

        public SelectionState Selected { get; set; } = new SelectionState();
        public event Action OnPaletteUpdated;

        public int HoveringIndex
        {
            get => _satWheel.HoveringIndex;
            set
            {
                _satWheel.HoveringIndex = value;
                _valWheel.HoveringIndex = value;
            }
        }

        public UIPaletteColorPicker()
        {
            LoadAndApplyStyleSheet();

            AddToClassList("color-picker__orchestrator");

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            _satWheel = new UISaturationColorWheel(_cache);
            _lightnessBar = new UIColorBar();
            _valWheel = new UIValueColorWheel(_cache);

            Add(_satWheel);
            Add(_lightnessBar);
            Add(_valWheel);

            _satWheel.OnColorModified += HandleColorModification;
            _valWheel.OnColorModified += HandleColorModification;
            _lightnessBar.OnValueChanged += HandleLightnessModification;

            _satWheel.OnSelectionChanged += HandleSelectionChanged;
            _valWheel.OnSelectionChanged += HandleSelectionChanged;

            _satWheel.OnHoverChanged += index => HoveringIndex = index;
            _valWheel.OnHoverChanged += index => HoveringIndex = index;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void LoadAndApplyStyleSheet()
        {
            if (_cachedStyleSheet != null)
            {
                styleSheets.Add(_cachedStyleSheet);
                return;
            }

            string[] guids = AssetDatabase.FindAssets("ColorWheelStyles t:StyleSheet");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (_cachedStyleSheet != null)
                {
                    styleSheets.Add(_cachedStyleSheet);
                }
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RegenerateTextures();
        }

        public void BindPalette(Palette palette)
        {
            _palette = palette;
            RefreshCacheAndTextures();
        }

        public void RefreshCacheAndTextures()
        {
            if (_palette == null) return;
            _cache.Sync(_palette.colors);
            RegenerateTextures();
        }

        public void RegenerateTextures()
        {
            if (_palette == null) return;

            bool hasSelected = !Selected.IsEmpty();
            int primarySelection = Selected.GetPrimarySelection();

            if (primarySelection < 0 || primarySelection >= _palette.colors.Count) return;

            Vector3 selectedHSV = hasSelected ? _cache.GetHSV(primarySelection) : new Vector3(0f, 1f, 1f);

            _satWheel.UpdateVisuals(_palette.colors, primarySelection, selectedHSV.z, Selected);
            _valWheel.UpdateVisuals(_palette.colors, primarySelection, selectedHSV.y, Selected);

            if (hasSelected)
            {
                VNTGColorSpace.HSVToHSL(selectedHSV.x, selectedHSV.y, selectedHSV.z, out _, out _, out float lightness);
                _lightnessBar.UpdateVisuals(selectedHSV, lightness, true);
            }
            else
            {
                _lightnessBar.UpdateVisuals(selectedHSV, 0.5f, false);
            }
        }

        private void HandleSelectionChanged(int index)
        {
            Selected.SetPrimarySelection(index);
            OnPaletteUpdated?.Invoke();
            RegenerateTextures();
        }

        private void HandleColorModification(int index, Vector3 newHSV)
        {
            if (_palette == null || index < 0 || index >= _palette.colors.Count) return;

            _cache.SetHSV(index, newHSV);
            Color newColor = Color.HSVToRGB(newHSV.x, newHSV.y, newHSV.z);

            Undo.RecordObject(_palette, "Changed Color");
            _palette.colors[index] = newColor;
            EditorUtility.SetDirty(_palette);

            OnPaletteUpdated?.Invoke();
            RegenerateTextures();
        }

        private void HandleLightnessModification(float newLightness)
        {
            if (_palette == null || Selected.IsEmpty()) return;

            int primaryIndex = Selected.GetPrimarySelection();

            Vector3 hsv = _cache.GetHSV(primaryIndex);
            VNTGColorSpace.HSVToHSL(hsv.x, hsv.y, hsv.z, out float h, out float s, out _);
            VNTGColorSpace.HSLToHSV(h, s, newLightness, out _, out float newS, out float newV);

            HandleColorModification(primaryIndex, new Vector3(hsv.x, newS, newV));
        }

        public void Cleanup()
        {
            _satWheel.Cleanup();
            _lightnessBar.Cleanup();
            _valWheel.Cleanup();
        }
    }
}