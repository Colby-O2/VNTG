using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    VNTGEditorGUIUtils.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX.Editor
{
    public static class VNTGEditorGUIUtils
    {
        private static readonly Dictionary<string, SerializedPropertyCopy> _cachedUserValues = new Dictionary<string, SerializedPropertyCopy>();

        private struct SerializedPropertyCopy
        {
            public SerializedPropertyType Type;
            public float FloatValue;
            public int IntValue;
            public bool BoolValue;
            public Vector2 Vector2Value;
            public Color ColorValue;
        }

        public static bool DrawSectionHeader(bool foldoutState, string headerTitle, System.Action resetCallback = null)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, 22f);

            float resetWidth = 45f;
            float minSpacing = 6f;
            float safeRightEdge = EditorGUIUtility.currentViewWidth - 18f;
            float currentRight = Mathf.Min(headerRect.xMax, safeRightEdge);

            if (resetCallback != null)
            {
                currentRight -= resetWidth;
                Rect resetRect = new Rect(currentRight, headerRect.y + 2f, resetWidth, 18f);

                if (GUI.Button(resetRect, "Reset", EditorStyles.miniButton))
                {
                    resetCallback.Invoke();
                }

                currentRight -= minSpacing;
            }

            float foldoutWidth = Mathf.Max(0f, currentRight - headerRect.x);
            Rect foldoutRect = new Rect(headerRect.x, headerRect.y + 2f, foldoutWidth, 18f);

            GUIContent labelContent = new GUIContent(headerTitle);
            return EditorGUI.Foldout(foldoutRect, foldoutState, labelContent, true, EditorStyles.foldout);
        }

        public static bool DrawSectionHeaderWithToggle(
            bool foldoutState,
            string headerTitle,
            SerializedDataParameter toggleParam,
            Action resetCallback = null
        )
        {
            return DrawHeaderWithToggleUI(foldoutState, headerTitle, toggleParam, p => p.value, resetCallback);
        }

        public static bool DrawSectionHeaderWithOverrideState(
            bool foldoutState,
            string headerTitle,
            SerializedDataParameter targetParam,
            Action resetCallback = null
        )
        {
            return DrawHeaderWithToggleUI(foldoutState, headerTitle, targetParam, p => p.overrideState, resetCallback);
        }

        private static bool DrawHeaderWithToggleUI(
            bool foldoutState,
            string headerTitle,
            SerializedDataParameter param,
            Func<SerializedDataParameter, SerializedProperty> propertySelector,
            Action resetCallback = null
        )
        {
            Rect totalRect = EditorGUILayout.GetControlRect(false, 22f);

            bool isToggleOn = true;
            bool showResetButton = resetCallback != null;

            float safeRightEdge = EditorGUIUtility.currentViewWidth - 18f;
            float currentRight = Mathf.Min(totalRect.xMax, safeRightEdge);

            float resetWidth = 45f;
            float minSpacing = 6f;

            float fixedShortWidth = 40f;

            float globalDisableThreshold = totalRect.x + 120f;

            Rect resetRect = Rect.zero;
            if (showResetButton)
            {
                currentRight -= resetWidth;
                resetRect = new Rect(currentRight, totalRect.y + 2f, resetWidth, 18f);
                currentRight -= minSpacing;
            }

            Rect toggleRect = Rect.zero;
            bool drawToggle = false;
            bool useShortText = false;

            if (param != null)
            {
                SerializedProperty boolProp = propertySelector(param);
                if (boolProp != null)
                {
                    bool currentState = boolProp.boolValue;
                    string fullText = $"{headerTitle}: {(currentState ? "ON" : "OFF")}";

                    float fullTextWidth = EditorStyles.miniButton.CalcSize(new GUIContent(fullText)).x + 10f;

                    float spaceForToggle = currentRight - totalRect.x;

                    if (currentRight >= globalDisableThreshold && spaceForToggle >= fixedShortWidth)
                    {
                        if (spaceForToggle >= fullTextWidth + 100f)
                        {
                            float toggleWidth = fullTextWidth;
                            currentRight -= toggleWidth;
                            toggleRect = new Rect(currentRight, totalRect.y + 2f, toggleWidth, 18f);
                            drawToggle = true;
                            useShortText = false;
                        }
                        else
                        {
                            currentRight -= fixedShortWidth;
                            toggleRect = new Rect(currentRight, totalRect.y + 2f, fixedShortWidth, 18f);
                            drawToggle = true;
                            useShortText = true;
                        }
                    }
                    else
                    {
                        drawToggle = false;
                    }
                }
            }

            float foldoutWidth = Mathf.Max(0f, currentRight - totalRect.x - minSpacing);
            Rect foldoutRect = new Rect(totalRect.x, totalRect.y + 2f, foldoutWidth, 18f);

            if (param != null)
            {
                SerializedProperty boolProp = propertySelector(param);
                if (boolProp != null)
                {
                    bool currentState = boolProp.boolValue;

                    if (drawToggle)
                    {
                        GUIStyle toggleStyle = new GUIStyle(EditorStyles.miniButton)
                        {
                            fontStyle = currentState ? FontStyle.Bold : FontStyle.Normal
                        };

                        EditorGUI.BeginChangeCheck();

                        string buttonText = useShortText
                            ? (currentState ? "ON" : "OFF")
                            : $"{headerTitle}: {(currentState ? "ON" : "OFF")}";

                        bool newState = GUI.Toggle(toggleRect, currentState, buttonText, toggleStyle);

                        if (EditorGUI.EndChangeCheck())
                        {
                            boolProp.boolValue = newState;
                            if (newState) foldoutState = true;
                        }
                    }

                    isToggleOn = boolProp.boolValue;
                }
            }

            if (showResetButton && drawToggle)
            {
                if (GUI.Button(resetRect, "Reset", EditorStyles.miniButton))
                {
                    resetCallback?.Invoke();
                }
            }

            GUIContent labelContent = new GUIContent(headerTitle);
            using (new EditorGUI.DisabledScope(!isToggleOn))
            {
                return EditorGUI.Foldout(foldoutRect, foldoutState, labelContent, true, EditorStyles.foldout);
            }
        }

        public static void DrawEnumToolbarParameter<TEnum>(
                SerializedDataParameter param,
                string label,
                string[] options,
                Func<int, bool> isOptionDisabled = null
            ) where TEnum : Enum
        {
            if (param == null || options == null || options.Length == 0) return;

            SerializedProperty enumProp = param.value;

            Rect totalRect = EditorGUILayout.GetControlRect(false, 18f);

            float safeRightEdge = EditorGUIUtility.currentViewWidth - 18f;
            float currentRight = Mathf.Min(totalRect.xMax, safeRightEdge);

            float labelWidth = EditorGUIUtility.labelWidth - 4f;
            Rect labelRect = new Rect(totalRect.x, totalRect.y, labelWidth, totalRect.height);

            float controlX = totalRect.x + labelWidth;
            float controlWidth = Mathf.Max(0f, currentRight - controlX);
            Rect controlRect = new Rect(controlX, totalRect.y, controlWidth, totalRect.height);

            EditorGUI.LabelField(labelRect, label);

            float minWidthPerItem = 55f;
            float requiredToolbarWidth = options.Length * minWidthPerItem;

            int currentSelection = enumProp.intValue;
            int newSelection = currentSelection;

            EditorGUI.BeginChangeCheck();

            if (controlWidth < requiredToolbarWidth)
            {
                List<GUIContent> activeOptions = new List<GUIContent>();
                List<int> originalIndices = new List<int>();

                for (int i = 0; i < options.Length; i++)
                {
                    if (isOptionDisabled == null || !isOptionDisabled(i))
                    {
                        string prettyEnumName = GetPrettyEnumName<TEnum>(i);
                        activeOptions.Add(new GUIContent(options[i], prettyEnumName));
                        originalIndices.Add(i);
                    }
                }

                int activePopupIndex = originalIndices.IndexOf(currentSelection);
                if (activePopupIndex == -1 && activeOptions.Count > 0)
                {
                    activePopupIndex = 0;
                }

                if (activeOptions.Count > 0)
                {
                    int newPopupIndex = EditorGUI.Popup(controlRect, activePopupIndex, activeOptions.ToArray());
                    newSelection = originalIndices[newPopupIndex];
                }
            }
            else
            {
                float buttonWidth = controlWidth / options.Length;

                for (int i = 0; i < options.Length; i++)
                {
                    Rect btnRect = new Rect(controlX + (i * buttonWidth), controlRect.y, buttonWidth, controlRect.height);

                    GUIStyle style = (i == 0) ? EditorStyles.miniButtonLeft :
                                     (i == options.Length - 1) ? EditorStyles.miniButtonRight :
                                     EditorStyles.miniButtonMid;

                    bool isTabDisabled = isOptionDisabled != null && isOptionDisabled(i);
                    bool isActive = (currentSelection == i);

                    string prettyEnumName = GetPrettyEnumName<TEnum>(i);

                    using (new EditorGUI.DisabledScope(isTabDisabled))
                    {
                        if (GUI.Toggle(btnRect, isActive, options[i], style) && !isActive)
                        {
                            newSelection = i;
                        }

                        GUIContent tooltipContent = new GUIContent(string.Empty, prettyEnumName);
                        EditorGUI.LabelField(btnRect, tooltipContent);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                enumProp.intValue = newSelection;
            }

            EditorGUILayout.Space(1);
        }

        public static void DrawEnumToolbar<TEnum>(
            MaterialEditor materialEditor,
            MaterialProperty property,
            string label,
            string[] options,
            Action<int> onChanged = null
        ) where TEnum : Enum
        {
            if (property == null || options == null || options.Length == 0) return;

            Rect totalRect = EditorGUILayout.GetControlRect(false, 18f);

            float safeRightEdge = EditorGUIUtility.currentViewWidth - 18f;
            float currentRight = Mathf.Min(totalRect.xMax, safeRightEdge);

            float labelWidth = EditorGUIUtility.labelWidth - 4f;
            Rect labelRect = new Rect(totalRect.x, totalRect.y, labelWidth, totalRect.height);

            float controlX = totalRect.x + labelWidth;
            float controlWidth = Mathf.Max(0f, currentRight - controlX);
            Rect controlRect = new Rect(controlX, totalRect.y, controlWidth, totalRect.height);

            EditorGUI.LabelField(labelRect, label);

            float minWidthPerItem = 55f;
            float requiredToolbarWidth = options.Length * minWidthPerItem;

            int currentSelection = Mathf.RoundToInt(property.floatValue);
            int newSelection = currentSelection;

            EditorGUI.BeginChangeCheck();

            if (controlWidth < requiredToolbarWidth)
            {
                GUIContent[] popupOptions = new GUIContent[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    string prettyEnumName = GetPrettyEnumName<TEnum>(i);
                    popupOptions[i] = new GUIContent(options[i], prettyEnumName);
                }

                newSelection = EditorGUI.Popup(controlRect, currentSelection, popupOptions);
            }
            else
            {
                float buttonWidth = controlWidth / options.Length;

                for (int i = 0; i < options.Length; i++)
                {
                    Rect btnRect = new Rect(controlX + (i * buttonWidth), controlRect.y, buttonWidth, controlRect.height);

                    GUIStyle style = (i == 0) ? EditorStyles.miniButtonLeft :
                                     (i == options.Length - 1) ? EditorStyles.miniButtonRight :
                                     EditorStyles.miniButtonMid;

                    bool isActive = (currentSelection == i);
                    string prettyEnumName = GetPrettyEnumName<TEnum>(i);

                    if (GUI.Toggle(btnRect, isActive, options[i], style) && !isActive)
                    {
                        newSelection = i;
                    }

                    GUIContent tooltipContent = new GUIContent(string.Empty, prettyEnumName);
                    EditorGUI.LabelField(btnRect, tooltipContent);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                onChanged?.Invoke(newSelection);
                property.floatValue = newSelection;
                UpdateShaderGraphEnumKeyword(materialEditor, property, newSelection, options);
                materialEditor.PropertiesChanged();
            }

            EditorGUILayout.Space(1);
        }

        private static string GetPrettyEnumName<TEnum>(int index) where TEnum : Enum
        {
            if (Enum.IsDefined(typeof(TEnum), index))
            {
                string rawName = Enum.GetName(typeof(TEnum), index);
                return FormatDisplayName(rawName);
            }

            TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));
            if (index >= 0 && index < values.Length)
            {
                return FormatDisplayName(values[index].ToString());
            }

            return string.Empty;
        }

        private static string FormatDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2");
        }

        public static void DrawToggleButton(SerializedDataParameter param, string label, bool inline = false)
        {
            if (param?.value == null) return;

            SerializedProperty boolProp = param.value;
            EditorGUI.BeginChangeCheck();
            bool newState = DrawToggleUI(label, boolProp.boolValue, inline);

            if (EditorGUI.EndChangeCheck())
            {
                boolProp.boolValue = newState;
            }
        }

        public static void DrawPropertyToggle(MaterialEditor materialEditor, MaterialProperty prop, string label, bool inline = false)
        {
            if (prop == null) return;

            bool currentState = prop.floatValue == 1.0f;
            EditorGUI.BeginChangeCheck();
            bool newState = DrawToggleUI(label, currentState, inline);

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = newState ? 1.0f : 0.0f;
                materialEditor?.PropertiesChanged();
            }
        }

        private static bool DrawToggleUI(string label, bool currentState, bool inline)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = currentState ? FontStyle.Bold : FontStyle.Normal
            };

            if (!inline)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            }

            string buttonText = inline ? $"{label}: {(currentState ? "ON" : "OFF")}" : (currentState ? "ON" : "OFF");
            float height = inline ? 20f : 18f;

            bool newState = GUILayout.Toggle(currentState, buttonText, style, GUILayout.Height(height), GUILayout.ExpandWidth(true));

            if (!inline)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            return newState;
        }

        public static void DrawParamField(SerializedDataParameter param, string labelOverride = null)
        {
            if (param == null) return;

            GUIContent label = new GUIContent(labelOverride ?? param.displayName);
            EditorGUILayout.PropertyField(param.value, label);
        }

        public static void DrawClampedSlider(SerializedDataParameter param, float min, float max, string labelOverride = null)
        {
            if (param?.value == null) return;

            GUIContent label = new GUIContent(labelOverride ?? param.displayName);
            param.value.floatValue = EditorGUILayout.Slider(label, param.value.floatValue, min, max);
        }

        public static void DrawClampedSlider(SerializedDataParameter param, int min, int max, string labelOverride = null)
        {
            if (param?.value == null) return;

            GUIContent label = new GUIContent(labelOverride ?? param.displayName);
            param.value.intValue = EditorGUILayout.IntSlider(label, param.value.intValue, min, max);
        }

        public static void DrawInlineVector2Param(SerializedDataParameter param)
        {
            if (param == null) return;

            SerializedProperty prop = param.value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(param.displayName, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

            EditorGUI.BeginChangeCheck();

            if (prop.propertyType == SerializedPropertyType.Vector2Int)
            {
                Vector2Int vec = EditorGUILayout.Vector2IntField(GUIContent.none, prop.vector2IntValue);
                if (EditorGUI.EndChangeCheck()) prop.vector2IntValue = vec;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 vec = EditorGUILayout.Vector2Field(GUIContent.none, prop.vector2Value);
                if (EditorGUI.EndChangeCheck()) prop.vector2Value = vec;
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void UpdateShaderGraphEnumKeyword(MaterialEditor materialEditor, MaterialProperty property, int activeIndex, string[] options)
        {
            if (property == null || options == null || options.Length == 0) return;

            string propName = property.name;

            foreach (UnityEngine.Object t in materialEditor.targets)
            {
                if (t is Material mat)
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        string keywordToDisable = $"{propName}_{options[i].ToUpper().Replace(" ", "_")}";
                        mat.DisableKeyword(keywordToDisable);
                    }

                    if (activeIndex >= 0 && activeIndex < options.Length)
                    {
                        string keywordToEnable = $"{propName}_{options[activeIndex].ToUpper().Replace(" ", "_")}";
                        mat.EnableKeyword(keywordToEnable);
                    }
                }
            }
        }
    }
}