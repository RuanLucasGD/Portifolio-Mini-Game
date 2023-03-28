using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Environment
{
    public class AutoRotator : MonoBehaviour
    {
        public float Velocity;
        public Vector3 Axis;

        void Update()
        {
            transform.Rotate(Axis * Velocity * Time.deltaTime, Space.Self);
        }
    }
}


