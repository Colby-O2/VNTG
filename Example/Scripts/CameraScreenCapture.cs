#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyO.VNTG.Example
{
    public class CameraScreenCapture : MonoBehaviour
    {
        public string fileName;
        public InputAction screenshotAction;
        public Transform _itemParent;

        [SerializeField] private Camera _camera;

        private Camera Camera
        {
            get
            {
                if (!_camera)
                {
                    _camera = GetComponent<Camera>();
                }
                return _camera;
            }
        }

        private void OnEnable()
        {
            screenshotAction.Enable();
        }

        private void OnDisable()
        {
            screenshotAction.Disable();
        }

        private void LateUpdate()
        {
            if (screenshotAction.WasPressedThisFrame()) Capture();
        }

        public void Capture()
        {
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = Camera.targetTexture;

            Camera.Render();

            Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height, TextureFormat.RGBA32_SIGNED, false, true);
            image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = activeRenderTexture;

            byte[] bytes = image.EncodeToPNG();
            Destroy(image);

            string fn = fileName;

            string path = Application.dataPath + "/Textures/ScreenCaptures/" + fn + ".png";
            Debug.Log($"Saving icon to {path}.");
            File.WriteAllBytes(path, bytes);
        }
    }
}
#endif