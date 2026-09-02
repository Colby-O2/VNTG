using System;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIColorSwatch.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIColorSwatch : VisualElement
    {
        public int Index { get; private set; }
        private bool _isSelected;
        private readonly VisualElement _innerColorDisplay;

        public event Action<int> OnSwatchHoverChanged;
        public class UIColorSwatchDragEventArgs : EventArgs { public UIColorSwatch Swatch { get; set; } }
        public event Action<UIColorSwatch> OnSwatchDragStarted;

        private bool _isPrimarySelectedSwatch;

        public UIColorSwatch(Color colorData, int associatedIndex, bool startingSelectionState, StyleSheet styleSheet)
        {
            Index = associatedIndex;
            _isSelected = startingSelectionState;

            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            AddToClassList("color-swatch");

            _innerColorDisplay = new VisualElement
            {
                pickingMode = PickingMode.Position
            };
            _innerColorDisplay.AddToClassList("color-swatch__inner-display");

            _innerColorDisplay.style.backgroundColor = colorData;
            Add(_innerColorDisplay);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerEnterEvent>(e => OnSwatchHoverChanged?.Invoke(Index));
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);

            _innerColorDisplay.generateVisualContent += DrawSelectionFoldTriangle;
        }

        public void UpdateSwatchData(Color targetColor, int newIndex, bool selectionState, bool isPrimarySelectedSwatch)
        {
            Index = newIndex;
            _innerColorDisplay.style.backgroundColor = targetColor;
            _isPrimarySelectedSwatch = isPrimarySelectedSwatch;

            if (_isSelected != selectionState)
            {
                _isSelected = selectionState;
                _innerColorDisplay.MarkDirtyRepaint();
            }
            else if (_isSelected)
            {
                _innerColorDisplay.MarkDirtyRepaint();
            }
        }

        private bool IsInFoldArea(Vector2 localPos)
        {
            float maxCoord = PaletteEditorStyle.FoldSize + PaletteEditorStyle.FoldPadding;
            return _isPrimarySelectedSwatch && localPos.x <= maxCoord && localPos.y <= maxCoord;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_isSelected && IsInFoldArea(evt.localPosition))
            {
                if (!_innerColorDisplay.ClassListContains("swatch-fold-hover"))
                {
                    _innerColorDisplay.AddToClassList("swatch-fold-hover");
                    _innerColorDisplay.MarkDirtyRepaint();
                }
            }
            else
            {
                if (_innerColorDisplay.ClassListContains("swatch-fold-hover"))
                {
                    _innerColorDisplay.RemoveFromClassList("swatch-fold-hover");
                    _innerColorDisplay.MarkDirtyRepaint();
                }
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            if (_isSelected && IsInFoldArea(evt.localPosition))
            {
                OnSwatchDragStarted?.Invoke(this);
                evt.StopPropagation();
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            OnSwatchHoverChanged?.Invoke(-1);
            _innerColorDisplay.RemoveFromClassList("swatch-fold-hover");
            _innerColorDisplay.MarkDirtyRepaint();
        }

        public void UpdateContourBorders(bool showTop, bool showBottom, bool showLeft, bool showRight)
        {
            ToggleClass(this, "swatch--out-top", showTop);
            ToggleClass(this, "swatch--out-bottom", showBottom);
            ToggleClass(this, "swatch--out-left", showLeft);
            ToggleClass(this, "swatch--out-right", showRight);

            ToggleClass(_innerColorDisplay, "swatch--in-top", showTop);
            ToggleClass(_innerColorDisplay, "swatch--in-bottom", showBottom);
            ToggleClass(_innerColorDisplay, "swatch--in-left", showLeft);
            ToggleClass(_innerColorDisplay, "swatch--in-right", showRight);
        }

        private void ToggleClass(VisualElement element, string className, bool enable)
        {
            if (enable) element.AddToClassList(className);
            else element.RemoveFromClassList(className);
        }

        private void DrawSelectionFoldTriangle(MeshGenerationContext mgc)
        {
            if (!_isSelected || !_isPrimarySelectedSwatch) return;

            Rect bounds = _innerColorDisplay.contentRect;
            float size = PaletteEditorStyle.FoldSize;

            var mesh = mgc.Allocate(6, 6);

            Vertex t1_v0 = new Vertex { position = new Vector3(bounds.x, bounds.y, Vertex.nearZ), tint = PaletteEditorStyle.FoldColorUpper };
            Vertex t1_v1 = new Vertex { position = new Vector3(bounds.x + size, bounds.y, Vertex.nearZ), tint = PaletteEditorStyle.FoldColorUpper };
            Vertex t1_v2 = new Vertex { position = new Vector3(bounds.x, bounds.y + size, Vertex.nearZ), tint = PaletteEditorStyle.FoldColorUpper };

            Vertex t2_v0 = new Vertex { position = new Vector3(bounds.x + size, bounds.y, Vertex.nearZ), tint = PaletteEditorStyle.FoldColorLower };
            Vertex t2_v1 = new Vertex { position = new Vector3(bounds.x + size, bounds.y + size, Vertex.nearZ), tint = PaletteEditorStyle.FoldColorLower };
            Vertex t2_v2 = new Vertex { position = new Vector3(bounds.x, bounds.y + size, Vertex.nearZ), tint = PaletteEditorStyle.FoldColorLower };

            mesh.SetAllVertices(new[] { t1_v0, t1_v1, t1_v2, t2_v0, t2_v1, t2_v2 });
            mesh.SetAllIndices(new ushort[] { 0, 1, 2, 3, 4, 5 });
        }
    }
}