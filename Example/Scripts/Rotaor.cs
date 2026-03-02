using UnityEngine;

namespace ColbyO.VNTG.Example
{
    public class Rotaor : MonoBehaviour
    {
        [SerializeField] private float _duration;

        float _t;

        private void Awake()
        {
            _t = 0.0f;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.SetY(0.0f));
        }

        private void Update()
        {
            _t += Time.deltaTime / _duration;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.SetY(Mathf.Lerp(0.0f, 360.0f, _t)));
        }
    }
}
