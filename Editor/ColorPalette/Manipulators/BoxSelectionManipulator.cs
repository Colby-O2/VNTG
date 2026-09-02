using System;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    BoxSelectionManipulator.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class BoxSelectionManipulator : Manipulator
    {
        private readonly Action<Rect, Vector2, SelectionMode, bool, bool> _onSelectionUpdated;
        private UISelectionBox _selectionBox;
        private Vector2 _startPosition;
        private bool _isActive;
        private bool _shouldRenderRegion;

        public enum SelectionMode
        {
            StartedDrag,
            ActiveDrag,
            SelectionConfirmed
        }

        public BoxSelectionManipulator(Action<Rect, Vector2, SelectionMode, bool, bool> onSelectionUpdated, bool shouldRenderRegion = true)
        {
            _onSelectionUpdated = onSelectionUpdated;
            _shouldRenderRegion = shouldRenderRegion;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            _isActive = true;
            _startPosition = evt.localPosition;

            bool isAdditive = evt.ctrlKey || evt.actionKey;
            bool isRangeSelect = evt.shiftKey;

            if (!isRangeSelect && _shouldRenderRegion)
            {
                _selectionBox = new UISelectionBox();
                target.Add(_selectionBox);
                _selectionBox.UpdatePosition(_startPosition, _startPosition);
            }

            Vector2 currentPos = evt.localPosition;
            _onSelectionUpdated?.Invoke(CalculateSelectionRect(_startPosition, currentPos), currentPos, SelectionMode.StartedDrag, isAdditive, isRangeSelect);

            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isActive) return;

            Vector2 currentPos = evt.localPosition;

            if (_selectionBox != null)
            {
                _selectionBox.UpdatePosition(_startPosition, currentPos);
            }

            Rect currentRect = CalculateSelectionRect(_startPosition, currentPos);
            bool isAdditive = evt.ctrlKey || evt.actionKey;
            bool isRangeSelect = evt.shiftKey;

            _onSelectionUpdated?.Invoke(currentRect, currentPos, SelectionMode.ActiveDrag, isAdditive, isRangeSelect);

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isActive) return;

            _isActive = false;

            Vector2 currentPos = evt.localPosition;
            Rect finalRect = CalculateSelectionRect(_startPosition, currentPos);
            bool isAdditive = evt.ctrlKey || evt.actionKey;
            bool isRangeSelect = evt.shiftKey;

            _onSelectionUpdated?.Invoke(finalRect, currentPos, SelectionMode.SelectionConfirmed, isAdditive, isRangeSelect);

            if (_selectionBox != null)
            {
                _selectionBox.RemoveFromHierarchy();
                _selectionBox = null;
            }

            if (target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }

            evt.StopPropagation();
        }

        private Rect CalculateSelectionRect(Vector2 start, Vector2 current)
        {
            float x = Mathf.Min(start.x, current.x);
            float y = Mathf.Min(start.y, current.y);
            float w = Mathf.Abs(start.x - current.x);
            float h = Mathf.Abs(start.y - current.y);
            return new Rect(x, y, w, h);
        }
    }
}