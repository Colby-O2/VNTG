using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIBaseColorWheel.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public abstract class UIBaseColorWheel : VisualElement
    {
        private const string COLOR_WHEEL_SHADER_PATH = "Hidden/ColbyO/VNTG/Editor/ColorWheel";

        protected readonly ColorWheelCache Cache;

        private Material _wheelMaterial;
        private RenderTexture _wheelTexture;
        private IList<Color> _currentColors;
        private SelectionState _selectedIndices;
        private int _selectedIndex = -1;

        private int _draggedColorIndex = -1;
        private Vector2 _dragMousePosStart;
        private float _dragStartValue;
        private float _dragStartSaturation;

        public event Action<int, Vector3> OnColorModified;
        public event Action<int> OnSelectionChanged;
        public event Action<int> OnHoverChanged;

        private int _hoveringIndex = -1;
        public int HoveringIndex
        {
            get => _hoveringIndex;
            set
            {
                if (_hoveringIndex != value)
                {
                    _hoveringIndex = value;
                    MarkDirtyRepaint();
                }
            }
        }

        protected abstract float ShaderWheelTypeValue { get; }
        protected abstract Vector2 GetMarkerPosition(int index, Rect wheelRect);
        protected abstract bool TryFindMarker(Vector2 localPos, Rect wheelRect, int selectedIndex, float radius, out int foundIndex);
        protected abstract void ProcessDragInput(Vector2 offset, float magnitude, float hue, ref Vector3 hsv, float startSaturation);
        protected abstract void ProcessShiftDragInput(float delta, ref Vector3 hsv, float startSaturation, float startValue);

        protected UIBaseColorWheel(ColorWheelCache cache)
        {
            Cache = cache;

            AddToClassList("color-picker__wheel");

            InitializeResources();

            generateVisualContent += DrawWheelMarkers;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<GeometryChangedEvent>(OnWheelGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(e => Cleanup());
        }

        private void OnWheelGeometryChanged(GeometryChangedEvent evt)
        {
            if (Mathf.Approximately(evt.newRect.width, evt.oldRect.width) &&
                Mathf.Approximately(evt.newRect.height, evt.oldRect.height)) return;

            RecreateRenderTexture((int)Mathf.Max(1f, evt.newRect.width), (int)Mathf.Max(1f, evt.newRect.height));
            MarkDirtyRepaint();
        }

        private void RecreateRenderTexture(int width, int height)
        {
            if (_wheelTexture != null && (_wheelTexture.width != width || _wheelTexture.height != height))
            {
                _wheelTexture.Release();
                UnityEngine.Object.DestroyImmediate(_wheelTexture);
                _wheelTexture = null;
            }

            if (_wheelTexture == null && width > 0 && height > 0)
            {
                _wheelTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
        }

        private void InitializeResources()
        {
            if (_wheelMaterial == null)
            {
                Shader shader = Shader.Find(COLOR_WHEEL_SHADER_PATH);
                if (shader != null) _wheelMaterial = new Material(shader);
            }

            if (_wheelTexture == null)
            {
                _wheelTexture = new RenderTexture((int)PaletteEditorStyle.WheelSize, (int)PaletteEditorStyle.WheelSize, 0, RenderTextureFormat.ARGB32)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
        }

        public void UpdateVisuals(IList<Color> colors, int selectedIndex, float modifier, SelectionState selectedIndices = null)
        {
            _currentColors = colors;
            _selectedIndex = selectedIndex;
            _selectedIndices = selectedIndices;

            InitializeResources();

            if (_wheelMaterial != null && _wheelTexture != null)
            {
                _wheelMaterial.SetFloat("_WheelType", ShaderWheelTypeValue);
                _wheelMaterial.SetFloat("_Modifier", modifier);

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = _wheelTexture;
                GL.Clear(true, true, Color.clear);
                Graphics.Blit(Texture2D.whiteTexture, _wheelTexture, _wheelMaterial);
                style.backgroundImage = Background.FromRenderTexture(_wheelTexture);
                RenderTexture.active = previousActive;
            }

            MarkDirtyRepaint();
        }

        private void DrawWheelMarkers(MeshGenerationContext mgc)
        {
            if (_currentColors == null || layout.width < 1f) return;

            Rect wheelRect = new Rect(0f, 0f, layout.width, layout.height);
            Painter2D painter = mgc.painter2D;

            for (int i = 0; i < _currentColors.Count; i++)
            {
                if (_selectedIndices != null && !_selectedIndices.Contains(i)) continue;
                if (_selectedIndex == i || HoveringIndex == i) continue;
                DrawVisualMarker(painter, i, wheelRect, Color.white, 1.0f);
            }

            if (_selectedIndex >= 0 && _selectedIndex < _currentColors.Count)
            {
                if (_selectedIndices == null || _selectedIndices.Contains(_selectedIndex))
                {
                    DrawVisualMarker(painter, _selectedIndex, wheelRect, Color.cyan, 1.5f);
                }
            }

            if (HoveringIndex >= 0 && HoveringIndex < _currentColors.Count && HoveringIndex != _selectedIndex)
            {
                if (_selectedIndices == null || _selectedIndices.Contains(HoveringIndex))
                {
                    DrawVisualMarker(painter, HoveringIndex, wheelRect, Color.white, 1.5f);
                }
            }
        }

        private void DrawVisualMarker(Painter2D painter, int index, Rect wheelRect, Color outerRingColor, float scale)
        {
            Vector2 position = GetMarkerPosition(index, wheelRect);
            float radius = PaletteEditorStyle.MarkerSize * scale;
            Color darkOutlineColor = new Color(0.12f, 0.12f, 0.12f, 1f);

            painter.lineWidth = 1.5f;
            painter.strokeColor = outerRingColor;
            painter.fillColor = darkOutlineColor;

            painter.BeginPath();
            painter.Arc(position, radius, 0f, 360f);
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();

            painter.strokeColor = darkOutlineColor;
            painter.fillColor = Cache.GetRGB(index);

            painter.BeginPath();
            painter.Arc(position, Mathf.Max(0.5f, radius - 1.0f), 0f, 360f);
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || _currentColors == null) return;

            Vector2 localPos = evt.localPosition;
            Rect localRect = new Rect(0f, 0f, layout.width, layout.height);

            if (localRect.Contains(localPos))
            {

                this.CapturePointer(evt.pointerId);
                if (TryFindMarker(localPos, localRect, _selectedIndex, PaletteEditorStyle.MarkerSize + 4f, out _draggedColorIndex))
                {
                    if (_selectedIndices != null && !_selectedIndices.Contains(_draggedColorIndex))
                    {
                        _draggedColorIndex = -1;
                        this.ReleasePointer(evt.pointerId);
                        return;
                    }

                    OnSelectionChanged?.Invoke(_draggedColorIndex);

                    _dragMousePosStart = localPos;
                    Vector3 hsv = Cache.GetHSV(_draggedColorIndex);
                    _dragStartValue = hsv.z;
                    _dragStartSaturation = hsv.y;
                }
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId) || _currentColors == null)
            {
                Rect localRect = new Rect(0f, 0f, layout.width, layout.height);
                int targetHoverIndex = -1;

                if (localRect.Contains(evt.localPosition))
                {
                    TryFindMarker(evt.localPosition, localRect, _selectedIndex, PaletteEditorStyle.MarkerSize + 4f, out targetHoverIndex);

                    if (_selectedIndices != null && !_selectedIndices.Contains(targetHoverIndex))
                    {
                        targetHoverIndex = -1;
                    }
                }

                OnHoverChanged?.Invoke(targetHoverIndex);
                return;
            }

            if (_draggedColorIndex >= 0 && _draggedColorIndex < _currentColors.Count)
            {
                Vector3 hsv = Cache.GetHSV(_draggedColorIndex);
                Rect localRect = new Rect(0f, 0f, layout.width, layout.height);

                if (evt.shiftKey)
                {
                    float delta = (evt.localPosition.y - _dragMousePosStart.y) / (0.8f * localRect.height);
                    ProcessShiftDragInput(delta, ref hsv, _dragStartSaturation, _dragStartValue);
                }
                else
                {
                    Vector2 offset = evt.localPosition - (Vector3)localRect.center;
                    float maxRadius = (localRect.width * 0.5f) - 1f;
                    float magnitude = Mathf.Clamp01(offset.magnitude / maxRadius);

                    float angle = Mathf.Atan2(-offset.y, offset.x);
                    float hue = angle / (2.0f * Mathf.PI);
                    if (hue < 0.0f) hue += 1.0f;

                    ProcessDragInput(offset, magnitude, hue, ref hsv, _dragStartSaturation);
                }

                OnColorModified?.Invoke(_draggedColorIndex, hsv);
                evt.StopPropagation();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
            {
                this.ReleasePointer(evt.pointerId);
                _draggedColorIndex = -1;
                evt.StopPropagation();
            }
        }

        public void Cleanup()
        {
            if (_wheelMaterial != null) { UnityEngine.Object.DestroyImmediate(_wheelMaterial); _wheelMaterial = null; }
            if (_wheelTexture != null) { _wheelTexture.Release(); UnityEngine.Object.DestroyImmediate(_wheelTexture); _wheelTexture = null; }
        }
    }
}