using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Basis for creating specific types of vehicles
    /// </summary>
    public class MMV_Vehicle : MonoBehaviour
    {
        [SerializeField] private float centerOfMassUp;
        [SerializeField] private float centerOfMassForward;

        // control the vehicle
        private float horizontal;
        private float vertical;
        private bool isBraking;
        private bool vehicleControlEnabled;

        private float strandedTime;
        private bool isManeuvering;
        private float maneuveringTime;

        private Rigidbody rb;
        private UnityEngine.AI.NavMeshPath aiNavMeshPath;

        /// <summary>
        /// Get vehicle rigidBody component
        /// </summary>
        public Rigidbody Rb
        {
            get
            {
                if (!rb)
                {
                    rb = GetComponent<Rigidbody>();
                }

                return rb;
            }
            set => rb = value;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// get vehicle velocity (meters per secound) in local space
        /// </returns>
        public Vector3 VelocityMs => transform.InverseTransformDirection(Rb.velocity);

        /// <summary>
        /// vehicle speed (provided by RigidBody) to kilometers per hour (KM/H)
        /// </summary>
        public float VelocityKMH => VelocityMs.z * 3.6f;

        /// <summary>
        /// Get Current Gear of the engine
        /// </summary>
        public virtual int CurrentGear => 0;

        /// <summary>
        /// Pass player controls to vehicle
        /// </summary>
        /// <param name="vertical">
        /// how much the vehicle should accelerate forward (from -1 to 1)
        /// </param>
        /// <param name="horizontal">
        /// how much the vehicle should turn left or right (from -1 to 1)
        /// </param>
        /// <param name="isBraking">
        /// If the vehicle is braking
        /// </param>
        public void PlayerInputs(float vertical, float horizontal, bool isBraking)
        {
            vertical = Mathf.Clamp(vertical, -1, 1);
            horizontal = Mathf.Clamp(horizontal, -1, 1);

            this.vertical = vertical;
            this.horizontal = horizontal;
            this.IsBraking = isBraking;
        }

        /// <summary>
        /// Move vehicle to position
        /// </summary>
        /// <param name="targetPosition">world position to move</param>
        /// <param name="stopDistance">min distance of position to stop vehicle</param>
        /// <param name="acceptReturns">if the target position is behind the vehicle it should accelerate backwards</param>
        /// <param name="useNavMesh">when true, vehicle uses scene Navmesh to calculate path to target position</param>
        public virtual void MoveTo(Vector3 targetPosition, float stopDistance, bool acceptReturns = false, bool useNavMesh = false)
        {
            stopDistance = Mathf.Max(stopDistance, 0);
         
            if (!VehicleControlsEnabled)
            {
                PlayerInputs(0, 0, true);
                return;
            }

            if (Vector3.Distance(transform.position, targetPosition) < stopDistance)
            {
                PlayerInputs(0, 0, true);
                return;
            }

            if (useNavMesh)
            {
                targetPosition = MoveDirectionInNavMesh(targetPosition);
            }

            IsBraking = false;
            targetPosition.y = transform.position.y;

            // turn to target direction smoothly
            var _directionToTarget = transform.InverseTransformDirection(targetPosition - transform.position);
            _directionToTarget.x /= _directionToTarget.z;

            if (!acceptReturns)
            {
                _directionToTarget.z = Mathf.Abs(_directionToTarget.z);
                
            }

            PlayerInputs(_directionToTarget.z, _directionToTarget.x, false);
        }

        /// <summary>
        /// Move vehicle to position
        /// </summary>
        /// <param name="targetPosition">world position to move</param>
        /// <param name="stopDistance">min distance of position to stop vehicle</param>
        /// <param name="acceptReturns">if the target position is behind the vehicle it should accelerate backwards</param>
        /// <param name="useNavMesh">when true, vehicle uses scene Navmesh to calculate path to target position</param>
        /// <param name="startManeuverTime">delay to start maneuver after stranded</param>
        /// <param name="endManeuverTime">time to complete maneuver</param>
        public virtual void MoveTo(Vector3 targetPosition, float stopDistance, bool acceptReturns = false, bool useNavMesh = false, float startManeuverTime = 1f, float endManeuverTime = 2f)
        {

            startManeuverTime = Mathf.Max(startManeuverTime, 0.1f);
            endManeuverTime = Mathf.Max(endManeuverTime, 0.2f);

            MoveTo(targetPosition, stopDistance, acceptReturns, useNavMesh);
            ManeuverWhenStranded(startManeuverTime, endManeuverTime);
        }

        /// <summary>
        /// Reverse the acceleration for vehicle go to backward when strand on an obstacle.
        /// </summary>
        /// <param name="startManeuverTime">delay to start maneuver after stranded</param>
        /// <param name="endManeuverTime">time to complete maneuver</param>
        protected void ManeuverWhenStranded(float startManeuverTime = 1f, float endManeuverTime = 2f)
        {
            // after of some time stopped, the vehicle start maneuvering
            if (IsStranded) strandedTime += Time.deltaTime;
            else strandedTime = 0f;

            if (strandedTime > startManeuverTime) isManeuvering = true;

            // go to back
            if (isManeuvering)
            {
                maneuveringTime += Time.deltaTime;
                PlayerInputs(-VerticalInput, 0, false);
                if (maneuveringTime > endManeuverTime)
                {
                    maneuveringTime = 0f;
                    isManeuvering = false;
                }
            }
        }

        /// <summary>
        /// Find target position using Unity NavMesh System
        /// </summary>
        /// <param name="targetPosition">target to find</param>
        /// <returns>position to vehicle move</returns>
        protected Vector3 MoveDirectionInNavMesh(Vector3 targetPosition)
        {
            var _direction = transform.position;

            if (aiNavMeshPath == null)
            {
                aiNavMeshPath = new UnityEngine.AI.NavMeshPath();
            }

            if (!UnityEngine.AI.NavMesh.CalculatePath(transform.position, targetPosition, UnityEngine.AI.NavMesh.AllAreas, aiNavMeshPath))
            {
                return targetPosition;
            }

            if (aiNavMeshPath.corners.Length > 1)
            {
                _direction = aiNavMeshPath.corners[1];
            }

            return _direction;
        }

        /// <summary>
        /// Controller direction input
        /// </summary>
        /// <value></value>
        public float HorizontalInput { set => horizontal = Mathf.Clamp(value, -1, 1); get => horizontal; }

        /// <summary>
        /// Controller throttle input
        /// </summary>
        /// <value></value>
        public float VerticalInput { set => vertical = Mathf.Clamp(value, -1, 1); get => vertical; }

        /// <summary>
        /// If the vehicle controls are enabled
        /// </summary>
        public bool VehicleControlsEnabled { get => vehicleControlEnabled; set => vehicleControlEnabled = value; }

        /// <summary>
        /// Returns true if the vehicle is turning right or left
        /// </summary>
        /// <returns></returns>
        public bool IsTurning => Mathf.Round(horizontal) != 0;

        /// <summary>
        /// Return true if the vehicle is moving to forward or backward
        /// </summary>
        /// <returns></returns>
        public bool IsAccelerating => Mathf.Round(vertical) != 0f;

        /// <summary>
        /// Check that the brake is actuated or activate it
        /// </summary>
        /// <value></value>
        public bool IsBraking { get => isBraking; set => isBraking = value; }

        /// <summary>
        /// Up center of gravity (relative to the center of the vehicle's wheels)
        /// </summary>
        /// <value></value>
        public float CenterOfMassUp { get => centerOfMassUp; set => centerOfMassUp = value; }

        /// <summary>
        /// Forward center of gravity (relative to the center of the vehicle's wheels)
        /// </summary>
        /// <value></value>
        public float CenterOfMassForward { get => centerOfMassForward; set => centerOfMassForward = value; }

        /// <summary>
        /// Return true if the vehicle is accelerating to forward or backward but don't move
        /// </summary>
        /// <value></value>
        public virtual bool IsStranded
        {
            get
            {
                var _isStranded = false;
                if ((int)Rb.velocity.magnitude == 0 && Mathf.Round(vertical) != 0)
                {
                    _isStranded = true;
                }
                return _isStranded;
            }
        }

        protected Action onAwake;
        protected Action onStart;
        protected Action onFixedUpdate;
        protected Action onUpdate;
        protected Action onLatedUpdate;

        protected virtual void SetupVehicle()
        {
            vehicleControlEnabled = true;
        }

        private void OnEnable()
        {
            SetupVehicle();
        }

        private void OnDisable() { }

        private void Start()
        {
            if (onStart != null) onStart();
        }

        private void Update()
        {
            if (onUpdate != null) onUpdate();
        }

        private void FixedUpdate()
        {
            if (onFixedUpdate != null) onFixedUpdate();
        }

        private void LateUpdate()
        {
            if (onLatedUpdate != null) onLatedUpdate();
        }

        // Recalculate vehicle center of mass relative to center of wheels
        public void RecalculateCenterOfMass(MMV_WheelManager wheels)
        {
            var _wheelsBounds = new Bounds(transform.position, Vector3.zero);
            foreach (var w in wheels.WheelsLeft) if (w.Mesh) _wheelsBounds.Encapsulate(w.Mesh.position);
            foreach (var w in wheels.WheelsRight) if (w.Mesh) _wheelsBounds.Encapsulate(w.Mesh.position);
            var _vehicleCenter = transform.InverseTransformPoint(_wheelsBounds.center);

            _vehicleCenter.x = 0f;
            _vehicleCenter.y = 0f;

            _vehicleCenter.z += CenterOfMassForward;
            _vehicleCenter.y += CenterOfMassUp;

            Rb.centerOfMass = _vehicleCenter;
        }
    }
}
