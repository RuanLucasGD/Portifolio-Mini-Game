namespace MMV
{
    /// <summary>
    /// does all acceleration and braking calculations for wheeled vehicles
    /// </summary>
    [System.Serializable]
    public class MMV_WheeledEngine : MMV_Engine
    {
        private float currentAcceleration;
        private float currentBrakeForce;

        /// <summary>
        /// Current engine acceleration force
        /// </summary>
        public float CurrentAcceleration => currentAcceleration;

        /// <summary>
        /// Current engine brake force
        /// </summary>
        public float CurrentBrakeForce => currentBrakeForce;

        /// <summary>
        /// Owner of the engine
        /// </summary>
        /// <value></value>
        public new MMV_WheeledVehicle Vehicle => (MMV_WheeledVehicle)base.Vehicle;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            GetCurrentAccelerationForce(Vehicle.VelocityKMH, out float accForce);
            GetCurrentBrakeForce(isBraking, out float brakeForce);

            currentAcceleration = accForce;
            currentBrakeForce = brakeForce;
            currentAcceleration *= vertical;

            if (isBraking) currentAcceleration = 0;
            else currentBrakeForce = 0;
        }

        public override void Update()
        {
            base.Update();
            var _velocity = Vehicle.VelocityMs;
            _velocity = Vehicle.transform.InverseTransformDirection(_velocity);
            _velocity.x = 0f;
            _velocity.y = 0f;
            _velocity = Vehicle.transform.TransformDirection(_velocity);
            _velocity = MMV_Engine.MsToKMH(_velocity);

            EngineSound.UseEngineSound(this, _velocity);
        }
    }
}