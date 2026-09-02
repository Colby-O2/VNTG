using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ColbyO.VNTG.ColorPalette;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteSelectorPopup.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Editor
{
    public class PaletteSelectorPopup : PopupWindowContent
    {
        private readonly SerializedProperty property;
        private readonly Action<Palette> onPaletteSelected;

        private List<Palette> cachedPalettes;
        private Vector2 scrollPos;
        private string searchFilter = "";
        private bool isFocusSet = false;

        public PaletteSelectorPopup(SerializedProperty property)
        {
            this.property = property;
            CachePalettes();
        }

        public PaletteSelectorPopup(Action<Palette> onPaletteSelected)
        {
            this.onPaletteSelected = onPaletteSelected;
            CachePalettes();
        }

        private void CachePalettes()
        {
            string[] guids = AssetDatabase.FindAssets("t:Palette");
            cachedPalettes = new List<Palette>(guids.Length);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Palette p = AssetDatabase.LoadAssetAtPath<Palette>(path);
                if (p != null) cachedPalettes.Add(p);
            }
        }

        public override Vector2 GetWindowSize()
        {
            int itemCount = cachedPalettes != null ? cachedPalettes.Count : 0;
            return new Vector2(300, Mathf.Min(itemCount * 26 + 65, 300));
        }

        public override void OnGUI(Rect rect)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseMove)
            {
                editorWindow.Repaint();
            }

            GUILayout.Space(2);
            GUI.SetNextControlName("PaletteSearchField");
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);

            if (!isFocusSet)
            {
                EditorGUI.FocusTextInControl("PaletteSearchField");
                isFocusSet = true;
            }

            GUILayout.Space(2);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            if (string.IsNullOrEmpty(searchFilter))
            {
                Rect noneRect = GUILayoutUtility.GetRect(new GUIContent("None"), EditorStyles.miniButton, GUILayout.Height(20));
                if (noneRect.Contains(e.mousePosition))
                {
                    EditorGUI.DrawRect(noneRect, new Color(0.3f, 0.5f, 0.8f, 0.35f));
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        SelectPalette(null);
                        e.Use();
                    }
                }
                GUI.Label(noneRect, "None", EditorStyles.centeredGreyMiniLabel);
            }

            for (int i = 0; i < cachedPalettes.Count; i++)
            {
                Palette palette = cachedPalettes[i];
                if (palette == null) continue;

                if (!string.IsNullOrEmpty(searchFilter) &&
                    !palette.name.ToLower().Contains(searchFilter.ToLower()))
                {
                    continue;
                }

                Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(24));

                bool isHovered = rowRect.Contains(e.mousePosition);

                if (isHovered)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0.25f, 0.55f, 0.95f, 0.35f));

                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        SelectPalette(palette);
                        e.Use();
                    }
                }

                Rect labelRect = new Rect(rowRect.x + 5, rowRect.y, 110, rowRect.height);
                GUI.Label(labelRect, palette.name, EditorStyles.miniBoldLabel);

                Rect paletteBarRect = new Rect(rowRect.x + 120, rowRect.y + 3, rowRect.width - 125, rowRect.height - 6);
                PalettePropertyDrawer.DrawPaletteBar(paletteBarRect, palette.colors);
            }

            GUILayout.EndScrollView();
        }

        private void SelectPalette(Palette palette)
        {
            if (property != null)
            {
                property.objectReferenceValue = palette;
                property.serializedObject.ApplyModifiedProperties();
            }

            onPaletteSelected?.Invoke(palette);

            editorWindow.Close();
        }
    }
}