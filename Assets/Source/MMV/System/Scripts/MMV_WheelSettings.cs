using UnityEngine;

namespace MMV
{
    /// <summary>
    /// All wheel behavior properties
    /// </summary>
    [CreateAssetMenu(fileName = "Wheel Settings", menuName = "MMV/Wheel Settings", order = 0)]
    public class MMV_WheelSettings : ScriptableObject
    {

        [SerializeField] private float wheelRadius;
        [SerializeField] private float springLength;
        [SerializeField] private float springHeight;
        [SerializeField] private LayerMask collisionLayer;
        [Space]
        [SerializeField] private int springStiffness;
        [SerializeField] private AnimationCurve springForceByCompression;
        [SerializeField] private int springDamper;
        [SerializeField] private AnimationCurve damperBySpringCompression;

        [Space]
        [SerializeField] private float forwardFriction;
        [SerializeField] private float sideFriction;
        [SerializeField] private float multiplyFriction;
        [Space]
        [SerializeField] private AnimationCurve steerByVelocityCurve;
        [Space]
        [SerializeField] private float maxDownForce;
        [SerializeField] private AnimationCurve downForceCurve;

        public const float MIN_RADIUS = 0.1f;
        public const float MAX_RADIUS = 5.0f;
        public const float MIN_SPRING_LENGTH = 0.05f;
        public const float MAX_SPRING_LENGTH = 5.0f;
        public const float MIN_SPRING_HEIGHT = 0.0f;
        public const float MAX_SPRING_HEIGHT = 1.0f;
        public const int MIN_SPRING_STIFFNESS = 100;
        public const int MAX_SPRING_STIFFNESS = 100000;
        public const int MIN_SPRING_DAMPER = 100;
        public const int MAX_SPRING_DAMPER = 100000;
        public const float MIN_FRICTION = 0.2f;
        public const float MAX_FRICTION = 5.0f;

        /// <summary>
        /// Radius of all wheels
        /// </summary>
        /// <value></value>
        public float WheelRadius
        {
            get => wheelRadius;
            set => wheelRadius = Mathf.Clamp(value, MIN_RADIUS, MAX_RADIUS);
        }

        /// <summary>
        /// Suspension height
        /// </summary>
        /// <value></value>
        public float SpringLength
        {
            get => springLength;
            set => springLength = Mathf.Clamp(value, MIN_SPRING_LENGTH, MAX_SPRING_LENGTH);
        }

        /// <summary>
        /// Resistense of spring
        /// </summary>
        public int SpringStiffness
        {
            get => springStiffness;
            set => springStiffness = Mathf.Clamp(value, MIN_SPRING_STIFFNESS, MAX_SPRING_STIFFNESS);
        }

        /// <summary>
        /// Suspension force based on ground distance
        /// </summary>
        /// <value></value>
        public AnimationCurve SpringForceByCompression { get => springForceByCompression; set { springForceByCompression = value; ValidadeSpringForceByCompressionCurve(); } }

        /// <summary>
        /// Suspension smooth force
        /// </summary>
        public int SpringDamper
        {
            get => springDamper;
            set => springDamper = Mathf.Clamp(value, MIN_SPRING_DAMPER, MAX_SPRING_DAMPER);
        }

        /// <summary>
        /// Start spring height
        /// </summary>
        public float SpringHeight
        {
            get => springHeight;
            set => springHeight = Mathf.Clamp(value, MIN_SPRING_HEIGHT, MAX_SPRING_HEIGHT);
        }

        /// <summary>
        /// Forward friction of wheels
        /// </summary>
        public float ForwardFriction
        {
            get => forwardFriction;
            set => forwardFriction = Mathf.Abs(value);
        }

        /// <summary>
        /// Modify front and side friction at the same time
        /// </summary>
        /// <value></value>
        public float MultiplyFriction
        {
            get => multiplyFriction;
            set => multiplyFriction = Mathf.Abs(value);
        }

        /// <summary>
        /// Side friction of wheels
        /// </summary>
        public float SideFriction
        {
            get => sideFriction;
            set => sideFriction = Mathf.Abs(value);
        }

        /// <summary>
        /// Steering angle intensity curve based on current vehicle speed
        /// </summary>
        /// <value></value>
        public AnimationCurve SteerByVelocityCurve
        {
            get => steerByVelocityCurve;
            set
            {
                steerByVelocityCurve = value;
                ValidateSteeringByVelocityCurve();
            }
        }

        /// <summary>
        /// Adjust suspension stability based on how compressed the spring
        /// </summary>
        /// <value></value>
        public AnimationCurve DamperBySpringCompression
        {
            get => damperBySpringCompression;
            set
            {
                damperBySpringCompression = value;
                ValidadeDamperBySpringCompressionCurve();
            }
        }

