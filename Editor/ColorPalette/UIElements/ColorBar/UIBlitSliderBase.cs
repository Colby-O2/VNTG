using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIBlitSliderBase.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public abstract class UIBlitSliderBase : VisualElement
    {
        private const string COLOR_BAR_SHADER_PATH = "Hidden/ColbyO/VNTG/Editor/ColorBar";
        private static StyleSheet _cachedStyleSheet;

        protected Material SliderMaterial;
        protected RenderTexture TrackTexture;
        protected VisualElement TrackElement;
        protected VisualElement KnobElement;

        public event Action<float> OnValueChanged;

        protected UIBlitSliderBase()
        {
            ApplySharedSliderStyles();
            Shader shader = Shader.Find(COLOR_BAR_SHADER_PATH);
            if (shader != null) SliderMaterial = new Material(shader);
        }

        private void ApplySharedSliderStyles()
        {
            if (_cachedStyleSheet != null)
            {
                styleSheets.Add(_cachedStyleSheet);
                return;
            }

            string[] guids = AssetDatabase.FindAssets("SliderStyles t:StyleSheet");
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

        protected void InitializeTrack(VisualElement track, float knobSize)
        {
            TrackElement = track;
            KnobElement = CreateKnobElement(knobSize);
            TrackElement.Add(KnobElement);
        }

        protected void BlitTrackTexture(int width, int height, Vector4 inColor, int shaderModeId, bool isHorizontal)
        {
            if (SliderMaterial == null || width <= 0 || height <= 0) return;

            if (TrackTexture == null || TrackTexture.width != width || TrackTexture.height != height)
            {
                if (TrackTexture != null) TrackTexture.Release();
                TrackTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                TrackElement.style.backgroundImage = Background.FromRenderTexture(TrackTexture);
            }

            SliderMaterial.SetVector("_InColor", inColor);
            SliderMaterial.SetVector("_Dimensions", new Vector4(width, height, 0, 0));
            SliderMaterial.SetFloat("_Radius", (isHorizontal ? height : width) * 0.5f);
            SliderMaterial.SetInt("_SliderMode", shaderModeId);
            SliderMaterial.SetInt("_IsHorizontal", isHorizontal ? 1 : 0);

            RenderTexture oldActive = RenderTexture.active;
            RenderTexture.active = TrackTexture;
            GL.Clear(true, true, Color.clear);
            Graphics.Blit(Texture2D.whiteTexture, TrackTexture, SliderMaterial);
            RenderTexture.active = oldActive;

            TrackElement.MarkDirtyRepaint();
        }

        protected void FireValueChanged(float rawValue)
        {
            OnValueChanged?.Invoke(rawValue);
        }

        private VisualElement CreateKnobElement(float knobSize)
        {
            var knob = new VisualElement();
            knob.AddToClassList("blit-slider__knob");
            knob.style.width = knobSize;
            knob.style.height = knobSize;

            float outerRadius = knobSize * 0.5f;
            SetBorderRadius(knob, outerRadius);

            VisualElement knobCore = new VisualElement { name = "Knob" };
            knobCore.AddToClassList("blit-slider__knob-core");
            SetBorderRadius(knobCore, outerRadius);

            knob.Add(knobCore);
            return knob;
        }

        private void SetBorderRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius; element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius; element.style.borderBottomRightRadius = radius;
        }

        public virtual void Cleanup()
        {
            if (SliderMaterial != null) { UnityEngine.Object.DestroyImmediate(SliderMaterial); SliderMaterial = null; }
            if (TrackTexture != null) { TrackTexture.Release(); UnityEngine.Object.DestroyImmediate(TrackTexture); TrackTexture = null; }
        }
    }
}