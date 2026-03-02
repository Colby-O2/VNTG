using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using TMPro;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    VolumeComponentUICreator.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    public class VolumeComponentUICreator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Transform _container;

        [Header("Effect")]
        [SerializeField] private string _targetEffectName = "CRTSettings";

        [Header("Prefabs")]
        [SerializeField] private GameObject _sliderRowPrefab;
        [SerializeField] private GameObject _toggleRowPrefab;
        [SerializeField] private GameObject _headerPrefab;
        [SerializeField] private GameObject _dropdownRowPrefab; 
        [SerializeField] private GameObject _vector2RowPrefab;
        [SerializeField] private GameObject _colorRowPrefab;

        private VolumeComponent _settings;

        private void Start()
        {
            RefreshUI(_targetEffectName);
        }

        private void GenerateUI()
        {
            FieldInfo[] fields = _settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                HeaderAttribute header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null) CreateHeader(header.header);

                if (typeof(VolumeParameter).IsAssignableFrom(field.FieldType))
                {
                    if (field.Name == "ShowInSceneView") continue;
                    CreateControl(field);
                }
            }
        }

        public void RefreshUI(string effectName)
        {
            foreach (Transform child in _container)
            {
                if (child.gameObject) Destroy(child.gameObject);
            }

            _targetEffectName = effectName;

            _title.text = (_targetEffectName == "CRTSettings") ? "CRT Post-Processing Settings" : "PSX Post-Processing Settings";

            Volume volume = FindFirstObjectByType<Volume>();
            if (volume != null && volume.profile != null)
            {
                foreach (VolumeComponent component in volume.profile.components)
                {
                    if (component.GetType().Name == _targetEffectName)
                    {
                        _settings = component;
                        GenerateUI();
                        break;
                    }
                }
            }
        }

        private void CreateControl(FieldInfo field)
        {
            string displayName = GetNameWithSpaces(field.Name);
            object paramValue = field.GetValue(_settings);

            if (paramValue is BoolParameter boolParam)
            {
                GameObject row = Instantiate(_toggleRowPrefab, _container);
                row.GetComponentInChildren<TMP_Text>().text = displayName;
                Toggle t = row.GetComponentInChildren<Toggle>();
                t.isOn = boolParam.value;
                t.onValueChanged.AddListener((val) => boolParam.value = val);
            }
            else if (paramValue is ClampedFloatParameter floatParam)
            {
                CreateSlider(displayName, floatParam.min, floatParam.max, floatParam.value, (val) => floatParam.value = val);
            }
            else if (paramValue is ClampedIntParameter intParam)
            {
                CreateSlider(displayName, intParam.min, intParam.max, intParam.value, (val) => intParam.value = (int)val);
            }
            else if (paramValue is Vector2Parameter vec2Param)
            {
                CreateVector2UI(displayName, vec2Param);
            }
            else if (paramValue is ColorParameter colorParam)
            {
                CreateColorUI(displayName, colorParam);
            }
            else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(EnumParameter<>))
            {
                CreateEnumDropdown(displayName, paramValue);
            }
        }

        private void CreateColorUI(string label, ColorParameter param)
        {
            GameObject row = Instantiate(_colorRowPrefab, _container);
            row.GetComponentInChildren<TMP_Text>().text = label;

            Image preview = row.transform.Find("Preview")?.GetComponent<Image>();
            if (preview != null) preview.color = param.value;

            Slider[] sliders = row.GetComponentsInChildren<Slider>();
            if (sliders.Length >= 3)
            {
                sliders[0].value = param.value.r;
                sliders[1].value = param.value.g;
                sliders[2].value = param.value.b;

                for (int i = 0; i < 3; i++)
                {
                    sliders[i].onValueChanged.AddListener((val) => {
                        param.value = new Color(sliders[0].value, sliders[1].value, sliders[2].value, 1);
                        if (preview != null) preview.color = param.value;
                    });
                }
            }
        }

        private void CreateVector2UI(string label, Vector2Parameter param)
        {
            GameObject row = Instantiate(_vector2RowPrefab, _container);
            row.GetComponentInChildren<TMP_Text>().text = label;

            TMP_InputField[] inputs = row.GetComponentsInChildren<TMP_InputField>();
            if (inputs.Length >= 2)
            {
                inputs[0].text = param.value.x.ToString();
                inputs[1].text = param.value.y.ToString();

                inputs[0].onEndEdit.AddListener((val) => { if (float.TryParse(val, out float res)) param.value = new Vector2(res, param.value.y); });
                inputs[1].onEndEdit.AddListener((val) => { if (float.TryParse(val, out float res)) param.value = new Vector2(param.value.x, res); });
            }
        }

        private void CreateEnumDropdown(string label, object paramValue)
        {
            GameObject row = Instantiate(_dropdownRowPrefab, _container);
            row.GetComponentInChildren<TMP_Text>().text = label;
            TMP_Dropdown dropdown = row.GetComponentInChildren<TMP_Dropdown>();

            PropertyInfo valueProp = paramValue.GetType().GetProperty("value");
            Enum currentEnum = (Enum)valueProp.GetValue(paramValue);

            string[] names = Enum.GetNames(currentEnum.GetType());
            List<string> options = new List<string>(names);

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = Array.IndexOf(names, currentEnum.ToString());

            dropdown.onValueChanged.AddListener((index) =>
            {
                object enumVal = Enum.Parse(currentEnum.GetType(), names[index]);
                valueProp.SetValue(paramValue, enumVal);
            });
        }

        private void CreateHeader(string label)
        {
            GameObject headerObj = Instantiate(_headerPrefab, _container);
            headerObj.GetComponentInChildren<TMP_Text>().text = label.ToUpper();
        }

       private string GetNameWithSpaces(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            return Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2");
        }

        private void CreateSlider(string label, float min, float max, float startVal, UnityEngine.Events.UnityAction<float> callback)
        {
            GameObject row = Instantiate(_sliderRowPrefab, _container);
            row.GetComponentInChildren<TMP_Text>().text = label;
            Slider s = row.GetComponentInChildren<Slider>();
            s.minValue = min;
            s.maxValue = max;
            s.value = startVal;
            s.onValueChanged.AddListener(callback);
        }
    }
}