        /// <summary>
        /// Maximum downward force that will be applied to prevent the vehicle from leaving the ground
        /// </summary>
        /// <value></value>
        public float MaxDownForce { get => maxDownForce; set => maxDownForce = value; }

        /// <summary>
        /// Amount of downward force applied based on how much the spring is compressed
        /// </summary>
        /// <value></value>
        public AnimationCurve DownForce { get => downForceCurve; set => downForceCurve = value; }

        /// <summary>
        /// Collisions layer
        /// </summary>
        /// <value></value>
        public LayerMask CollisionLayer { get => collisionLayer; set => collisionLayer = value; }

        public MMV_WheelSettings()
        {
            SpringHeight = 0.2f;
            SpringLength = 0.3f;
            WheelRadius = 0.4f;
            SpringStiffness = 15000;
            SpringDamper = 2000;
            ForwardFriction = 0.2f;
            SideFriction = 1.0f;
            MultiplyFriction = 1.0f;
            MaxDownForce = 90000;

            OnValidate();
        }

        private void OnValidate()
        {
            ValidateSteeringByVelocityCurve();
            ValidadeDamperBySpringCompressionCurve();
            ValidadeSpringForceByCompressionCurve();
            ValidadeDownForceCurve();
        }

        private void ValidadeDamperBySpringCompressionCurve()
        {
            if (damperBySpringCompression == null)
            {
                damperBySpringCompression = new AnimationCurve(new Keyframe(0.0f, 1.0f),
                                                               new Keyframe(0.3f, 0.1f),
                                                               new Keyframe(1.0f, 0.1f));
            }

            if (damperBySpringCompression.keys.Length < 1)
            {
                damperBySpringCompression.AddKey(0, 1);
            }
            else
            {
                damperBySpringCompression.keys[0] = new Keyframe(0, 1);
            }

            damperBySpringCompression = MMV_Utils.ClampAnimationCurve(damperBySpringCompression, 1, 1, 0, 0);
        }

        private void ValidadeSpringForceByCompressionCurve()
        {
            if (springForceByCompression == null)
            {
                springForceByCompression = new AnimationCurve(new Keyframe(0.0f, 1.0f),
                                                              new Keyframe(0.4f, 1.0f),
                                                              new Keyframe(0.7f, 0.2f),
                                                              new Keyframe(1.0f, 0.2f));
            }

            if (springForceByCompression.keys.Length < 1)
            {
                springForceByCompression.AddKey(0, 1);
            }

            if (springForceByCompression.keys[0].value != 1)
            {
                springForceByCompression.RemoveKey(0);
                springForceByCompression.AddKey(0, 1);
            }

            springForceByCompression = MMV_Utils.ClampAnimationCurve(springForceByCompression, 1, 1, 0, 0);
        }

        private void ValidateSteeringByVelocityCurve()
        {
            if (steerByVelocityCurve == null)
            {
                steerByVelocityCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
                                                          new Keyframe(0.1f, 1, 0, 0),
                                                          new Keyframe(0.5f, 0.3f, -1, -1),
                                                          new Keyframe(1, 0.1f, 0, 0));
            }
            else if (steerByVelocityCurve.keys.Length < 1)
            {
                steerByVelocityCurve.AddKey(0, 1);
            }
            else if (steerByVelocityCurve.keys[0].value != 1f)
            {
                var _time = steerByVelocityCurve.keys[0].time;
                steerByVelocityCurve.RemoveKey(0);
                steerByVelocityCurve.AddKey(_time, 1);
            }

            steerByVelocityCurve = MMV_Utils.ClampAnimationCurve(steerByVelocityCurve, 1, 1, 0, 0);
        }

        private void ValidadeDownForceCurve()
        {
            if (downForceCurve == null)
            {
                downForceCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            }

            if (downForceCurve.keys.Length < 1)
            {
                downForceCurve.AddKey(0, 0);
            }

            if (downForceCurve.keys.Length < 2)
            {
                downForceCurve.AddKey(1, 1);
            }

            if (downForceCurve.keys[0].value != 0f)
            {
                var _time = downForceCurve.keys[0].time;
                downForceCurve.RemoveKey(0);
                downForceCurve.AddKey(_time, 0);
            }

            if (downForceCurve.keys[downForceCurve.length - 1].value != 1f)
            {
                var _time = downForceCurve.keys[downForceCurve.length - 1].time;
                downForceCurve.RemoveKey(downForceCurve.length - 1);
                downForceCurve.AddKey(_time, 1);
            }

            downForceCurve = MMV_Utils.ClampAnimationCurve(downForceCurve, 1, 1, 0, 0);
        }
    }
}
