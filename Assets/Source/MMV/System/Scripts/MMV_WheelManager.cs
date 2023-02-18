using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Base for wheel management
    /// </summary>
    public class MMV_WheelManager
    {
        [SerializeField] private MMV_WheelSettings wheelSettings;
        [SerializeField] private MMV_Wheel[] wheelsLeft;
        [SerializeField] private MMV_Wheel[] wheelsRight;

        [NonSerialized] private MMV_Vehicle vehicle;

        private bool leftTrackOnGround;
        private bool rightTrackOnGround;

        /// <summary>
        /// Vehucle owner of this module
        /// </summary>
        public MMV_Vehicle Vehicle => vehicle;

        /// <summary>
        /// Right wheels
        /// </summary>
        /// <value></value>
        public MMV_Wheel[] WheelsRight { get => wheelsRight; set => wheelsRight = value; }

        /// <summary>
        /// Left wheels
        /// </summary>
        /// <value></value>
        public MMV_Wheel[] WheelsLeft { get => wheelsLeft; set => wheelsLeft = value; }

        /// <summary>
        /// Wheel behavior setting
        /// </summary>
        /// <value></value>
        public MMV_WheelSettings Settings { get => wheelSettings; set => wheelSettings = value; }

        /// <summary>
        /// If the left side of vehicle is on ground
        /// </summary>
        public bool LeftTracksOnGround => leftTrackOnGround;

        /// <summary>
        /// If the right side of vehicle is on ground
        /// </summary>
        public bool RightTracksOnGround => rightTrackOnGround;

        /// <summary>
        /// Check if wheels is on grounded
        /// </summary>
        /// <param name="wheels">wheels to check</param>
        /// <returns></returns>
        protected bool WheelsOnGround(MMV_Wheel[] wheels)
        {
            foreach (var w in wheels)
            {
                if (!w.OnGronded) return false;
            }

            return true;
        }

        /// <summary>
        /// return intensity of wheel steering based on "SteerByVelocityCurve" of wheel settings asset
        /// </summary>
        /// <param name="wheelSettings">asset with wheel configurations</param>
        /// <param name="engine">vehicle engine</param>
        /// <returns></returns>
        public float SteerByVelocity(MMV_WheelSettings wheelSettings, MMV_Engine engine)
        {
            var _relativeSpeed = Mathf.Abs(Vehicle.VelocityKMH) / Mathf.Abs(engine.CurrentMaxVelocityByDirection);
            return Mathf.Clamp01(Settings.SteerByVelocityCurve.Evaluate(_relativeSpeed));
        }

        /// <summary>
        /// Returns the speed at which the vehicle's wheels are moving in km/h on local space
        /// </summary>
        /// <value></value>
        public Vector3 WheelsVelocity
        {
            get
            {
                var _leftWheelsSpeed = WheelsMovementVelocity(WheelsLeft);
                var _rightWheelsSpeed = WheelsMovementVelocity(WheelsRight);
                var _moveSpeed = (_leftWheelsSpeed + _rightWheelsSpeed) / 2;

                return MMV_Engine.MsToKMH(_moveSpeed);
            }
        }

        /// <summary>
        /// Get higher velocity of wheels
        /// </summary>
        /// <param name="wheels">
        /// Wheels of some side
        /// </param>
        /// <returns>
        /// Median velocity in local space (meters per secound)
        /// </returns>
        public Vector3 WheelsMovementVelocity(MMV_Wheel[] wheels)
        {
            if (wheels.Length == 0) return Vector3.zero;

            var _higherVelocity = wheels[0].LocalVelocity;

            foreach (var w in wheels)
            {
                if (w.OnGronded)
                {
                    if (w.LocalVelocity.magnitude > _higherVelocity.magnitude)
                    {
                        _higherVelocity = w.LocalVelocity;
                    }
                }
            }

            return _higherVelocity;
        }

        /// <summary>
        /// Generate wheels configurations
        /// </summary>
        /// <param name="vehicle">
        /// Owner of wheels
        /// </param>
        public virtual void SetupWheels(MMV_Vehicle vehicle)
        {
            this.vehicle = vehicle;
        }

        public virtual void FixedUpdate()
        {
            leftTrackOnGround = WheelsOnGround(WheelsLeft);
            rightTrackOnGround = WheelsOnGround(WheelsRight);
        }
    }
}