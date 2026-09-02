using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ColbyO.VNTG.ColorPalette;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PalettePropertyDrawer.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Editor
{
    [CustomPropertyDrawer(typeof(Palette))]
    public class PalettePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            Palette currentPalette = property.objectReferenceValue as Palette;

            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            {
                PopupWindow.Show(position, new PaletteSelectorPopup(property));
                Event.current.Use();
            }

            if (currentPalette != null && currentPalette.colors != null && currentPalette.colors.Count > 0)
            {
                DrawPaletteBar(position, currentPalette.colors);

                GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                EditorGUI.DropShadowLabel(position, currentPalette.name, textStyle);
            }
            else
            {
                GUI.Box(position, currentPalette != null ? currentPalette.name : "None (Click to select Palette)");
            }

            EditorGUI.EndProperty();
        }

        public static void DrawPaletteBar(Rect position, List<Color> colors)
        {
            if (colors == null || colors.Count == 0) return;

            float blockWidth = position.width / colors.Count;
            for (int i = 0; i < colors.Count; i++)
            {
                Rect colorRect = new Rect(position.x + (i * blockWidth), position.y, blockWidth, position.height);
                EditorGUI.DrawRect(colorRect, colors[i]);
            }
        }
    }
}