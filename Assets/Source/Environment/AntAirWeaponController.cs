using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMV;

namespace Game.Environment
{
    public class AntAirWeaponController : MonoBehaviour
    {
        public float RandomInterval;
        public MMV_ShooterManager[] Weapons;

        void Start()
        {
            InvokeRepeating(nameof(SetRandomTargetPosition), 0, RandomInterval);
        }

        private void SetRandomTargetPosition()
        {
            foreach (var w in Weapons)
            {
                w.TargetPosition = transform.position + new Vector3(Random.Range(-1000, 1000), Random.Range(100, 1000), Random.Range(-1000, 1000));
            }
        }
    }
}
