using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIPaletteGridElement.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIPaletteGridElement : VisualElement
    {
        private Palette _palette;
        private StyleSheet _cachedStyleSheet;

        private readonly VisualElement _gridContainer;
        private readonly List<UIColorSwatch> _swatchElements = new List<UIColorSwatch>();
        private VisualElement _resizeHandle;

        private VisualElement _buttonContainer;
        private VisualElement _plusButton;
        private VisualElement _minusButton;

        private bool _isDraggingSizeHandle;
        private int _draggedSwatchIndex = -1;
        private float _dragAccumulatorX;
        private float _dragAccumulatorY;
        private const float ResizeThreshold = 36.0f;

        private int _originalPaletteSizeBeforeDrag = -1;
        private readonly List<Color> _colorHistoryCache = new List<Color>();

        public event Action<int> OnSelectionChanged;
        public event Action<int> OnSwatchHoverChanged;
        public event Action OnPaletteUpdated;

        private HashSet<int> _initialSelectionState = new HashSet<int>();
        private int _lastSelectedIndex;
        private int _shiftDragAnchorIndex = -1;

        public SelectionState Selection { get; set; } = new SelectionState();

        public UIPaletteGridElement()
        {
            style.paddingLeft = style.paddingRight = style.marginTop = style.marginBottom = 4f;

            _buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    width = Length.Percent(100),
                    marginBottom = 4f
                }
            };
            Add(_buttonContainer);

            _gridContainer = new VisualElement();
            _gridContainer.AddToClassList("palette-grid__container");
            Add(_gridContainer);

            focusable = true;

            RegisterCallback<KeyDownEvent>(OnKeyDown);

            this.AddManipulator(new BoxSelectionManipulator(ProcessSelection, shouldRenderRegion: false));
            this.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                if (_palette == null) return;

                bool hasSelection = !Selection.IsEmpty();
                int selectedCount = Selection.Indices.Count;

                if (hasSelection)
                {
                    string deleteLabel = selectedCount > 1 ? $"Delete {selectedCount} Selected Items" : "Delete Selected Item";
                    evt.menu.AppendAction(deleteLabel, (action) => DeleteSelectedSwatches());

                    string duplicateLabel = selectedCount > 1 ? $"Duplicate {selectedCount} Selected Items" : "Duplicate Selected Item";
                    evt.menu.AppendAction(duplicateLabel, (action) => DuplicateSelectedSwatches());

                    evt.menu.AppendSeparator();

                    string copyLabel = selectedCount > 1 ? $"Copy {selectedCount} Colors" : "Copy Color";
                    evt.menu.AppendAction(copyLabel, (action) => CopySelectedToClipboard());

                    string cutLabel = selectedCount > 1 ? $"Cut {selectedCount} Colors" : "Cut Color";
                    evt.menu.AppendAction(cutLabel, (action) =>
                    {
                        CopySelectedToClipboard();
                        DeleteSelectedSwatches();
                    });
                }

                DropdownMenuAction.Status pasteStatus = IsClipboardValidHex()
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled;

                evt.menu.AppendAction("Paste Color(s)", (action) => PasteFromClipboard(), pasteStatus);
            }));

            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete)
            {
                if (_palette != null && !Selection.IsEmpty())
                {
                    DeleteSelectedSwatches();
                    evt.StopPropagation();
                }
                return;
            }

            if (evt.actionKey)
            {
                if (evt.keyCode == KeyCode.C)
                {
                    if (_palette != null && !Selection.IsEmpty())
                    {
                        CopySelectedToClipboard();
                        evt.StopPropagation();
                    }
                }
                else if (evt.keyCode == KeyCode.V)
                {
                    if (_palette != null && IsClipboardValidHex())
                    {
                        PasteFromClipboard();
                        evt.StopPropagation();
                    }
                }
                if (evt.keyCode == KeyCode.X)
                {
                    if (_palette != null && !Selection.IsEmpty())
                    {
                        CopySelectedToClipboard();
                        DeleteSelectedSwatches();
                        evt.StopPropagation();
                    }
                }
                else if (evt.keyCode == KeyCode.A)
                {
                    if (_palette != null && _palette.colors.Count > 0)
                    {
                        SelectAllSwatches();
                        evt.StopPropagation();
                    }
                }
            }
        }

        private void SelectAllSwatches()
        {
            if (_palette == null) return;

            Selection.Clear();

            for (int i = 0; i < _palette.colors.Count; i++)
            {
                Selection.Add(i);
            }

            _lastSelectedIndex = _palette.colors.Count - 1;

            RefreshSwatchVisuals();

            OnSelectionChanged?.Invoke(Selection.GetPrimarySelection());
        }

        private void DuplicateSelectedSwatches()
        {
            if (_palette == null || Selection.IsEmpty()) return;

            Undo.RecordObject(_palette, "Duplicate Colors");

            List<int> sortedSelected = new List<int>(Selection.Indices);
            sortedSelected.Sort();

            List<Color> colorsToDuplicate = new List<Color>();
            foreach (int idx in sortedSelected)
            {
                colorsToDuplicate.Add(_palette.colors[idx]);
            }

            int insertionIndex = sortedSelected[sortedSelected.Count - 1] + 1;
            _palette.colors.InsertRange(insertionIndex, colorsToDuplicate);

            Selection.Clear();
            for (int i = 0; i < colorsToDuplicate.Count; i++)
            {
                Selection.Add(insertionIndex + i);
            }
            _lastSelectedIndex = insertionIndex;

            FinalizePaletteMutation();
        }

        private void CopySelectedToClipboard()
        {
            if (_palette == null || Selection.IsEmpty()) return;

            List<int> sortedSelected = new List<int>(Selection.Indices);
            sortedSelected.Sort();

            List<string> hexStrings = new List<string>();
            foreach (int idx in sortedSelected)
            {
                hexStrings.Add("#" + ColorUtility.ToHtmlStringRGBA(_palette.colors[idx]));
            }

            EditorGUIUtility.systemCopyBuffer = string.Join(", ", hexStrings);
        }

        private void PasteFromClipboard()
        {
            if (_palette == null) return;

            string clipboard = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboard)) return;

            string[] tokens = clipboard.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<Color> parsedColors = new List<Color>();

            foreach (string token in tokens)
            {
                string cleanToken = token.Trim();
                if (ColorUtility.TryParseHtmlString(cleanToken, out Color parsedColor))
                {
                    parsedColors.Add(parsedColor);
                }
            }

            if (parsedColors.Count == 0) return;

            Undo.RecordObject(_palette, "Paste Colors from Clipboard");

            int insertionIndex;
            if (!Selection.IsEmpty())
            {
                List<int> sortedSelected = new List<int>(Selection.Indices);
                sortedSelected.Sort();
                insertionIndex = sortedSelected[sortedSelected.Count - 1] + 1;
                _palette.colors.InsertRange(insertionIndex, parsedColors);
            }
            else
            {
                insertionIndex = _palette.colors.Count;
                _palette.colors.AddRange(parsedColors);
            }

            Selection.Clear();
            for (int i = 0; i < parsedColors.Count; i++)
            {
                Selection.Add(insertionIndex + i);
            }
            _lastSelectedIndex = insertionIndex;

            FinalizePaletteMutation();
        }

        private bool IsClipboardValidHex()
        {
            string clipboard = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboard)) return false;

            string firstToken = clipboard.Split(',')[0].Trim();
            return ColorUtility.TryParseHtmlString(firstToken, out _);
        }

        private void DeleteSelectedSwatches()
        {
            if (_palette == null || Selection.IsEmpty()) return;

            Undo.RecordObject(_palette, "Delete Selected Swatches");

            List<int> indicesToDelete = new List<int>(Selection.Indices);
            indicesToDelete.Sort((a, b) => b.CompareTo(a));

            foreach (int index in indicesToDelete)
            {
                if (index >= 0 && index < _palette.colors.Count)
                {
                    _palette.colors.RemoveAt(index);
                }
            }

            Selection.Clear();
            if (_palette.colors.Count > 0)
            {
                Selection.Add(0);
                _lastSelectedIndex = 0;
            }
            else
            {
                _lastSelectedIndex = -1;
            }

            FinalizePaletteMutation();
        }

        private void ProcessSelection(Rect selectionRect, Vector2 pointerPos, BoxSelectionManipulator.SelectionMode mode, bool isAdditive, bool isRangeSelect)
        {
            if (_isDraggingSizeHandle || _palette == null) return;

            if (mode == BoxSelectionManipulator.SelectionMode.StartedDrag)
            {
                _initialSelectionState = new HashSet<int>(Selection.Indices);
                _shiftDragAnchorIndex = isRangeSelect && _shiftDragAnchorIndex == -1 ? _lastSelectedIndex : -1;

                if (!isRangeSelect && !isAdditive)
                {
                    Selection.Clear();
                    _initialSelectionState.Clear();
                }
                return;
            }

            if (isRangeSelect) HandleRangeSelection(pointerPos, mode, isAdditive);
            else HandleBoxOrClickSelection(selectionRect, isAdditive);

            if (mode == BoxSelectionManipulator.SelectionMode.SelectionConfirmed)
            {
                if (Selection.IsEmpty())
                {
                    //Selection.Add(0);
                    SelectAllSwatches();
                    _lastSelectedIndex = 0;
                }
                _shiftDragAnchorIndex = -1;
                _initialSelectionState?.Clear();
            }

            RefreshSwatchVisuals();
            OnSelectionChanged?.Invoke(Selection.GetPrimarySelection());
        }

        private void HandleRangeSelection(Vector2 pointerPos, BoxSelectionManipulator.SelectionMode mode, bool isAdditive)
        {
            Vector2 currentGridPos = this.ChangeCoordinatesTo(_gridContainer, pointerPos);
            int currentHoveredIndex = _swatchElements.FindIndex(swatch => swatch.layout.Contains(currentGridPos));

            if (currentHoveredIndex == -1 || _shiftDragAnchorIndex == -1) return;

            int targetIndex = Selection.IsEmpty() ? _shiftDragAnchorIndex : Selection.GetPrimarySelection();
            Selection.Clear();
            int start = Mathf.Min(targetIndex, currentHoveredIndex);
            int end = Mathf.Max(targetIndex, currentHoveredIndex);

            for (int i = start; i <= end; i++) Selection.Add(i);

            if (isAdditive) Selection.UnionWith(_initialSelectionState);
            if (mode == BoxSelectionManipulator.SelectionMode.SelectionConfirmed) _lastSelectedIndex = currentHoveredIndex;
        }

        private void HandleBoxOrClickSelection(Rect selectionRect, bool isAdditive)
        {
            Vector2 gridSpaceMin = this.ChangeCoordinatesTo(_gridContainer, selectionRect.min);
            Vector2 gridSpaceMax = this.ChangeCoordinatesTo(_gridContainer, selectionRect.max);
            Rect gridSpaceRect = Rect.MinMaxRect(gridSpaceMin.x, gridSpaceMin.y, gridSpaceMax.x, gridSpaceMax.y);

            bool isSingleClick = gridSpaceRect.size.sqrMagnitude == 0f;
            int primaryHitIndex = -1;

            for (int i = 0; i < _swatchElements.Count; i++)
            {
                bool isHit = isSingleClick
                    ? _swatchElements[i].layout.Contains(gridSpaceRect.min)
                    : _swatchElements[i].layout.Overlaps(gridSpaceRect);

                if (isHit)
                {
                    if (isSingleClick) primaryHitIndex = i;

                    if (isAdditive && _initialSelectionState.Contains(i))
                        Selection.Remove(i);
                    else
                        Selection.Add(i);
                }
                else
                {
                    if (isAdditive && _initialSelectionState.Contains(i))
                        Selection.Add(i);
                    else
                        Selection.Remove(i);
                }
            }

            if (isSingleClick && primaryHitIndex != -1) _lastSelectedIndex = primaryHitIndex;
        }

        public void BindPalette(Palette palette, SelectionState selection)
        {
            if (selection != null) Selection = selection;

            if (_palette == palette && _palette != null && _swatchElements.Count == _palette.colors.Count)
            {
                UpdateHistoryCache();
                RefreshSwatchVisuals();
                return;
            }

            _palette = palette;
            UpdateHistoryCache();
            RebuildGridStructure();
            RefreshSwatchVisuals();
        }

        private void UpdateHistoryCache()
        {
            if (_palette?.colors == null) return;

            if (_colorHistoryCache.Count != _palette.colors.Count)
            {
                _colorHistoryCache.Clear();
                _colorHistoryCache.AddRange(_palette.colors);
                return;
            }

            for (int i = 0; i < _palette.colors.Count; i++)
            {
                _colorHistoryCache[i] = _palette.colors[i];
            }
        }

        private void RefreshSwatchVisuals()
        {
            if (_palette == null) return;

            //bool isActivelyInteracting = _isDraggingSizeHandle || _draggedSwatchIndex >= 0;
            //DisplayStyle buttonVisibility = isActivelyInteracting ? DisplayStyle.None : DisplayStyle.Flex;

            //if (_plusButton != null) _plusButton.style.display = buttonVisibility;
            //if (_minusButton != null) _minusButton.style.display = buttonVisibility;

            float gridWidth = _gridContainer.resolvedStyle.width > 0 ? _gridContainer.resolvedStyle.width : 300f;
            int columns = Mathf.Max(1, Mathf.FloorToInt(gridWidth / ResizeThreshold));

            for (int i = 0; i < _swatchElements.Count; i++)
            {
                bool isSelected = Selection.Contains(i);
                _swatchElements[i].UpdateSwatchData(_palette.colors[i], i, isSelected, i == Selection.GetPrimarySelection());

                bool topSelected = i >= columns && Selection.Contains(i - columns);
                bool bottomSelected = i + columns < _swatchElements.Count && Selection.Contains(i + columns);
                bool leftSelected = i % columns != 0 && Selection.Contains(i - 1);
                bool rightSelected = (i + 1) % columns != 0 && Selection.Contains(i + 1);

                _swatchElements[i].UpdateContourBorders(
                    showTop: isSelected && !topSelected,
                    showBottom: isSelected && !bottomSelected,
                    showLeft: isSelected && !leftSelected,
                    showRight: isSelected && !rightSelected
                );
            }
        }

        private StyleSheet GetOrLoadStyleSheet()
        {
            if (_cachedStyleSheet != null) return _cachedStyleSheet;

            string[] guids = AssetDatabase.FindAssets("SwatchStyles t:StyleSheet");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (_cachedStyleSheet != null) styleSheets.Add(_cachedStyleSheet);
            }
            return _cachedStyleSheet;
        }

        private void RebuildGridStructure()
        {
            _gridContainer.Clear();
            _swatchElements.Clear();
            if (_palette == null) return;

            StyleSheet standardStyleSheet = GetOrLoadStyleSheet();
            for (int i = 0; i < _palette.colors.Count; i++)
            {
                var swatch = CreateSwatchElement(_palette.colors[i], i, standardStyleSheet);
                _swatchElements.Add(swatch);
                _gridContainer.Add(swatch);
            }

            InitializeAddButton();
            InitializeRemoveButton();

            InitializeResizeHandle();
            _gridContainer.Add(_resizeHandle);

            _gridContainer.RegisterCallback<GeometryChangedEvent>(OnGridGeometryChanged);
        }

        private void OnGridGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.newRect.width <= 0 || evt.oldRect.width == evt.newRect.width)
                return;

            RefreshSwatchVisuals();
        }

        private UIColorSwatch CreateSwatchElement(Color color, int index, StyleSheet sheet)
        {
            var swatch = new UIColorSwatch(color, index, Selection.Contains(index), sheet);
            swatch.OnSwatchDragStarted += StartSwatchDrag;
            swatch.OnSwatchHoverChanged += (hoveredIndex) => OnSwatchHoverChanged?.Invoke(hoveredIndex);
            return swatch;
        }

        private void InitializeAddButton()
        {
            if (_plusButton != null) return;

            _plusButton = new Button(() => AlterPaletteSizeByAmount(1))
            {
                text = "+",
                style =
                {
                    width = PaletteEditorStyle.RectSize,
                    height = PaletteEditorStyle.RectSize,
                    fontSize = 14f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 4f
                }
            };
            _plusButton.AddToClassList("palette-modifier-btn");
            _buttonContainer.Add(_plusButton);
        }

        private void InitializeRemoveButton()
        {
            if (_minusButton != null) return;

            _minusButton = new Button(() => AlterPaletteSizeByAmount(-1))
            {
                text = "-",
                style =
                {
                    width = PaletteEditorStyle.RectSize,
                    height = PaletteEditorStyle.RectSize,
                    fontSize = 14f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 2f
                }
            };
            _minusButton.AddToClassList("palette-modifier-btn");
            _buttonContainer.Add(_minusButton);
        }

        private void InitializeResizeHandle()
        {
            if (_resizeHandle != null) return;

            StyleSheet standardStyleSheet = GetOrLoadStyleSheet();

            _resizeHandle = new VisualElement
            {
                pickingMode = PickingMode.Position,
                style =
        {
            width = PaletteEditorStyle.RectSize,
            height = PaletteEditorStyle.RectSize,
            backgroundColor = PaletteEditorStyle.HandleRectColor,
            justifyContent = Justify.Center,
            alignItems = Align.Center
        }
            };
            _resizeHandle.AddToClassList("resize-handle");
            _resizeHandle.AddToClassList("swatch-grid-resizing");

            Label handleLabel = new Label("↔")
            {
                pickingMode = PickingMode.Ignore,
                style = { color = PaletteEditorStyle.HandleIconColor }
            };
            _resizeHandle.Add(handleLabel);

            _resizeHandle.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button != 0) return;
                _isDraggingSizeHandle = true;
                _dragAccumulatorX = _dragAccumulatorY = 0f;

                AddToClassList("swatch-grid-resizing");
                RefreshSwatchVisuals();
                this.CapturePointer(e.pointerId);
                e.StopPropagation();
            });
        }

        private void AlterPaletteSizeByAmount(int amount)
        {
            if (_palette == null) return;

            int newTotalCount = Mathf.Max(1, _palette.colors.Count + amount);
            if (newTotalCount == _palette.colors.Count) return;

            Undo.RecordObject(_palette, amount > 0 ? "Added Color" : "Removed Color");

            while (_palette.colors.Count < newTotalCount)
            {
                _palette.colors.Add(GetFillerColor(_palette.colors.Count, false));
            }

            while (_palette.colors.Count > newTotalCount)
            {
                int targetIdx = _palette.colors.Count - 1;
                _palette.colors.RemoveAt(targetIdx);

                if (Selection.Contains(targetIdx))
                {
                    Selection.Remove(targetIdx);
                    if (targetIdx > 0) Selection.Add(targetIdx - 1);
                }
            }

            FinalizePaletteMutation();
        }

        private void StartSwatchDrag(UIColorSwatch swatch)
        {
            _draggedSwatchIndex = swatch.Index;
            if (_palette != null) _originalPaletteSizeBeforeDrag = _palette.colors.Count;
            RefreshSwatchVisuals();
            this.CapturePointer(PointerId.mousePointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_isDraggingSizeHandle)
            {
                ProcessResizeDelta(evt.deltaPosition);
                evt.StopPropagation();
                return;
            }

            if (_draggedSwatchIndex >= 0)
            {
                ProcessReorderingTarget(evt.localPosition);
                evt.StopPropagation();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_isDraggingSizeHandle)
            {
                _isDraggingSizeHandle = false;
                RemoveFromClassList("swatch-grid-resizing");
                RefreshSwatchVisuals();
                if (this.HasPointerCapture(evt.pointerId)) this.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }

            if (_draggedSwatchIndex >= 0)
            {
                bool structureChanged = false;
                while (_palette.colors.Count > _originalPaletteSizeBeforeDrag && _swatchElements.Count > 1)
                {
                    int lastIndex = _palette.colors.Count - 1;

                    if (Selection.Contains(lastIndex)) break;

                    Undo.RecordObject(_palette, "Resized Palette");
                    _palette.colors.RemoveAt(lastIndex);

                    var visualToRemove = _swatchElements[lastIndex];
                    if (_gridContainer.Contains(visualToRemove)) _gridContainer.Remove(visualToRemove);
                    _swatchElements.RemoveAt(lastIndex);
                    structureChanged = true;
                }

                if (structureChanged)
                {
                    FinalizePaletteMutation();
                }

                _draggedSwatchIndex = -1;
                _originalPaletteSizeBeforeDrag = -1;
                RefreshSwatchVisuals();
                if (this.HasPointerCapture(evt.pointerId)) this.ReleasePointer(evt.pointerId);

                UpdateHistoryCache();
                evt.StopPropagation();
            }
        }

        private Color GetFillerColor(int index, bool ignoreCache = false)
        {
            return (!ignoreCache && index < _colorHistoryCache.Count) ? _colorHistoryCache[index] : Color.black;
        }

        private void FinalizePaletteMutation()
        {
            EditorUtility.SetDirty(_palette);
            OnPaletteUpdated?.Invoke();
            RebuildGridStructure();
            RefreshSwatchVisuals();
        }

        private void ProcessReorderingTarget(Vector2 localizedMousePos)
        {
            Vector2 gridSpaceMousePos = this.ChangeCoordinatesTo(_gridContainer, localizedMousePos);
            float gridWidth = _gridContainer.resolvedStyle.width > 0 ? _gridContainer.resolvedStyle.width : 300f;
            int columns = Mathf.Max(1, Mathf.FloorToInt(gridWidth / ResizeThreshold));

            bool isMultiDrag = Selection.Indices.Count > 1 && Selection.Contains(_draggedSwatchIndex);
            List<int> sortedIndices = isMultiDrag ? new List<int>(Selection.Indices) : new List<int> { _draggedSwatchIndex };
            sortedIndices.Sort();

            int handleOffset = 0;

            if (_swatchElements.Count > 0)
            {
                var lastSwatch = _swatchElements[_swatchElements.Count - 1];

                bool isBelowGrid = gridSpaceMousePos.y > lastSwatch.layout.yMax;
                bool isPastLastElementSameRow = gridSpaceMousePos.y > lastSwatch.layout.yMin && gridSpaceMousePos.x > lastSwatch.layout.xMax;

                if (isBelowGrid || isPastLastElementSameRow)
                {
                    int targetColumn = Mathf.FloorToInt(gridSpaceMousePos.x / ResizeThreshold);
                    targetColumn = Mathf.Clamp(targetColumn, 0, columns - 1);

                    float swatchHeight = lastSwatch.layout.height > 0 ? lastSwatch.layout.height : ResizeThreshold;
                    int targetRow = Mathf.FloorToInt(gridSpaceMousePos.y / swatchHeight);

                    int desiredFlatIndex = (targetRow * columns) + targetColumn;

                    int gridCountWithoutDragged = _palette.colors.Count - sortedIndices.Count;
                    int targetInsertIndex = desiredFlatIndex - handleOffset;

                    if (targetInsertIndex > gridCountWithoutDragged)
                    {
                        int slotsToCreate = targetInsertIndex - gridCountWithoutDragged;

                        int currentCount = _palette.colors.Count;
                        List<Color> newFillerColors = new List<Color>();
                        StyleSheet activeSheet = GetOrLoadStyleSheet();

                        for (int s = 0; s < slotsToCreate; s++)
                            newFillerColors.Add(GetFillerColor(currentCount + s, true));

                        int insertIndex = gridCountWithoutDragged + slotsToCreate;

                        MoveColorsAndVisuals(sortedIndices, insertIndex, handleOffset, newFillerColors, activeSheet);
                        return;
                    }
                }
            }

            for (int i = 0; i < _swatchElements.Count; i++)
            {
                if (i == _draggedSwatchIndex) continue;

                if (_swatchElements[i].layout.Contains(gridSpaceMousePos))
                {
                    int insertIndex = Mathf.Clamp(i - handleOffset, 0, _palette.colors.Count - sortedIndices.Count);

                    if (insertIndex == sortedIndices[0]) return;

                    MoveColorsAndVisuals(sortedIndices, insertIndex, handleOffset, null, null);
                    break;
                }
            }
        }

        private void MoveColorsAndVisuals(List<int> sortedIndices, int insertIndex, int handleOffset, List<Color> fillerColors, StyleSheet activeSheet)
        {
            Undo.RecordObject(_palette, fillerColors != null ? "Expanded Palette" : "Reordered Color");

            List<Color> targetColors = new List<Color>();
            List<UIColorSwatch> visualsToMove = new List<UIColorSwatch>();

            foreach (int idx in sortedIndices)
            {
                targetColors.Add(_palette.colors[idx]);
                visualsToMove.Add(_swatchElements[idx]);
            }

            for (int k = sortedIndices.Count - 1; k >= 0; k--)
            {
                _palette.colors.RemoveAt(sortedIndices[k]);
                _swatchElements.RemoveAt(sortedIndices[k]);
            }

            foreach (var visual in visualsToMove)
            {
                if (_gridContainer.Contains(visual)) _gridContainer.Remove(visual);
            }

            if (fillerColors != null)
            {
                _palette.colors.AddRange(fillerColors);
                _palette.colors.InsertRange(insertIndex, targetColors);

                for (int s = 0; s < fillerColors.Count; s++)
                {
                    var newSwatch = CreateSwatchElement(fillerColors[s], _palette.colors.Count - fillerColors.Count + s, activeSheet);
                    _swatchElements.Add(newSwatch);
                    _gridContainer.Insert(_swatchElements.Count - 1, newSwatch);
                }
            }
            else
            {
                _palette.colors.InsertRange(insertIndex, targetColors);
            }

            for (int m = 0; m < visualsToMove.Count; m++)
            {
                _swatchElements.Insert(insertIndex + m, visualsToMove[m]);
                _gridContainer.Insert(insertIndex + m, visualsToMove[m]);
            }

            Selection.Clear();
            for (int m = 0; m < targetColors.Count; m++) Selection.Add(insertIndex + m);

            _draggedSwatchIndex = insertIndex + handleOffset;

            _resizeHandle.BringToFront();

            EditorUtility.SetDirty(_palette);
            OnSelectionChanged?.Invoke(_draggedSwatchIndex);
            OnPaletteUpdated?.Invoke();
            RefreshSwatchVisuals();
        }

        private void ProcessResizeDelta(Vector3 delta)
        {
            _dragAccumulatorX += delta.x;
            _dragAccumulatorY += delta.y;

            int itemMutationAmount = 0;

            if (Mathf.Abs(_dragAccumulatorX) >= ResizeThreshold)
            {
                itemMutationAmount += Mathf.RoundToInt(_dragAccumulatorX / ResizeThreshold);
                _dragAccumulatorX %= ResizeThreshold;
            }
            if (Mathf.Abs(_dragAccumulatorY) >= ResizeThreshold)
            {
                float computedWidth = resolvedStyle.width > 0 ? resolvedStyle.width : 300f;
                int elementsPerRow = Mathf.Max(1, Mathf.FloorToInt(computedWidth / PaletteEditorStyle.RectSize));
                itemMutationAmount += Mathf.RoundToInt(_dragAccumulatorY / ResizeThreshold) * elementsPerRow;
                _dragAccumulatorY %= ResizeThreshold;
            }

            if (itemMutationAmount != 0)
            {
                int newTotalCount = Mathf.Max(1, _palette.colors.Count + itemMutationAmount);
                if (newTotalCount != _palette.colors.Count)
                {
                    Undo.RecordObject(_palette, "Resized Palette");

                    while (_palette.colors.Count < newTotalCount)
                    {
                        _palette.colors.Add(GetFillerColor(_palette.colors.Count, false));
                    }

                    while (_palette.colors.Count > newTotalCount)
                    {
                        int targetIdx = _palette.colors.Count - 1;
                        _palette.colors.RemoveAt(targetIdx);

                        if (Selection.Contains(targetIdx))
                        {
                            Selection.Remove(targetIdx);
                            if (targetIdx > 0) Selection.Add(targetIdx - 1);
                        }
                    }

                    EditorUtility.SetDirty(_palette);
                    FinalizePaletteMutation();
                }
            }
        }
    }
}