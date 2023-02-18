using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Makes all acceleration and braking calculations for the vehicle's tracks
    /// </summary>
    [System.Serializable]
    public class MMV_TrackedEngine : MMV_Engine
    {
        [SerializeField] private float turnSpeed;

        private float currentAcceleration;
        private float currentBrakeForce;

        /// <summary>
        /// Current engine acceleration force
        /// </summary>
        public float CurrentAcceleration => currentAcceleration;

        /// <summary>
        /// Current vehicle brake force
        /// </summary>
        public float CurrentBrakeForce => currentBrakeForce;

        /// <summary>
        /// The vehicle owner of this engine
        /// </summary>
        /// <returns></returns>
        public new MMV_TrackedVehicle Vehicle => (MMV_TrackedVehicle)base.Vehicle;

        /// <summary>
        /// Vehicle Turn Speed relative to max vehicle velocity
        /// </summary>
        /// <value></value>
        public float TurnSpeed
        {
            get => turnSpeed;
            set
            {
                value = Mathf.Clamp(value, 0.01f, 1f);
                turnSpeed = value;
            }
        }

        /// <summary>
        /// Return a value bettwen 0 to 1 of turn velocity curve, when 0 is low turn speed and 1 is max turn speed.
        /// </summary>
        public float CurrentTurnSpeedBySteer
        {
            get
            {
                // control max velocity only if is turning vehicle
                var _horizontal = Mathf.RoundToInt(Mathf.Abs(Vehicle.HorizontalInput));
                var _vertical = Mathf.RoundToInt(Mathf.Abs(Vehicle.VerticalInput));
                var _maxVelocity = Mathf.Lerp(_vertical, TurnSpeed, Mathf.Max(_horizontal - _vertical, 0));
                return _maxVelocity;
            }
        }

        /// <summary>
        /// Return a value bettwen 0 - 1 of slowdown curve, when 0 is low slowdown force and 1 is max slowdown force.
        /// </summary>
        /// <value></value>
        public float CurrentSlowdownByVelocity
        {
            get
            {
                if (!EngineSettings)
                {
                    return 0f;
                }

                var _velocity = Mathf.Abs(Vehicle.VelocityKMH) / CurrentMaxVelocityByDirection;
                var _vertical = 1 - Mathf.Abs(vertical);
                var _curve = (EngineSettings.SlowdownByVelocityCurve.Evaluate(_velocity)) * _vertical;

                _curve = Mathf.Clamp01(_curve);

                return _curve;
            }
        }

        /// <summary>
        /// Return the velocity to forward of backward relative to vehicle of wheels in KM/H 
        /// </summary>
        /// <returns></returns>
        private float WheelsForwardVelocity => Mathf.Abs(Vehicle.Wheels.WheelsVelocity.z);

        public MMV_TrackedEngine()
        {
            TurnSpeed = 0.3f;
        }

        public override void Update()
        {
            base.Update();
            EngineSound.UseEngineSound(this, Vehicle.Wheels.WheelsVelocity);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            GetCurrentAccelerationForce(Vehicle.VelocityKMH, out float accForce);
            GetCurrentBrakeForce(isBraking, out float brakeForce);

            var _accelerationInput = ConvertInputsToAcceleration(vertical, horizontal);

            currentAcceleration = accForce * _accelerationInput;
            currentBrakeForce = brakeForce;

            if (isBraking) currentAcceleration = 0;
        }

        protected override void GetCurrentAccelerationForce(float currentSpeed, out float accelerationForce)
        {
            base.GetCurrentAccelerationForce(currentSpeed, out accelerationForce);

            var _recommendedMaxVelocity = CurrentMaxVelocityByDirection * CurrentTurnSpeedBySteer;

            if (WheelsForwardVelocity > _recommendedMaxVelocity)
            {
                accelerationForce = 0f;
            }
        }

        protected override void GetCurrentBrakeForce(bool isBraking, out float brakeForce)
        {
            if (!EngineSettings)
            {
                Debug.LogWarningFormat($"No Engine Configuration passed to Whole Vehicle {Vehicle.name}");
                brakeForce = 0;
                return;
            }

            var _recommendedMaxVelocity = CurrentMaxVelocityByDirection * CurrentTurnSpeedBySteer;
            var _slowdown = CurrentSlowdownByVelocity * EngineSettings.Slowdown;

            var _finalBrakeForce = 0f;

            if (isBraking || IsReversingAcceleration) _finalBrakeForce = EngineSettings.MaxBrakeForce;
            else if (WheelsForwardVelocity > _recommendedMaxVelocity) _finalBrakeForce = _slowdown;
            else if (!Vehicle.IsAccelerating) _finalBrakeForce = _slowdown;

            brakeForce = _finalBrakeForce;
        }

        // for the vehicle to turn it is necessary to accelerate the wheels, so convert the steering input into acceleration
        private float ConvertInputsToAcceleration(float vertical, float horizontal)
        {
            var _acceleration = vertical;                                           // accelerating
            _acceleration += Mathf.Abs(horizontal) * (_acceleration >= 0 ? 1 : -1); // stoped steering and steering accelerating

            _acceleration = Mathf.Clamp(_acceleration, -1, 1);
            return _acceleration;
        }
    }
}