using UnityEngine;
using UnityEngine.AI;

namespace MMV
{
    /// <summary>
    /// General tracked vehicle simulation system
    /// </summary>
    public sealed class MMV_TrackedVehicle : MMV_Vehicle
    {
        [SerializeField] private MMV_TrackedEngine engine;
        [SerializeField] private MMV_TrackedWheelManager wheels;

        // Used by AI to have smoother movement ignoring curves with insignificant angle
        private const float AI_DEATH_CURVE = 0.1f;

        public MMV_TrackedEngine Engine { get => engine; set => engine = value; }

        /// <summary>
        /// Manage all wheels, applie physics and simulate tracks
        /// </summary>
        /// <value></value>
        public MMV_TrackedWheelManager Wheels { get => wheels; set => wheels = value; }

        /// <inheritdoc/>
        public override int CurrentGear => Engine.CurrentGear;

        /// <summary>
        /// Forward center of gravity (relative to the center of the vehicle's wheels)
        /// </summary>
        /// <value></value>
        public new float CenterOfMassForward
        {
            get => base.CenterOfMassForward;
            set { base.CenterOfMassForward = value; RecalculateCenterOfMass(Wheels); }
        }

        /// <summary>
        /// Up center of gravity (relative to the center of the vehicle's wheels)
        /// </summary>
        /// <value></value>
        public new float CenterOfMassUp
        {
            get => base.CenterOfMassUp;
            set { base.CenterOfMassUp = value; RecalculateCenterOfMass(Wheels); }
        }

        /// <summary>
        /// Speed at which the vehicle is turning in KM/H (uses the X local speed of the wheels)
        /// </summary>
        public float TurnSpeed => Wheels.WheelsVelocity.x;

        /// <summary>
        /// Verify if vehicle is turning stoped
        /// </summary>
        /// <returns></returns>
        public bool IsTurningStoped => Mathf.Round(VerticalInput) == 0f && IsTurning;

        /// <summary>
        /// The direction in which the vehicle is moving in world space
        /// </summary>
        /// <returns></returns>
        public Vector3 MoveDirection => transform.TransformDirection(new Vector3(HorizontalInput, 0, VerticalInput).normalized);

        /// <inheritdoc/>
        public override bool IsStranded
        {
            get
            {
                var _isStranded = false;
                if ((int)wheels.WheelsVelocity.z == 0 && Mathf.Round(Mathf.Abs(VerticalInput) + Mathf.Abs(HorizontalInput)) != 0)
                {
                    _isStranded = true;
                }
                return _isStranded;
            }
        }

        protected override void SetupVehicle()
        {
            base.SetupVehicle();

            RecalculateCenterOfMass(Wheels);

            Engine.SetupEngine(this);
            Wheels.SetupWheels(this);

            onStart = MbtStart;
            onUpdate = MbtUpdate;
            onFixedUpdate = MbtFixedUpdate;
            onLatedUpdate = MbtLateUpdate;
        }

        private void MbtAwake() { }

        private void MbtStart() { }

        private void MbtFixedUpdate()
        {
            Engine.FixedUpdate();
            wheels.FixedUpdate();

            if (Wheels.LeftTracksOnGround && Wheels.RightTracksOnGround)
            {
                Engine.DecelerationBySlopeAngle(Rb);
            }
        }

        private void MbtUpdate()
        {
            Engine.Update();
        }

        private void MbtLateUpdate() { }

        /// <inheritdoc/>
        public override void MoveTo(Vector3 targetPosition, float stopDistance, bool acceptReturns = false, bool useNavMesh = false)
        {
            base.MoveTo(targetPosition, stopDistance, acceptReturns, useNavMesh);

            if (IsBraking)
            {
                return;
            }

            if (useNavMesh)
            {
                targetPosition = MoveDirectionInNavMesh(targetPosition);
            }

            var _braking = false;
            var _vertical = VerticalInput;
            var _horizontal = HorizontalInput;
            var _targetIsOnBack = transform.InverseTransformPoint(targetPosition).z < 0;

            if (!acceptReturns)
            {
                // when target position is on back of vehicle, stop if is on high speed after turn to right or left
                if (IsTurning)
                {
                    if (Mathf.Abs(VelocityKMH) > Engine.CurrentMaxVelocityByDirection / 4)
                    {
                        _braking = true;
                        _horizontal = 0;
                    }
                    else
                    {
                        _horizontal = _horizontal > 0 ? 1 : -1;
                    }

                    _vertical = 0;
                }
            }

            if (_targetIsOnBack)
            {
                _horizontal *= -1;

                if (IsTurning)
                {
                    _vertical = 0;
                }
            }

            PlayerInputs(_vertical, _horizontal, _braking);
        }

        /// <inheritdoc/>
        public override void MoveTo(Vector3 targetPosition, float stopDistance, bool acceptReturns = false, bool useNavMesh = false, float startManeuverTime = 1f, float endManeuverTime = 2f)
        {
            MoveTo(targetPosition, stopDistance, acceptReturns, useNavMesh);
            ManeuverWhenStranded(startManeuverTime, endManeuverTime);
        }
    }
}
