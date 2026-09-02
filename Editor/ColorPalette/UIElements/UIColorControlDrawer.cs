using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIColorControlElement.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIColorControlElement : VisualElement
    {
        private Palette _palette;
        private int _selectedIndex = -1;
        private StyleSheet _cachedStyleSheet;

        private readonly Toolbar _modeToolbar;
        private readonly List<Button> _tabButtons = new List<Button>();
        private readonly VisualElement _slidersContainer;
        private readonly UIChannelSliderElement[] _sliders;
        private readonly TextField _hexField;

        private ColorFieldsMode _currentFieldsMode = ColorFieldsMode.RGB;
        private readonly string[] _modeTabs = { "RGB", "HSV", "HSL", "LAB", "Hex" };
        private readonly ColorControlCache _sliderCache = new ColorControlCache();

        private bool _isUpdatingInternally;
        public event Action OnPaletteUpdated;

        public UIColorControlElement()
        {
            LoadAndApplyStyleSheet();

            AddToClassList("color-control__main-container");

            _modeToolbar = new Toolbar();
            _modeToolbar.AddToClassList("color-control__mode-toolbar");

            for (int i = 0; i < _modeTabs.Length; i++)
            {
                int modeIndex = i;
                Button tabButton = new Button(() => SwitchMode((ColorFieldsMode)modeIndex))
                {
                    text = _modeTabs[i]
                };

                tabButton.AddToClassList("color-control__mode-tab-button");

                _modeToolbar.Add(tabButton);
                _tabButtons.Add(tabButton);
            }
            Add(_modeToolbar);

            _slidersContainer = new VisualElement();
            Add(_slidersContainer);

            _sliders = new UIChannelSliderElement[4];
            for (int i = 0; i < 4; i++)
            {
                int sliderIndex = i;
                _sliders[i] = new UIChannelSliderElement();
                _sliders[i].OnValueChanged += (val) => OnSliderMoved(sliderIndex, val);
            }

            _hexField = new TextField("Hex Color Value");
            _hexField.AddToClassList("color-control__hex-field");
            _hexField.RegisterValueChangedCallback(evt => OnHexChanged(evt.newValue));
        }

        private void LoadAndApplyStyleSheet()
        {
            if (_cachedStyleSheet != null) return;

            string[] guids = AssetDatabase.FindAssets("ColorControlStyles t:StyleSheet");
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

        public void BindPalette(Palette palette, int selectedIndex)
        {
            if (_selectedIndex != selectedIndex || _palette != palette)
            {
                _sliderCache.Clear();
            }

            _palette = palette;
            _selectedIndex = selectedIndex;
            RefreshLayoutStructure();
        }

        private void SwitchMode(ColorFieldsMode newMode)
        {
            if (_currentFieldsMode == newMode) return;

            _sliderCache.Clear();
            _currentFieldsMode = newMode;

            UpdateTabVisualSelection();
            RefreshLayoutStructure();
        }

        private void UpdateTabVisualSelection()
        {
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (i == (int)_currentFieldsMode)
                {
                    _tabButtons[i].AddToClassList("color-control__mode-tab-button--active");
                }
                else
                {
                    _tabButtons[i].RemoveFromClassList("color-control__mode-tab-button--active");
                }
            }
        }

        private void RefreshLayoutStructure()
        {
            _slidersContainer.Clear();
            if (_palette == null || _selectedIndex < 0 || _selectedIndex >= _palette.colors.Count) return;

            Color currentColor = _palette.colors[_selectedIndex];

            if (_currentFieldsMode == ColorFieldsMode.Hex)
            {
                _isUpdatingInternally = true;
                _hexField.SetValueWithoutNotify("#" + ColorUtility.ToHtmlStringRGB(currentColor));
                _isUpdatingInternally = false;
                _slidersContainer.Add(_hexField);
                return;
            }

            RebuildSliderData(currentColor);

            int activeSlidersCount = (_currentFieldsMode == ColorFieldsMode.RGB) ? 4 : 3;
            for (int i = 0; i < activeSlidersCount; i++)
            {
                _slidersContainer.Add(_sliders[i]);
            }

            UpdateShaderTracks(currentColor);
        }

        private void RebuildSliderData(Color c)
        {
            _isUpdatingInternally = true;

            switch (_currentFieldsMode)
            {
                case ColorFieldsMode.RGB:
                    _sliderCache.GetHSV(c, out _, out _, out float v);
                    _sliders[0].Setup("R", c.r * 255f, 0, 255, 0, asInt: true);
                    _sliders[1].Setup("G", c.g * 255f, 0, 255, 1, asInt: true);
                    _sliders[2].Setup("B", c.b * 255f, 0, 255, 2, asInt: true);
                    _sliders[3].Setup("Brightness", v * 100f, 0, 100, 3, asInt: true);
                    break;

                case ColorFieldsMode.HSV:
                    _sliderCache.GetHSV(c, out float h, out float s, out float hsvV);
                    _sliders[0].Setup("H", h * 360f, 0, 360, 4, asInt: true);
                    _sliders[1].Setup("S", s * 100f, 0, 100, 5, asInt: true);
                    _sliders[2].Setup("V", hsvV * 100f, 0, 100, 6, asInt: true);
                    break;

                case ColorFieldsMode.LAB:
                    _sliderCache.GetLab(c, out float L, out float a, out float b);
                    _sliders[0].Setup("L", L, 0, 100, 8, asInt: true);
                    _sliders[1].Setup("a", a, -128, 127, 9, asInt: true);
                    _sliders[2].Setup("b", b, -128, 127, 10, asInt: true);
                    break;

                case ColorFieldsMode.HSL:
                    _sliderCache.GetHSL(c, out float hslH, out float hslS, out float hslL);
                    _sliders[0].Setup("H", hslH * 360f, 0, 360, 11, asInt: true);
                    _sliders[1].Setup("S", hslS * 100f, 0, 100, 12, asInt: true);
                    _sliders[2].Setup("L", hslL * 100f, 0, 100, 13, asInt: true);
                    break;
            }

            _isUpdatingInternally = false;
        }

        private void UpdateShaderTracks(Color c)
        {
            Vector4 trackContext = GetColorSpaceVector(c, _currentFieldsMode, out bool useDirectColor);
            int activeCount = (_currentFieldsMode == ColorFieldsMode.RGB) ? 4 : 3;

            for (int i = 0; i < activeCount; i++)
            {
                bool useRawColor = useDirectColor && i < 3;
                _sliders[i].UpdateShaderTrack(useRawColor ? (Vector4)c : trackContext);
            }
        }

        private void OnSliderMoved(int sliderIndex, float value)
        {
            if (_isUpdatingInternally || _palette == null) return;

            Color targetColor = _palette.colors[_selectedIndex];

            switch (_currentFieldsMode)
            {
                case ColorFieldsMode.RGB:
                    if (sliderIndex < 3)
                    {
                        targetColor[sliderIndex] = value / 255f;
                    }
                    else
                    {
                        _sliderCache.GetHSV(targetColor, out float h, out float s, out _);
                        targetColor = Color.HSVToRGB(h, s, value / 100f);
                        _sliderCache.UpdateHSV(h, s, value / 100f, targetColor);
                    }
                    break;

                case ColorFieldsMode.HSV:
                    _sliderCache.GetHSV(targetColor, out float currentH, out float currentS, out float currentV);
                    float nextH = (sliderIndex == 0) ? value / 360f : currentH;
                    float nextS = (sliderIndex == 1) ? value / 100f : currentS;
                    float nextV = (sliderIndex == 2) ? value / 100f : currentV;
                    targetColor = Color.HSVToRGB(nextH, nextS, nextV);
                    _sliderCache.UpdateHSV(nextH, nextS, nextV, targetColor);
                    break;

                case ColorFieldsMode.LAB:
                    _sliderCache.GetLab(targetColor, out float labL, out float labA, out float labB);
                    if (sliderIndex == 0) labL = value;
                    if (sliderIndex == 1) labA = value;
                    if (sliderIndex == 2) labB = value;
                    targetColor = VNTGColorSpace.LabToRGB(labL, labA, labB).Clamp01();
                    _sliderCache.UpdateLab(labL, labA, labB, targetColor);
                    break;

                case ColorFieldsMode.HSL:
                    _sliderCache.GetHSL(targetColor, out float hslH, out float hslS, out float hslL);
                    if (sliderIndex == 0) hslH = value / 360f;
                    if (sliderIndex == 1) hslS = value / 100f;
                    if (sliderIndex == 2) hslL = value / 100f;

                    targetColor = VNTGColorSpace.HslToRGB(hslH, hslS, hslL).Clamp01();
                    _sliderCache.UpdateHSL(hslH, hslS, hslL, targetColor);
                    break;
            }

            ApplyColorMutation(targetColor);

            _isUpdatingInternally = true;
            RebuildSliderData(targetColor);
            UpdateShaderTracks(targetColor);
            _isUpdatingInternally = false;
        }

        private Vector4 GetColorSpaceVector(Color c, ColorFieldsMode mode, out bool isRgbBase)
        {
            isRgbBase = false;
            if (mode == ColorFieldsMode.RGB)
            {
                isRgbBase = true;
                _sliderCache.GetHSV(c, out float h, out float s, out float v);
                return new Vector4(h, s, v, 1f);
            }
            if (mode == ColorFieldsMode.HSV)
            {
                _sliderCache.GetHSV(c, out float h, out float s, out float v);
                return new Vector4(h, s, v, 1f);
            }
            if (mode == ColorFieldsMode.LAB)
            {
                _sliderCache.GetLab(c, out float L, out float a, out float b);
                return new Vector4(L, a, b, 1f);
            }
            if (mode == ColorFieldsMode.HSL)
            {
                _sliderCache.GetHSL(c, out float h, out float s, out float l);
                return new Vector4(h, s, l, 1f);
            }
            return Vector4.one;
        }

        private void OnHexChanged(string newHex)
        {
            if (_isUpdatingInternally || _palette == null) return;

            if (ColorUtility.TryParseHtmlString(newHex, out Color parsedColor))
            {
                _sliderCache.Clear();
                ApplyColorMutation(parsedColor);
            }
        }

        private void ApplyColorMutation(Color finalColor)
        {
            finalColor.a = _palette.colors[_selectedIndex].a;

            Undo.RecordObject(_palette, "Changed Color");
            _palette.colors[_selectedIndex] = finalColor;
            EditorUtility.SetDirty(_palette);

            OnPaletteUpdated?.Invoke();
        }

        public void Cleanup()
        {
            for (int i = 0; i < 4; i++)
            {
                _sliders[i]?.Cleanup();
            }
        }
    }
}