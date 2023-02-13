using UnityEngine;
using UnityEngine.AI;

namespace MMV
{
    public sealed class MMV_WheeledVehicle : MMV_Vehicle
    {
        [SerializeField] private MMV_WheeledEngine engine;
        [SerializeField] private MMV_WheeledWheelManager wheels;

        /// <summary>
        /// Engine of vehicle
        /// </summary>
        /// <value></value>
        public MMV_WheeledEngine Engine { get => engine; set => engine = value; }

        /// <summary>
        /// Vehicle physics and control simulation system
        /// </summary>
        /// <value></value>
        public MMV_WheeledWheelManager Wheels { get => wheels; set => wheels = value; }

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

        protected override void SetupVehicle()
        {
            base.SetupVehicle();
            RecalculateCenterOfMass(Wheels);

            Engine.SetupEngine(this);
            Wheels.SetupWheels(this);

            onAwake = WheeledAwake;
            onStart = WheeledStart;
            onFixedUpdate = WheeledFixedUpdate;
            onUpdate = WheeledUpdade;
        }

        private void WheeledAwake() { }
        private void WheeledStart() { }

        private void WheeledUpdade()
        {
            Engine.Update();
        }

        private void WheeledFixedUpdate()
        {
            Engine.FixedUpdate();
            Wheels.FixedUpdate();

            if (Wheels.LeftTracksOnGround && Wheels.RightTracksOnGround)
            {
                Engine.DecelerationBySlopeAngle(Rb);
            }
        }

        /// <inheritdoc/>
        public override void MoveTo(Vector3 targetPosition, float stopDistance, bool acceptReturns = false, bool useNavMesh = false)
        {
            targetPosition.y = transform.position.y;

            if (Vector3.Distance(transform.position, targetPosition) < stopDistance)
            {
                PlayerInputs(0, 0, true);
                return;
            }

            if (useNavMesh)
            {
                targetPosition = MoveDirectionInNavMesh(targetPosition);
            }

            base.MoveTo(targetPosition, 0, acceptReturns, useNavMesh);

            if (IsBraking)
            {
                return;
            }

            var _braking = false;
            var _vertical = VerticalInput;
            var _horizontal = HorizontalInput;

            // reduce velocity when is turn to avoid out of control 
            if (IsTurning)
            {
                if (Mathf.Abs(VelocityKMH) > Engine.CurrentMaxVelocityByDirection / 4)
                {
                    _braking = true;
                }
            }

            if (Engine.IsReversingAcceleration)
            {
                _vertical = 0f;
                _braking = true;
            }

            var _inverseDirection = transform.InverseTransformPoint(targetPosition);
            var _targetIsOnBack = _inverseDirection.z < 0;
            var _smoothSteer = Mathf.Abs(Mathf.Abs(_inverseDirection.x) / Mathf.Abs(_inverseDirection.z));

            // when is moving to backward
            if (_targetIsOnBack)
            {
                _horizontal = HorizontalInput >= 0 ? -1 : 1;

                if (acceptReturns)
                {
                    _vertical = -1;
                    _horizontal = _smoothSteer * _horizontal;
                }
                else
                {
                    _vertical = 1;
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

