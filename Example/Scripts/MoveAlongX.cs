using UnityEngine;

namespace ColbyO.VNTG.Example
{
    public class MoveAlongX : MonoBehaviour
    {
        [SerializeField] private float _distance = 5f;
        [SerializeField] private float _speed = 2f;

        private Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            float offset = Mathf.PingPong(Time.time * _speed, _distance * 2) - _distance;
            transform.position = startPos + transform.right * offset;
        }
    }
}