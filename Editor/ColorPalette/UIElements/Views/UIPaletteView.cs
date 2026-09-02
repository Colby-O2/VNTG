using ColbyO.VNTG.CRT;
using ColbyO.VNTG.PSX;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIPaletteView.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIPaletteView : VisualElement
    {
        private readonly DropdownField _cameraDropdown;

        private readonly VisualElement _viewportContainer;
        private readonly VisualElement _baseImage;
        private readonly VisualElement _filteredMask;
        private readonly VisualElement _filteredImage;
        private readonly VisualElement _sliderHandle;

        private Camera _previewCamera;
        private Volume _localVolume;
        private VolumeProfile _internalRuntimeCloneProfile;

        private RenderTexture _renderTextureBase;
        private RenderTexture _renderTextureFiltered;

        private readonly List<Camera> _foundCameras = new List<Camera>();
        private Camera _explicitSelectedCamera;
        private bool _useSceneViewFallback = true;
        private Palette _currentPaletteAsset;

        private bool _isDraggingSlider;
        private float _splitPercentage = 0.5f;

        private Rect _imageBounds;

        public UIPaletteView()
        {
            style.flexGrow = 1;
            style.flexShrink = 1;
            style.flexBasis = new StyleLength(StyleKeyword.Auto);

            style.minHeight = 400;
            style.marginTop = 10;
            style.marginBottom = 10;

            VisualElement headerControls = new VisualElement();
            headerControls.style.flexDirection = FlexDirection.Row;
            headerControls.style.alignItems = Align.Center;
            headerControls.style.justifyContent = Justify.SpaceBetween;
            headerControls.style.marginBottom = 4;

            Label titleLabel = new Label("Palette Preview");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 14;

            headerControls.Add(titleLabel);

            _cameraDropdown = new DropdownField("")
            {
                style = { width = 250 }
            };

            _cameraDropdown.RegisterCallback<PointerDownEvent>(evt => RefreshCameraDropdownOptions());
            _cameraDropdown.RegisterValueChangedCallback(OnDropdownValueChanged);

            headerControls.Add(_cameraDropdown);
            Add(headerControls);

            _viewportContainer = new VisualElement();
            _viewportContainer.style.flexGrow = 1;
            _viewportContainer.style.position = Position.Relative;
            _viewportContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            _viewportContainer.style.overflow = Overflow.Hidden;
            _viewportContainer.RegisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
            Add(_viewportContainer);

            _baseImage = new VisualElement();
            _baseImage.style.position = Position.Absolute;
            _baseImage.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _viewportContainer.Add(_baseImage);

            _filteredMask = new VisualElement();
            _filteredMask.style.position = Position.Absolute;
            _filteredMask.style.overflow = Overflow.Hidden;
            _viewportContainer.Add(_filteredMask);

            _filteredImage = new VisualElement();
            _filteredImage.style.position = Position.Absolute;
            _filteredImage.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _filteredMask.Add(_filteredImage);

            _sliderHandle = new VisualElement();
            _sliderHandle.style.position = Position.Absolute;
            _sliderHandle.style.top = 0; _sliderHandle.style.bottom = 0;
            _sliderHandle.style.width = 4;
            _sliderHandle.style.marginLeft = -2;
            _sliderHandle.style.backgroundColor = Color.white;
            _sliderHandle.style.borderLeftColor = Color.black;
            _sliderHandle.style.borderRightColor = Color.black;
            _sliderHandle.style.borderLeftWidth = 1;
            _sliderHandle.style.borderRightWidth = 1;

            _sliderHandle.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _sliderHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _sliderHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _sliderHandle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            _viewportContainer.Add(_sliderHandle);

            _viewportContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _viewportContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _viewportContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _viewportContainer.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);

            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);

            InitializeRendering();
        }

        private void OnViewportGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateRenderResolution();
            UpdateSplitterPositions();
        }

        private void UpdateSplitterPositions()
        {
            if (_imageBounds.width <= 0) return;

            float splitXWithinImage = _imageBounds.width * _splitPercentage;

            _filteredMask.style.left = _imageBounds.x;
            _filteredMask.style.top = _imageBounds.y;
            _filteredMask.style.width = splitXWithinImage;
            _filteredMask.style.height = _imageBounds.height;

            _filteredImage.style.width = _imageBounds.width;
            _filteredImage.style.height = _imageBounds.height;
            _filteredImage.style.left = 0;

            _sliderHandle.style.left = _imageBounds.x + splitXWithinImage;
            _sliderHandle.style.top = _imageBounds.y;
            _sliderHandle.style.height = _imageBounds.height;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDraggingSlider = true;
            _viewportContainer.CapturePointer(evt.pointerId);

            UpdateSplitFromPointer(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDraggingSlider) return;

            UpdateSplitFromPointer(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_isDraggingSlider)
            {
                _isDraggingSlider = false;
                _viewportContainer.ReleasePointer(evt.pointerId);
            }
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _isDraggingSlider = false;
        }

        private void UpdateSplitFromPointer(Vector2 pointerPosition)
        {
            Vector2 localMousePos = _viewportContainer.WorldToLocal(pointerPosition);

            if (_imageBounds.width > 0)
            {
                float relativeX = localMousePos.x - _imageBounds.x;
                _splitPercentage = Mathf.Clamp01(relativeX / _imageBounds.width);

                UpdateSplitterPositions();
            }
        }

        public void BindPaletteContext(Palette palette)
        {
            _currentPaletteAsset = palette;
            UpdateVolumeWithPalette(null);
        }

        private void UpdateVolumeWithPalette(Camera camera)
        {
            if (_currentPaletteAsset == null || _localVolume == null) return;

            if (_internalRuntimeCloneProfile == null)
            {
                _internalRuntimeCloneProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                _internalRuntimeCloneProfile.name = "VNTG_Override_Profile";
                _localVolume.profile = _internalRuntimeCloneProfile;
            }

            if (!_internalRuntimeCloneProfile.TryGet(out PSXEffectSettings psxSettings))
            {
                psxSettings = _internalRuntimeCloneProfile.Add<PSXEffectSettings>(true);
            }

            if (camera != null)
            {
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();

                LayerMask layerMask = cameraData != null ? cameraData.volumeLayerMask : (LayerMask)(-1);
                Transform triggerTransform = (cameraData != null && cameraData.volumeTrigger != null)
                    ? cameraData.volumeTrigger
                    : camera.transform;

                VolumeManager.instance.Update(triggerTransform, layerMask);

                VolumeStack stack = VolumeManager.instance.stack;
                if (stack != null)
                {
                    PSXEffectSettings activePSXSettings = stack.GetComponent<PSXEffectSettings>();
                    if (activePSXSettings != null && psxSettings != null)
                    {
                        UnityEditor.EditorUtility.CopySerialized(activePSXSettings, psxSettings);
                    }
                }
            }

            psxSettings.EnableColorPalette.overrideState = true;
            psxSettings.EnableColorPalette.value = true;

            psxSettings.PaletteAsset.overrideState = true;
            psxSettings.PaletteAsset.value = _currentPaletteAsset;
        }

        private void OnAttached(AttachToPanelEvent evt)
        {
            RefreshCameraDropdownOptions();
            TogglePreview(true);
        }

        private void OnDetached(DetachFromPanelEvent evt)
        {
            TogglePreview(false);
            Cleanup();
        }

        private void InitializeRendering()
        {
            GameObject camGO = new GameObject("VNTG_Preview_Camera") { hideFlags = HideFlags.HideAndDontSave };

            _previewCamera = camGO.AddComponent<Camera>();
            _previewCamera.clearFlags = CameraClearFlags.Skybox;
            _previewCamera.forceIntoRenderTexture = true;
            _previewCamera.enabled = false;

            var urpCamData = _previewCamera.GetComponent<UniversalAdditionalCameraData>();
            if (urpCamData == null) urpCamData = _previewCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            urpCamData.renderPostProcessing = true;

            _localVolume = camGO.AddComponent<Volume>();
            _localVolume.isGlobal = true;
            _localVolume.priority = 9999;
            _localVolume.enabled = false;

            UpdateRenderResolution();
        }

        private void RefreshCameraDropdownOptions()
        {
            _foundCameras.Clear();
            List<string> options = new List<string> { "Active Scene View Camera" };

#if UNITY_6000_4_OR_NEWER
            Camera[] allCams = Object.FindObjectsByType<Camera>();
#else
            Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
#endif
            foreach (Camera cam in allCams)
            {
                if (cam.gameObject.name == "VNTG_Preview_Camera") continue;

                _foundCameras.Add(cam);
                options.Add($"{cam.gameObject.name} ({cam.tag})");
            }

            _cameraDropdown.choices = options;

            if (_useSceneViewFallback || _explicitSelectedCamera == null)
            {
                _cameraDropdown.SetValueWithoutNotify("Active Scene View Camera");
            }
        }

        private void OnDropdownValueChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "Active Scene View Camera" || string.IsNullOrEmpty(evt.newValue))
            {
                _useSceneViewFallback = true;
                _explicitSelectedCamera = null;
            }
            else
            {
                int selectedIndex = _cameraDropdown.choices.IndexOf(evt.newValue) - 1;
                if (selectedIndex >= 0 && selectedIndex < _foundCameras.Count)
                {
                    _explicitSelectedCamera = _foundCameras[selectedIndex];
                    _useSceneViewFallback = false;
                }
            }
        }

        private void UpdateRenderResolution()
        {
            float layoutWidth = float.IsNaN(_viewportContainer.layout.width) ? 256f : _viewportContainer.layout.width;
            float layoutHeight = float.IsNaN(_viewportContainer.layout.height) ? 256f : _viewportContainer.layout.height;

            if (layoutWidth <= 0 || layoutHeight <= 0) return;

            float targetAspect = 16f / 9f;

            float containerAspect = layoutWidth / layoutHeight;
            float finalWidth = layoutWidth;
            float finalHeight = layoutHeight;

            if (containerAspect > targetAspect)
            {
                finalWidth = layoutHeight * targetAspect;
            }
            else
            {
                finalHeight = layoutWidth / targetAspect;
            }

            float leftOffset = (layoutWidth - finalWidth) / 2f;
            float topOffset = (layoutHeight - finalHeight) / 2f;

            _imageBounds = new Rect(leftOffset, topOffset, finalWidth, finalHeight);

            _baseImage.style.width = finalWidth;
            _baseImage.style.height = finalHeight;
            _baseImage.style.left = leftOffset;
            _baseImage.style.top = topOffset;

            float scaleModifier = 1.0f;
            int targetWidth = Mathf.Max(64, Mathf.RoundToInt(finalWidth * scaleModifier));
            int targetHeight = Mathf.Max(64, Mathf.RoundToInt(finalHeight * scaleModifier));

            if (_renderTextureBase == null || _renderTextureBase.width != targetWidth || _renderTextureBase.height != targetHeight)
            {
                if (_renderTextureBase != null) _renderTextureBase.Release();
                if (_renderTextureFiltered != null) _renderTextureFiltered.Release();

                _renderTextureBase = new RenderTexture(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);
                _renderTextureBase.Create();

                _renderTextureFiltered = new RenderTexture(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);
                _renderTextureFiltered.Create();

                _baseImage.style.backgroundImage = Background.FromRenderTexture(_renderTextureBase);
                _filteredImage.style.backgroundImage = Background.FromRenderTexture(_renderTextureFiltered);
            }
        }

        private void TogglePreview(bool enable)
        {
            if (enable)
                EditorApplication.update += Render;
            else
                EditorApplication.update -= Render;
        }

        private void Render()
        {
            if (_previewCamera == null || _renderTextureBase == null || _renderTextureFiltered == null) return;

            if (!_useSceneViewFallback && _explicitSelectedCamera != null)
            {
                _previewCamera.transform.position = _explicitSelectedCamera.transform.position;
                _previewCamera.transform.rotation = _explicitSelectedCamera.transform.rotation;
                _previewCamera.fieldOfView = _explicitSelectedCamera.fieldOfView;
                _previewCamera.orthographic = _explicitSelectedCamera.orthographic;
                _previewCamera.orthographicSize = _explicitSelectedCamera.orthographicSize;
                _previewCamera.cullingMask = _explicitSelectedCamera.cullingMask;
                UpdateVolumeWithPalette(_explicitSelectedCamera);
            }
            else if (SceneView.lastActiveSceneView != null)
            {
                _previewCamera.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                _previewCamera.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                _previewCamera.fieldOfView = 60.0f;
                _previewCamera.orthographic = SceneView.lastActiveSceneView.orthographic;
                _previewCamera.orthographicSize = SceneView.lastActiveSceneView.camera.orthographicSize;
                _previewCamera.cullingMask = SceneView.lastActiveSceneView.camera.cullingMask;
                UpdateVolumeWithPalette(SceneView.lastActiveSceneView.camera);
            }

            if (!_internalRuntimeCloneProfile.TryGet(out PSXEffectSettings psxSettings))
            {
                psxSettings = _internalRuntimeCloneProfile.Add<PSXEffectSettings>(true);
            }

            if (!_internalRuntimeCloneProfile.TryGet(out CRTSettings crtSettings))
            {
                crtSettings = _internalRuntimeCloneProfile.Add<CRTSettings>(true);
            }

            crtSettings.Enabled.value = false;

            _previewCamera.targetTexture = _renderTextureBase;
            _localVolume.enabled = true;
            psxSettings.EnableColorPalette.value = false;
            psxSettings.EnableFog.value = false;
            _previewCamera.Render();

            _previewCamera.targetTexture = _renderTextureFiltered;
            psxSettings.EnableColorPalette.value = true;
            psxSettings.EnableFog.value = false;
            psxSettings.Enabled.value = true;
            psxSettings.ShowInSceneView.value = true;
            _previewCamera.Render();
            _localVolume.enabled = false;

            _viewportContainer.MarkDirtyRepaint();
        }

        private void Cleanup()
        {
            if (_internalRuntimeCloneProfile != null)
            {
                Object.DestroyImmediate(_internalRuntimeCloneProfile);
                _internalRuntimeCloneProfile = null;
            }

            if (_previewCamera != null)
            {
                Object.DestroyImmediate(_previewCamera.gameObject);
                _previewCamera = null;
            }

            if (_renderTextureBase != null)
            {
                _renderTextureBase.Release();
                Object.DestroyImmediate(_renderTextureBase);
                _renderTextureBase = null;
            }

            if (_renderTextureFiltered != null)
            {
                _renderTextureFiltered.Release();
                Object.DestroyImmediate(_renderTextureFiltered);
                _renderTextureFiltered = null;
            }
        }
    }
}