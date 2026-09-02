using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UISelectionBox.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UISelectionBox : VisualElement
    {
        public UISelectionBox()
        {
            style.position = Position.Absolute;
            style.backgroundColor = new Color(0.2f, 0.5f, 1.0f, 0.15f);
            style.borderLeftColor = style.borderRightColor = style.borderTopColor = style.borderBottomColor = new Color(0.2f, 0.6f, 1f, 0.8f);
            style.borderLeftWidth = style.borderRightWidth = style.borderTopWidth = style.borderBottomWidth = 1f;
            pickingMode = PickingMode.Ignore;
        }

        public void UpdatePosition(Vector2 start, Vector2 current)
        {
            float x = Mathf.Min(start.x, current.x);
            float y = Mathf.Min(start.y, current.y);
            float w = Mathf.Abs(start.x - current.x);
            float h = Mathf.Abs(start.y - current.y);

            style.left = x;
            style.top = y;
            style.width = w;
            style.height = h;
        }
    }
}