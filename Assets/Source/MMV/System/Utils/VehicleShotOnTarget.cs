using UnityEngine;
using MMV;

namespace Game
{
    public class VehicleShotOnTarget : MonoBehaviour
    {
        public MMV_ShooterManager weapon;
        public MMV_Vehicle vehicle;
        public float shotInterval;
        public Transform target;

        void Start()
        {
            if (target)
            {
                weapon.TargetPosition = target.position;
            }
            InvokeRepeating(nameof(Shoot), shotInterval, shotInterval);
        }

        private void Update()
        {
            if (vehicle)
            {
                vehicle.IsBraking = true;
            }
        }

        private void Shoot()
        {
            if (!weapon || !target)
            {
                return;
            }

            weapon.Shoot();
        }
    }
}