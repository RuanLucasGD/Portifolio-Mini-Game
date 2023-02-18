using System.Collections.Generic;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// All vehicle engine properties
    /// </summary>
    [CreateAssetMenu(fileName = "Engine Settings", menuName = "MMV/Engine Settings", order = 0)]
    public class MMV_EngineSettings : ScriptableObject
    {
        /// <summary>
        /// Maximum brake force that the vehicle can be configured
        /// </summary>
        public const float MAX_BRAKE_FORCE = 10000.0f;

        /// <summary>
        /// Minimum brake force that the vehicle can be configured
        /// </summary>
        public const float MIN_BRAKE_FORCE = 1.0f;

        /// <summary>
        /// Minimum acceleration speed that the engine can have
        /// </summary>
        public const float MIN_ACCELERATION_VELOCITY = 10.0f;

        /// <summary>
        /// Maximum acceleration that the engine can reach
        /// </summary>
        public const float MAX_ACCELERATION_FORCE = 10000.0f;

        /// <summary>
        /// Acceptable minimum throttle that can be passed as maximum throttle for the engine 
        /// </summary>
        public const float MIN_ACCELERATION_FORCE = 100.0f;

        /// <summary>
        ///  Minimum speed that can be set for the vehicle
        /// </summary>
        public const float MIN_SPEED = 2.0f;

        /// <summary>
        /// Maximum speed that can be set on the vehicle
        /// </summary>
        public const float MAX_SPEED = 350.0f;

        /// <summary>
        /// Minimum amount of gears that the engine can have
        /// </summary>
        public const int MIN_GEARS_AMOUNT = 1;

        /// <summary>
        /// Maximum amount of gears that the engine can have
        /// </summary>
        public const int MAX_GEARS_AMOUNT_FORWARD = 10;

        /// <summary>
        ///  Minimum engine deceleration force
        /// </summary>
        public const float MIN_SLOWDOWN = 1;

        [SerializeField] private float maxAcceleration;
        [SerializeField] private AnimationCurve accelerationCurve;
        [SerializeField] private float slowdown;
        [SerializeField] private AnimationCurve slowdownByVelocityCurve;

        [Space]

        [SerializeField] private float maxForwardVelocity;
        [SerializeField] private float maxReverseVelocity;
        [SerializeField] private float maxBrakeForce;

        [Space]

        [SerializeField] private float[] forwardGears;
        [SerializeField] private float[] reverseGears;

        /// <summary>
        /// The engine deceleration speed
        /// </summary>
        /// <value></value>
        public float Slowdown
        {
            get => slowdown;
            set => slowdown = Mathf.Max(value, MIN_SLOWDOWN);
        }

        /// <summary>
        /// Control vehicle deceleration force according to current speed
        /// </summary>
        /// <value></value>
        public AnimationCurve SlowdownByVelocityCurve
        {
            get => slowdownByVelocityCurve;
            set { slowdownByVelocityCurve = value; ValidadeSlowdownByVelocity(); }
        }

        /// <summary>
        /// <para>The acceleration curve from 0 to 1.</para>
        /// <para>The higher the curve value, the stronger the vehicle acceleration.</para>
        /// <para>Turn time equals vehicle speed.</para>
        /// </summary>
        /// <value></value>
        public AnimationCurve AccelerationCurve
        {
            get => accelerationCurve;
            set { accelerationCurve = value; ValidadeAccelerationCurve(); }
        }

        /// <summary>
        /// The maximum speed the vehicle can reach
        /// </summary>
        /// <value></value>
        public float MaxAcceleration { get => maxAcceleration; set => maxAcceleration = value; }

        /// <summary>
        /// Max move speed to forward in KM/H
        /// </summary>
        /// <value></value>
        public float MaxForwardVelocity
        {
            get => maxForwardVelocity;
            set => maxForwardVelocity = Mathf.Clamp(value, MIN_SPEED, MAX_SPEED);
        }

        /// <summary>
        /// Max move speed to backward in KM/H
        /// </summary>
        /// <value></value>
        public float MaxReverseVelocity
        {
            get => maxReverseVelocity;
            set => maxReverseVelocity = Mathf.Clamp(value, MIN_SPEED, MAX_SPEED);
        }

        /// <summary>
        /// Force of brake
        /// </summary>
        /// <value></value>
        public float MaxBrakeForce
        {
            get => maxBrakeForce;
            set => maxBrakeForce = Mathf.Clamp(value, MIN_BRAKE_FORCE, MAX_BRAKE_FORCE);
        }

        /// <summary>
        /// Limit speeds in KM/H for gear shifting to forward
        /// </summary>
        /// <value></value>
        public float[] ForwardGears
        {
            get => forwardGears;
            set => forwardGears = value;
        }

        /// <summary>
        /// Limit speeds in KM/H for gear shifting to backward 
        /// </summary>
        /// <value></value>
        public float[] ReverseGears { get => reverseGears; set => reverseGears = value; }

        public MMV_EngineSettings()
        {
            MaxAcceleration = 3000;
            Slowdown = 1000;
            MaxBrakeForce = 3000;

            MaxForwardVelocity = 80;
            MaxReverseVelocity = 40;

            GenerateForwardGears(4);
            GenerateReverseGears(1);

            OnValidate();
        }

        private float[] GenerateGearsArray(int amount, float maxSpeed)
        {
            amount = Mathf.Clamp(amount, MIN_GEARS_AMOUNT, MAX_GEARS_AMOUNT_FORWARD);
            var _gears = new List<float>();

            // the engine must never be less than one gear
            if (amount <= 0)
            {
                _gears.Add(maxSpeed);
            }
            else
            {
                for (int i = 1; i <= amount; i++)
                {
                    // adding a gear with default value
                    _gears.Add((maxSpeed / amount) * i);
                }
            }

            return _gears.ToArray();
        }

        /// <summary>
        /// Generates a list of engine gears
        /// </summary>
        /// <param name="amount"></param>
        public void GenerateForwardGears(int amount)
        {
            forwardGears = GenerateGearsArray(amount, MaxForwardVelocity);
        }

        /// <summary>
        /// Generates a list of reverse engine gears
        /// </summary>
        /// <param name="amount"></param>
        public void GenerateReverseGears(int amount)
        {
            reverseGears = GenerateGearsArray(amount, MaxReverseVelocity);
        }

        private void OnValidate()
        {
            ValidadeSlowdownByVelocity();
            ValidadeAccelerationCurve();
        }

        private void ValidadeSlowdownByVelocity()
        {
            if (slowdownByVelocityCurve == null)
            {
                slowdownByVelocityCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 0, 0),
                                                             new Keyframe(0.1f, 0.0f, 0, 0),
                                                             new Keyframe(0.2f, 1.0f, 0, 0),
                                                             new Keyframe(1.0f, 1.0f, 0, 0));
            }

            if (slowdownByVelocityCurve.length < 1)
            {
                slowdownByVelocityCurve.AddKey(new Keyframe(1, 1, 0, 0));
            }

            slowdownByVelocityCurve = MMV_Utils.ClampAnimationCurve(slowdownByVelocityCurve, 1, 1, 0, 0);
        }

        private void ValidadeAccelerationCurve()
        {
            if (accelerationCurve == null)
            {
                accelerationCurve = new AnimationCurve(new Keyframe(0.0f, 0.4f, 0, 0),
                                                       new Keyframe(0.1f, 1.0f, 0, 0),
                                                       new Keyframe(0.2f, 1.0f, 0, 0),
                                                       new Keyframe(1.0f, 0.2f, 0, 0));
            }

            if (accelerationCurve.keys.Length < 1)
            {
                accelerationCurve.AddKey(new Keyframe(0, 1));
            }

            accelerationCurve = MMV_Utils.ClampAnimationCurve(accelerationCurve, 1, 1, 0, 0);
        }
    }
}

