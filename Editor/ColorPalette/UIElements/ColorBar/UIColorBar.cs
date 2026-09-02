using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIColorBar.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIColorBar : UIBlitSliderBase
    {
        private bool _isDragging;

        public UIColorBar() : base()
        {
            AddToClassList("color-bar__container");

            style.width = PaletteEditorStyle.BarWidth;
            style.height = Length.Percent(100);
            style.marginLeft = PaletteEditorStyle.Spacing;
            style.flexShrink = 0;

            InitializeTrack(this, PaletteEditorStyle.KnobSize);
            KnobElement.style.left = (PaletteEditorStyle.BarWidth - PaletteEditorStyle.KnobSize) / 2f;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        public void UpdateVisuals(Vector3 hsv, float lightnessValue, bool hasSelection)
        {
            if (!hasSelection)
            {
                KnobElement.style.display = DisplayStyle.None;
                return;
            }

            KnobElement.style.display = DisplayStyle.Flex;

            int currentWidth = (int)Mathf.Max(1f, layout.width > 0 ? layout.width : PaletteEditorStyle.BarWidth);
            int currentHeight = (int)Mathf.Max(1f, layout.height > 0 ? layout.height : PaletteEditorStyle.WheelSize);

            Vector4 inColor = new Vector4(hsv.x, hsv.y, hsv.z, 1.0f);
            BlitTrackTexture(currentWidth, currentHeight, inColor, 7, isHorizontal: false);

            float trackHeight = layout.height > 0 ? layout.height : PaletteEditorStyle.WheelSize;
            float knobTopY = Mathf.Lerp(trackHeight - 12f, 0f, lightnessValue);
            KnobElement.style.top = knobTopY;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            this.CapturePointer(evt.pointerId);
            _isDragging = true;
            ProcessDrag(evt.localPosition);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || !this.HasPointerCapture(evt.pointerId)) return;
            ProcessDrag(evt.localPosition);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
            {
                this.ReleasePointer(evt.pointerId);
                _isDragging = false;
                evt.StopPropagation();
            }
        }

        private void ProcessDrag(Vector2 localPos)
        {
            float value = Mathf.InverseLerp(layout.height, 0f, localPos.y);
            float targetL = Mathf.Clamp(value, 0.0001f, 0.9999f);
            FireValueChanged(targetL);
        }
    }
}