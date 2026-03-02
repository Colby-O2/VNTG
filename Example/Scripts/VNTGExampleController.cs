using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

using ColbyO.VNTG.CRT;
using ColbyO.VNTG.PSX;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    VNTGExampleController.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    public class VNTGExampleController : MonoBehaviour
    {
        [SerializeField] private Volume _volume;

        [SerializeField] private GameObject _psxMaterialScene;
        [SerializeField] private GameObject _defaultMaterialScene;

        [SerializeField] private GameObject _menu;
        [SerializeField] MovementController _player;

        [SerializeField] private Button _psxSettingsButton;
        [SerializeField] private Button _crtSettingsButton;

        [SerializeField] private VolumeComponentUICreator _settings;

        private bool _psxSceneEnabled = true;

        private PSXEffectSettings _psx; 
        private CRTSettings _crt;

        private void Awake()
        {
            _volume.profile.TryGet(out _psx);
            _volume.profile.TryGet(out _crt);

            _psxMaterialScene.SetActive(true);
            _defaultMaterialScene.SetActive(false);

            OpenPSXSettings();

            _psxSettingsButton.onClick.AddListener(OpenPSXSettings);
            _crtSettingsButton.onClick.AddListener(OpenCRTSettings);
        }

        private void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) ToggleMenu();
            if (Keyboard.current.eKey.wasPressedThisFrame) TogglePSXEffect();
            if (Keyboard.current.rKey.wasPressedThisFrame) ToggleCRTEffect();
            if (Keyboard.current.fKey.wasPressedThisFrame) ToggleShadows();
        }

        public void ToggleShadows()
        {
            UniversalRenderPipelineAsset urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            urpAsset.shadowDistance = (urpAsset.shadowDistance == 0.0f) ? 100.0f : 0.0f;
        }

        public void ToggleMenu()
        {
            _menu.SetActive(!_menu.activeSelf);
            if (!_menu.activeSelf)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                _player.Unfreeze();
            }
            else
            {
                OpenPSXSettings();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                _player.Freeze();
            }
        }

        public void OpenPSXSettings()
        {
            _settings.RefreshUI("PSXEffectSettings");
        }

        public void OpenCRTSettings()
        {
            _settings.RefreshUI("CRTSettings");
        }

        public void TogglePSXEffect()
        {
            if (_psx) _psx.Enabled.value = !_psx.Enabled.value;
            TogglePSXMaterials();
        }

        public void ToggleFogEffect()
        {
            if (_psx) _psx.EnableFog.value = !_psx.EnableFog.value;
        }

        public void ToggleCRTEffect()
        {
            if (_crt) _crt.Enabled.value = !_crt.Enabled.value;
        }

        public void TogglePSXMaterials()
        {
            _psxSceneEnabled = !_psxSceneEnabled;
            _psxMaterialScene.SetActive(_psxSceneEnabled);
            _defaultMaterialScene.SetActive(!_psxSceneEnabled);
        }
    }
}