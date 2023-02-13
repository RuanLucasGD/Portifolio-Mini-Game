using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Manages all wheels
    /// </summary>
    [Serializable]
    public class MMV_TrackedWheelManager : MMV_WheelManager
    {
        /// <summary>
        /// Emits particles when the vehicle is in motion
        /// </summary>
        [Serializable]
        public class TrackParticles
        {
            public const float MIN_PARTICLES_EMISSION = 0;
            public const float MAX_PARTICLES_EMISSION = 10;

            [SerializeField] private float maxEmission;

            [SerializeField] private ParticleSystem leftParticle;
            [SerializeField] private ParticleSystem rightParticle;

            /// <summary>
            /// Set max emission of the particles.
            /// emission = velocity * maxEmission.
            /// </summary>
            /// <value></value>
            public float MaxEmission
            {
                get => maxEmission;
                set => maxEmission = Mathf.Clamp(value, MIN_PARTICLES_EMISSION, MAX_PARTICLES_EMISSION);
            }

            /// <summary>
            /// Left track particle
            /// </summary>
            /// <value></value>
            public ParticleSystem LeftParticle { get => leftParticle; set => leftParticle = value; }

            /// <summary>
            /// RIght track particle
            /// </summary>
            /// <value></value>
            public ParticleSystem RightParticle { get => rightParticle; set => rightParticle = value; }

            /// <summary>
            /// Set up particle system
            /// </summary>
            public void SetupParticle()
            {
                if (!leftParticle || !rightParticle)
                {
                    return;
                }

                var _leftMainModule = leftParticle.main;
                var _rightMainModule = rightParticle.main;

                _leftMainModule.loop = true;
                _rightMainModule.loop = true;
            }

            public void UseParticles(MMV_Wheel[] leftWheels, MMV_Wheel[] rightWheels)
            {
                if (!leftParticle || !rightParticle)
                {
                    return;
                }

                var _effect = new MMV_WheelsEffects();

                _effect.ControlWheelsDustParticleEmission(leftWheels, leftParticle, MaxEmission);
                _effect.ControlWheelsDustParticleEmission(rightWheels, RightParticle, MaxEmission);
            }
        }

        [SerializeField] private float trackMoveSpeed;

        [SerializeField] private Renderer leftTrack;
        [SerializeField] private Renderer rightTrack;

        [SerializeField] private TrackParticles tracksParticles;

        [SerializeField] private Transform[] leftAdditionalWheelsRenderers;
        [SerializeField] private Transform[] rightAdditionalWheelsRenderers;

        // material of tracks (used for make moviment effect)
        [NonSerialized] private Material leftTrackMaterial;
        [NonSerialized] private Material rightTrackMaterial;

        private Bounds wheelBounds;

        private float maxWheelPosition_Z;
        private float maxWheelPosition_ZNegative;

        //-------------------------------------------------------

        /// <summary>
        /// Track movement velocity
        /// </summary>
        public float TrackMoveSpeed { get => trackMoveSpeed; set => trackMoveSpeed = value; }

        /// <summary>
        /// Left track renderer
        /// </summary>
        public Renderer LeftTrack { get => leftTrack; set => leftTrack = value; }

        /// <summary>
        /// Right track renderer
        /// </summary>
        public Renderer RightTrack { get => rightTrack; set => rightTrack = value; }

        /// <summary>
        /// Get the additional wheels that do not have physics on the left side
        /// </summary>
        public Transform[] AdditionalWheelMeshLeft => leftAdditionalWheelsRenderers;

        /// <summary>
        /// Get the additional wheels that do not have physics on the right side
        /// </summary>
        public Transform[] AdditionalWheelMeshRight => rightAdditionalWheelsRenderers;

        /// <summary>
        /// Material of the left track 
        /// </summary>
        /// <value></value>
        public Material LeftTrackMaterial => leftTrackMaterial;

        /// <summary>
        /// Material of the right track 
        /// </summary>
        /// <value></value>
        public Material RightTrackMaterial => rightTrackMaterial;

        /// <summary>
        /// emission of particles during the movement of the vehicle
        /// </summary>
        /// <value></value>
        public TrackParticles TracksParticles { get => tracksParticles; set => tracksParticles = value; }

        /// <summary>
        /// The vehicle ower of this module
        /// </summary>
        /// <returns></returns>
        public new MMV_TrackedVehicle Vehicle => (MMV_TrackedVehicle)base.Vehicle;

        public MMV_TrackedWheelManager()
        {
            TrackMoveSpeed = 1.0f;
            WheelsLeft = new MMV_Wheel[] { };
            WheelsRight = new MMV_Wheel[] { };
        }

        /// <summary>
        /// Generate wheels configurations
        /// </summary>
        /// <param name="vehicle">
        /// Owner of wheels
        /// </param>
        public override void SetupWheels(MMV_Vehicle vehicle)
        {
            base.SetupWheels(vehicle);

            // getting material of tracks
            if (leftTrack) leftTrackMaterial = LeftTrack.material;
            if (rightTrack) rightTrackMaterial = RightTrack.material;

            foreach (var w in WheelsLeft) Apply(w);
            foreach (var w in WheelsRight) Apply(w);

            tracksParticles.SetupParticle();

            CalculateWheelBounds(out maxWheelPosition_Z, out maxWheelPosition_ZNegative);

            void Apply(MMV_Wheel w)
            {
                w.SetupWheel(this, vehicle.Rb);
                w.Settings = Settings;
                w.ApplyAcceleration = true;
                w.ApplyBrake = true;
                w.MaxSteerAngle = 90;
                w.MeshApplyAccelerationRotation = true;
                w.MeshApplySteerRotation = false;
            }
        }

        public override void FixedUpdate()
        {
            if (!Settings)
            {
                return;
            }

            base.FixedUpdate();
            var _engine = Vehicle.Engine;
            var _acc = _engine.CurrentAcceleration;
            var _steer = CalculateSteer(Vehicle.VerticalInput, Vehicle.HorizontalInput);
            var _brake = _engine.CurrentBrakeForce;
            var _higherMaxVelocity = _engine.CurrentMaxVelocityByDirection;

            var _leftVelocity = WheelsMovementVelocity(WheelsLeft).z;
            var _rightVelocity = WheelsMovementVelocity(WheelsRight).z;

            var _leftWheelRot = (Vector3.right * _leftVelocity);
            var _rightWheelRot = (Vector3.right * _rightVelocity);

            var _steerBySteeringVelocity = SteerByVelocity(Settings, _engine);

            if (!Vehicle.VehicleControlsEnabled)
            {
                _acc = 0;
                _steer = 0;
            }

            foreach (var w in WheelsLeft) if (w.Mesh) ApplyPhysics(w, _acc, _steer);
            foreach (var w in WheelsRight) if (w.Mesh) ApplyPhysics(w, _acc, _steer);

            // apply rotation on additional wheel meshs
            foreach (var w in leftAdditionalWheelsRenderers) if (w) w.Rotate(_leftWheelRot);
            foreach (var w in rightAdditionalWheelsRenderers) if (w) w.Rotate(_rightWheelRot);

            // applie rotation on track
            if (leftTrackMaterial) leftTrackMaterial.mainTextureOffset += new Vector2(_leftWheelRot.z, _leftWheelRot.x) * trackMoveSpeed * Time.fixedDeltaTime;
            if (rightTrackMaterial) rightTrackMaterial.mainTextureOffset += new Vector2(_rightWheelRot.z, _rightWheelRot.x) * trackMoveSpeed * Time.fixedDeltaTime;

            // --- Applie particle system
            tracksParticles.UseParticles(WheelsLeft, WheelsRight);

            void ApplyPhysics(MMV.MMV_Wheel w, float acceleration, float steer)
            {
                var _steerByDistanceOfCenter = SteerByDistanceCenterOfMass(w.Mesh.position);
                var _accelerationByDistanceOfCenter = Mathf.Abs(_steerByDistanceOfCenter);
                var _accelerationBySpeed = AccelerationByCurrentVelocityKMH(MMV_Engine.MsToKMH(w.LocalVelocity.magnitude));
                steer *= _steerByDistanceOfCenter;
                steer *= _steerBySteeringVelocity;
                acceleration *= _accelerationBySpeed;
                acceleration *= _accelerationByDistanceOfCenter;

                w.UseWheel(acceleration, steer, _brake);
            }
        }

        // controls the direction the wheels should turn
        private float CalculateSteer(float vertical, float horizontal)
        {
            var _steer = horizontal * Mathf.Abs(vertical);
            _steer += horizontal;
            _steer = Mathf.Clamp(_steer, -1, 1);

            return _steer;
        }

        /// <summary>
        /// Returns a value ranging from 0 to 1 relative to distance of center fo mass
        /// </summary>
        /// <param name="wheelPos">wheel position in world</param>
        /// <returns></returns>
        private float SteerByDistanceCenterOfMass(Vector3 wheelPos)
        {
            wheelPos = Vehicle.transform.InverseTransformPoint(wheelPos);
            wheelPos = wheelPos - Vehicle.Rb.centerOfMass;
            var _maxWheelPos = wheelPos.z >= 0 ? maxWheelPosition_Z : maxWheelPosition_ZNegative;
            return wheelPos.z / Mathf.Abs(_maxWheelPos);
        }

        /// <summary>
        /// Returns a value ranging from 0 to 1 depending on the current vehicle speed
        /// </summary>
        /// <param name="currentVelocity">
        /// Current velocity in km/h
        /// </param>
        /// <returns></returns>
        private float AccelerationByCurrentVelocityKMH(float currentVelocity)
        {
            return 1 - (Mathf.Abs(currentVelocity) / Vehicle.Engine.CurrentMaxVelocityByDirection);
        }

        /// <summary>
        /// Takes the positions (Z axis) of the wheels farthest from the vehicle's center of mass
        /// </summary>
        /// <param name="maxWheelPosForward">
        /// Front position furthest from center of mass
        /// </param>
        /// <param name="maxWheelPosBackward">
        /// Back position furthest from center of mass
        /// </param>
        private void CalculateWheelBounds(out float maxWheelPosForward, out float maxWheelPosBackward)
        {
            GetMaxWheelPos(WheelsLeft, out maxWheelPosForward, out maxWheelPosBackward);
            GetMaxWheelPos(WheelsRight, out maxWheelPosForward, out maxWheelPosBackward);

            void GetMaxWheelPos(MMV_Wheel[] wheels, out float maxForward, out float maxBackward)
            {
                maxForward = 0f;
                maxBackward = 0f;
                var _centerZ = Vehicle.Rb.centerOfMass.z;

                foreach (var w in wheels)
                {
                    var _wheelPosZ = Vehicle.transform.InverseTransformPoint(w.Mesh.position).z - _centerZ;
                    if (_wheelPosZ >= 0)
                    {
                        if (_wheelPosZ >= maxForward) maxForward = _wheelPosZ;
                    }
                    else
                    {
                        if (_wheelPosZ <= maxBackward) maxBackward = _wheelPosZ;
                    }
                }
            }
        }
    }
}
