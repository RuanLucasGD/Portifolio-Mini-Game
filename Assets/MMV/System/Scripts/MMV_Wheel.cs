using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Apply physics simulation on the wheel
    /// </summary>
    [Serializable]
    public class MMV_Wheel
    {
        [SerializeField] private Transform bone;
        [SerializeField] private Transform mesh;

        [SerializeField] private float maxSteerAngle;
        [SerializeField] private bool applyAcceleration;
        [SerializeField] private bool applyBrake;

        //-------------------------------------------------------------------

        private Rigidbody rb;
        private RaycastHit wheelHit;

        private MMV_WheelSettings settings;

        //-------------------------------------------------------------------

        private float lastSpringLength;

        private Vector3 offsetBone; // distance of bone relative to wheel
        private Vector3 wheelMoveSpeed;
        private Vector3 colliderPosition;
        private float currentWheelRot;

        private float currentAcceleration;
        private float currentSteering;
        private float currentBrake;

        //------------------------------------------------------------------

        /// <summary>
        /// Enable wheel accleration rotation
        /// </summary>
        /// <value></value>
        public bool MeshApplyAccelerationRotation { get; set; }

        /// <summary>
        /// Enable wheel steering rotation
        /// </summary>
        /// <value></value>
        public bool MeshApplySteerRotation { get; set; }

        /// <summary>
        /// Current suspension spring size
        /// </summary>
        /// <value></value>
        public float CurrentSpringLength { private set; get; }

        /// <summary>
        /// Current suspension damper force
        /// </summary>
        /// <value></value>
        public float CurrentDamperForce { private set; get; }

        /// <summary>
        /// Speed that the suspension spring is being forced down
        /// </summary>
        /// <value></value>
        public float CompressVelocity { private set; get; }

        /// <summary>
        /// Current suspension spring force
        /// </summary>
        /// <value></value>
        public float CurrentSpringForce { private set; get; }

        /// <summary>
        /// Current suspension force
        /// </summary>
        /// <value></value>
        public float CurrentSuspensionForce { private set; get; }

        /// <summary>
        /// Check if wheel is on ground
        /// </summary>
        /// <value></value>
        public bool OnGronded => wheelHit.transform != null;

        /// <summary>
        /// Get ground info
        /// </summary>
        /// <value></value>
        public RaycastHit WheelHit => wheelHit;

        /// <summary>
        /// Bone of wheel for simulate track suspension
        /// </summary>
        /// <value></value>
        public Transform Bone => bone;

        /// <summary>
        /// MeshRenderer of wheel
        /// </summary>
        /// <value></value>
        public Transform Mesh => mesh;

        /// <summary>
        /// Wheel velocity in local space
        /// </summary>
        public Vector3 LocalVelocity => wheelMoveSpeed;

        /// <summary>
        /// Wheel velocity in world space
        /// </summary>
        /// <returns></returns>
        public Vector3 Velocity => Rb ? Rb.transform.TransformDirection(wheelMoveSpeed) : Vector3.zero;

        /// <summary>
        /// Vehicle RigidBody
        /// </summary>
        public Rigidbody Rb => rb;

        /// <summary>
        /// Configurations of wheel
        /// </summary>
        /// <value></value>
        public MMV_WheelSettings Settings { get => settings; set => settings = value; }

        /// <summary>
        /// Max angle maximum angle the wheel can turn
        /// </summary>
        /// <value></value>
        public float MaxSteerAngle { get => maxSteerAngle; set => maxSteerAngle = value; }

        /// <summary>
        /// Whether the wheel will apply acceleration force
        /// </summary>
        /// <value></value>
        public bool ApplyAcceleration { get => applyAcceleration; set => applyAcceleration = value; }

        /// <summary>
        /// Whether the wheel will apply braking force
        /// </summary>
        /// <value></value>
        public bool ApplyBrake { get => applyBrake; set => applyBrake = value; }

        /// <summary>
        /// Global wheel position
        /// </summary>
        /// <returns></returns>
        public Vector3 WheelPosition => rb.transform.TransformPoint(colliderPosition);

        /// <summary>
        /// Forward direction of acceleration
        /// </summary>
        /// <returns></returns>
        public Vector3 WheelForward => Rb.transform.TransformDirection(Quaternion.Euler(0, currentSteering, 0) * Vector3.forward);

        /// <summary>
        /// Right side of the wheel (with steering)
        /// </summary>
        /// <returns></returns>
        public Vector3 WheelRight => Rb.transform.TransformDirection(Quaternion.Euler(0, currentSteering, 0) * Vector3.right);

        /// <summary>
        /// Up direction of suspension
        /// </summary>
        /// <returns></returns>
        public Vector3 WheelUp => Rb.transform.TransformDirection(Quaternion.Euler(Vector3.zero) * Vector3.up);

        private float CurrentDamperIntensity => Mathf.Clamp01(settings.DamperBySpringCompression.Evaluate(CurrentSpringLength / settings.SpringLength));

        private float CurrentDownForceIntensity => Mathf.Clamp01(settings.DownForce.Evaluate(CurrentSpringLength / settings.SpringLength));

        public MMV_Wheel()
        {
            maxSteerAngle = 0;
            applyAcceleration = true;
            applyBrake = true;
        }

        /// <summary>
        /// Create wheel
        /// </summary>
        /// <param name="owner">
        /// Wheel owner vehicle
        /// </param>
        public void SetupWheel(MMV_WheelManager wheelManager, Rigidbody rb)
        {
            this.rb = rb;

            if (bone)
            {
                offsetBone = bone.localPosition;
            }

            if (mesh)
            {
                colliderPosition = rb.transform.InverseTransformPoint(mesh.position);
            }
        }



        /// <summary>
        /// Apply wheel physics and pass the throttle and brake commands
        /// </summary>
        /// <param name="accelerationInput">
        /// Vertical input clamped -1 to 1
        /// <param name="brakeInput">
        /// Brake input force clamped -1 to 1
        /// /<param>
        public void UseWheel(float accelerationInput, float steeringInput, float brakeInput)
        {
            if (!Rb || !Mesh)
            {
                return;
            }

            if (!settings)
            {
                Debug.LogWarningFormat($"No {typeof(MMV_WheelSettings).ToString()} has been passed to the vehicle, physics cannot work like that. (Vehicle Rigidbody: {Rb.name})");
                return;
            }

            steeringInput *= MaxSteerAngle;
            if (!applyAcceleration) accelerationInput = 0;
            if (!applyBrake) brakeInput = 0;

            this.currentAcceleration = accelerationInput;
            this.currentSteering = steeringInput;
            this.currentBrake = brakeInput;

            var _vehicleVelocity = Rb.transform.InverseTransformDirection(Rb.velocity);
            var _springPosition = WheelPosition + (Rb.transform.up * settings.SpringHeight);   // when start raycast of suspension

            // springHeight is added so that the initial position of the suspension 
            // raycast does not get out of the vehicle collider, just set the value.

            var _maxRayDist = settings.SpringHeight + settings.WheelRadius + settings.SpringLength;

            if (Physics.Raycast(_springPosition, -Rb.transform.up * settings.SpringHeight, out wheelHit, _maxRayDist))
            {
                //---get current spring length

                lastSpringLength = CurrentSpringLength;
                CurrentSpringLength = wheelHit.distance - settings.WheelRadius - settings.SpringHeight;

                // prevents the distance from the ground from being negative

                if (CurrentSpringLength < 0)
                {
                    CurrentSpringLength = 0;
                }

                //---get suspension force

                var _springStiffness = settings.SpringStiffness * settings.SpringForceByCompression.Evaluate(CurrentSpringLength / settings.SpringLength);
                CurrentSpringForce = _springStiffness * (settings.SpringLength - CurrentSpringLength);
                CompressVelocity = (lastSpringLength - CurrentSpringLength) / Time.fixedDeltaTime;
                CurrentDamperForce = settings.SpringDamper * CompressVelocity * CurrentDamperIntensity;
                CurrentSuspensionForce = CurrentSpringForce + CurrentDamperForce;

                //---get wheel friction

                var _currentSideFriction = Mathf.Clamp(LocalVelocity.x, -settings.SideFriction, settings.SideFriction);
                var _currentForwardFriction = Mathf.Clamp(LocalVelocity.z, -settings.ForwardFriction, settings.ForwardFriction);

                // prevents friction when vehicle is accelerating
                _currentForwardFriction -= Mathf.Clamp(accelerationInput, -Mathf.Abs(_currentForwardFriction), Mathf.Abs(_currentForwardFriction));

                //-----------------------

                var _sideStiffness = CurrentSpringForce * _currentSideFriction;
                var _longitudinalStiffness = CurrentSpringForce * _currentForwardFriction;

                var _upForce = Vector3.up * (CurrentSuspensionForce + CurrentDamperForce);
                var _sideForce = -WheelRight * _sideStiffness;
                var _forwardForce = -WheelForward * _longitudinalStiffness;

                var _forwardAcc = WheelForward * accelerationInput;
                var _forwardBrake = WheelForward * brakeInput * Mathf.Clamp(-LocalVelocity.z, -1, 1);
                var _directionForce = _forwardAcc + _forwardBrake;

                var _downForce = -Vector3.up * CurrentDownForceIntensity * settings.MaxDownForce * Time.fixedDeltaTime;

                var _totalForce = _upForce + _downForce + _sideForce + _forwardForce + _directionForce;

                Rb.AddForceAtPosition(_totalForce, wheelHit.point);


                // --- apply position in bone and mesh
                var _wheelPos = WheelPosition + (-Rb.transform.up * (wheelHit.distance - settings.SpringHeight - settings.WheelRadius));

                if (bone)
                {
                    var _bonePos = Rb.transform.InverseTransformPoint(bone.position);
                    var _hitPos = Rb.transform.InverseTransformPoint(wheelHit.point);

                    _bonePos.y = _hitPos.y;

                    bone.position = Rb.transform.TransformPoint(_bonePos);
                }

                if (mesh)
                {
                    mesh.position = _wheelPos;
                }
            }

            else
            {
                CurrentSpringLength = Settings.SpringLength;

                var _newWheelPos = WheelPosition + (-Rb.transform.up * settings.SpringLength);

                if (mesh)
                {
                    mesh.position = _newWheelPos;
                }

                if (bone)
                {
                    var _currentBonePos = Rb.transform.InverseTransformPoint(bone.position);
                    var _newBonePos = _newWheelPos - (Rb.transform.up * settings.WheelRadius);

                    _newBonePos = Rb.transform.InverseTransformPoint(_newBonePos);
                    _newBonePos.x = _currentBonePos.x;
                    _newBonePos.z = _currentBonePos.z;
                    bone.position = Rb.transform.TransformPoint(_newBonePos);
                }
            }

            wheelMoveSpeed = CurrentWheelMoveSpeed(wheelMoveSpeed);

            if (mesh)
            {
                RotateWheel(steeringInput);
            }
        }

        // wheel move speed (meters per second) in local space
        private Vector3 CurrentWheelMoveSpeed(Vector3 current)
        {
            var _velocity = current;

            if (OnGronded)
            {
                var _rot = mesh.localRotation;
                mesh.localRotation = Quaternion.Euler(0, currentSteering, 0);
                _velocity = mesh.transform.InverseTransformDirection(Rb.GetPointVelocity(wheelHit.point));
                mesh.localRotation = _rot;
            }
            else
            {
                _velocity -= _velocity * Time.deltaTime;
            }
            return new Vector3(_velocity.x, 0, _velocity.z);
        }

        private void RotateWheel(float steerAngle)
        {
            var _velocity = LocalVelocity.z / settings.WheelRadius;
            currentWheelRot += _velocity;

            mesh.localRotation = MeshApplySteerRotation ? Quaternion.Euler(Vector3.up * steerAngle) : Quaternion.identity;
            mesh.localRotation *= MeshApplyAccelerationRotation ? Quaternion.Euler(currentWheelRot, 0, 0) : Quaternion.identity;

        }
    }
}
