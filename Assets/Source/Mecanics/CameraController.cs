using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Mecanics
{
    public class CameraController : MonoBehaviour
    {
        public Transform target;
        public float distance;
        public float height;
        public Vector3 direction;

        private Vector2 ScreenProporcion => new Vector2((float)Screen.width / Screen.height, (float)Screen.height / Screen.width);
        public float DistanceByScreenSize => ScreenProporcion.magnitude * distance;

        void Start()
        {

        }

        void FixedUpdate()
        {

        }

        void Update()
        {
            var _cameraDirection = direction * DistanceByScreenSize;
            transform.position = target.position + _cameraDirection;
            transform.LookAt(target.position);
        }
    }
}