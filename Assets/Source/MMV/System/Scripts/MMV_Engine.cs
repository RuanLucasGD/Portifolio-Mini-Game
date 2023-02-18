using System;
using UnityEngine;
using System.Collections.Generic;

namespace MMV
{
    /// <summary>
    /// <para>Responsible for handling MBT vehicle engine data.</para>
    /// <para>MBT_Engine delivery:</para>
    /// <para>* Accelerating force of the two tracks</para>
    /// <para>* Engine gearing system</para>
    /// <para>* Engine sound</para>
    /// <para>* Track braking system</para>
    /// </summary>
    [Serializable]
    public class MMV_Engine
    {
        /// <summary>
        /// Manages engine sound using throttle and gearshift parameters
        /// </summary>
        [Serializable]
        public class SoundSystem
        {
            [SerializeField] private AudioSource audioPlayer;
            [SerializeField] private AudioClip engineSound;

            [SerializeField] private float basePitch;
            [SerializeField] private float maxPitch;

            //------------------------------------------------------

            // lerp velocity of change gear (used on engine sound)
            private const float TRANSMISSION_CHANGE_SPEED = 15.0f;

            private float currentSoundPitch;

            //------------------------------------------------------

            /// <summary>
            /// The AudioSource of the engine
            /// </summary>
            /// <value></value>
            public AudioSource AudioPlayer { get => audioPlayer; set => audioPlayer = value; }

            /// <summary>
            /// Min pitch of the engine sound
            /// </summary>
            /// <value></value>
            public float BasePitch { get => basePitch; set => basePitch = value; }

            /// <summary>
            /// Max engine sound pitch 
            /// </summary>
            /// <value></value>
            public float MaxPitch { get => maxPitch; set => maxPitch = value; }

            /// <summary>
            /// Current engine sound acceleration pitch
            /// </summary>
            public float CurrentPitch => currentSoundPitch;

            /// <summary>
            /// Sound of the engine
            /// </summary>
            /// <value></value>
            public AudioClip Sound { get => engineSound; set => engineSound = value; }

            public SoundSystem()
            {
                basePitch = 0.9f;
                maxPitch = 5f;
            }

            /// <summary>
            /// Apply engine acceleration sound
            /// </summary>
            /// <param name="engine">the vehicle engine</param>
            /// <param name="velocity">current vehicle velocity in KM/H</param>
            public void UseEngineSound(MMV_Engine engine, Vector3 velocity)
            {
                if (!audioPlayer)
                {
                    return;
                }

                if (!engine.EngineSettings)
                {
                    Debug.LogWarningFormat($"No Engine Configuration passed to Whole Vehicle {engine.vehicle.name}");
                    return;
                }

                velocity = Vector3.ClampMagnitude(velocity, engine.CurrentMaxVelocityByDirection - 1);

                var _forwardGears = engine.engineSettings.ForwardGears;
                var _reverseGears = engine.engineSettings.ReverseGears;

                var _isTurningStoped = engine.IsTurningStoped;
                var _wheelsSpeed = velocity.magnitude;
                var _gears = velocity.z >= 0 || _isTurningStoped ? _forwardGears : _reverseGears;
                var _currentGear = Mathf.Clamp(Mathf.Abs(engine.CurrentGear), 0, _gears.Length - 1);
                var _maxSpeedPerGear = _gears[_currentGear];
                var _accelerationPitch = basePitch + ((velocity.magnitude / _maxSpeedPerGear) * (MaxPitch - basePitch));
                _accelerationPitch = Mathf.Clamp(_accelerationPitch, BasePitch, MaxPitch);

                // smooth gear shift
                currentSoundPitch = Mathf.Lerp(currentSoundPitch, _accelerationPitch, Time.deltaTime * TRANSMISSION_CHANGE_SPEED);

                audioPlayer.pitch = currentSoundPitch;

                if (!audioPlayer.isPlaying)
                {
                    audioPlayer.Play();
                }
            }
        }

        /// <summary>
        /// Minimum rotation speed that the vehicle can have
        /// </summary>
        public const float MIN_ROTATION_VELOCITY = 0.3f;

        /// <summary>
        /// Minimum pitch that the engine sound can have
        /// </summary>
        public const float MIN_SOUND_PITCH = 0.1f;

        /// <summary>
        /// Maximum pitch that the engine sound can have
        /// </summary>
        public const float MAX_SOUND_PITCH = 10.0f;

        [SerializeField] private MMV_EngineSettings engineSettings;
        [SerializeField] private SoundSystem engineSound;
        [SerializeField] private float decelerationByAngle;
        [SerializeField] private AnimationCurve angleDecelerationByAngleCurve;

        private MMV_Vehicle vehicle;

        private float currentAccelerationForce;
        private int currentGear;

        // controller inputs
        protected float vertical;
        protected float horizontal;
        protected bool isBraking;

        //------------------------------------------------------

        /// <summary>
        /// Owner of the engine
        /// </summary>
        /// <value></value>
        public MMV_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        /// <summary>
        /// Engine power and acceleration setting
        /// </summary>
        /// <value></value>
        public MMV_EngineSettings EngineSettings { get => engineSettings; set => engineSettings = value; }

        /// <summary>
        /// Slows down the vehicle on steep slopes
        /// </summary>
        /// <returns></returns>
        public float DecelerationByAngle { set => decelerationByAngle = Mathf.Abs(value); get => decelerationByAngle; }

        /// <summary>
        /// Engine sound management
        /// </summary>
        /// <value></value>
        public SoundSystem EngineSound { get => engineSound; set => engineSound = value; }

        /// <summary>
        /// The current engine gear
        /// </summary>
        public int CurrentGear => currentGear;

        /// <summary>
        /// Setup engine on vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        public void SetupEngine(MMV_Vehicle vehicle)
        {
            this.vehicle = vehicle;

            if (engineSound.AudioPlayer)
            {
                if (!engineSound.AudioPlayer.loop) engineSound.AudioPlayer.loop = true;
                if (!engineSound.AudioPlayer.isPlaying) engineSound.AudioPlayer.Play();

                engineSound.AudioPlayer.clip = engineSound.Sound;
            }
        }

        /// <summary>
        /// The velocity in KH/H when the vehicle is moving to forward is different 
        /// of the velocity moving to backward
        /// </summary>
        public float CurrentMaxVelocityByDirection => vehicle.VelocityKMH >= 0 ? engineSettings.MaxForwardVelocity : engineSettings.MaxReverseVelocity;

        /// <summary>
        /// Returns true if the vehicle is stationary and turning right or left
        /// </summary>
        /// <returns></returns>
        public bool IsTurningStoped => Mathf.Abs(horizontal) > 0.1f && vertical == 0;

        /// <summary>
        /// Returns forward if velocity is opposite to acceleration.
        /// Ex:
        /// Accelerating forward but the vehicle is going backwards.
        /// </summary>
        /// <returns></returns>
        public bool IsReversingAcceleration => (vertical < 0 && (int)vehicle.VelocityKMH > 0) || (vertical > 0 && (int)vehicle.VelocityKMH < 0);

        public AnimationCurve AngleDecelerationByAngleCurve
        {
            get => angleDecelerationByAngleCurve;
            set
            {
                angleDecelerationByAngleCurve = value;
                ValidadeDecelerationBySlopAngleCurve();
            }
        }

        public MMV_Engine()
        {
            EngineSound = new SoundSystem();
            DecelerationByAngle = 50000;

            ValidadeDecelerationBySlopAngleCurve();
        }

        public virtual void Update()
        {
            if (!EngineSettings)
            {
                Debug.LogWarningFormat($"No Engine Configuration passed to Whole Vehicle {vehicle.name}");
                return;
            }

            this.vertical = vehicle.VerticalInput;
            this.horizontal = Vehicle.HorizontalInput;
            this.isBraking = Vehicle.IsBraking;
            currentGear = GetCurrentGear();
        }

        public virtual void FixedUpdate()
        {
            if (!EngineSettings)
            {
                Debug.LogWarningFormat($"No Engine Configuration passed to Whole Vehicle {vehicle.name}");
                return;
            }
        }

        /// <summary>
        /// Calculates current engine acceleration force
        /// </summary>
        /// <param name="currentSpeed">
        /// The speed of movement of the vehicle in KM/H
        /// </param>
        /// <param name="accelerationForce">
        /// The current acceleration force
        /// </param>
        protected virtual void GetCurrentAccelerationForce(float currentSpeed, out float accelerationForce)
        {
            if (!EngineSettings)
            {
                Debug.LogWarningFormat($"No Engine Configuration passed to Whole Vehicle {vehicle.name}");
                accelerationForce = 0;
                currentSpeed = 0;
                return;
            }

            if (Mathf.Abs(currentSpeed) > CurrentMaxVelocityByDirection)
            {
                accelerationForce = 0f;
                return;
            }

            currentSpeed = Mathf.Clamp(currentSpeed, -EngineSettings.MaxReverseVelocity, EngineSettings.MaxForwardVelocity);

            var _accelerationCurve = EngineSettings.AccelerationCurve;
            var _maxAcceleration = EngineSettings.MaxAcceleration;
            var _relativeSpeed = Mathf.Abs(currentSpeed) / CurrentMaxVelocityByDirection;
            var _engineForce = _accelerationCurve.Evaluate(_relativeSpeed) * _maxAcceleration;

            accelerationForce = _engineForce;
        }

        /// <summary>
        /// Current wheel braking force
        /// </summary>
        /// <param name="isBraking">
        /// If it is for the vehicle to brake
        /// </param>
        /// <param name="brakeForce">
        /// braking force
        /// </param>
        protected virtual void GetCurrentBrakeForce(bool isBraking, out float brakeForce)
        {
            brakeForce = 0f;

            if (!EngineSettings)
            {
                Debug.LogWarningFormat($"No Engine Configuration passed to Whole Vehicle {vehicle.name}");
                brakeForce = 0;
                return;
            }

            if (isBraking || IsReversingAcceleration)
            {
                brakeForce = EngineSettings.MaxBrakeForce;
            }
            else
            {
                if (!Vehicle.IsAccelerating)
                {
                    brakeForce = EngineSettings.Slowdown;
                }
            }
        }

        private int GetCurrentGear()
        {
            var _speedKMH = vehicle.VelocityKMH;
            var _forwardGears = EngineSettings.ForwardGears;
            var _reverseGears = engineSettings.ReverseGears;

            // if is moving to forward (1) else (-1)
            var _gear = 0;

            if (Mathf.Round(_speedKMH) >= 0)   // change transmission moving forward
            {
                for (int i = 0; i <= _forwardGears.Length - 1; i++)
                {
                    if (_speedKMH > _forwardGears[i])
                    {
                        _gear = i + 1;
                        _gear = Mathf.Clamp(_gear, 0, _forwardGears.Length);
                    }
                }
            }
            else                    // change transmission moving backward
            {
                for (int i = 1; i <= _reverseGears.Length; i++)
                {
                    if (_speedKMH < -_reverseGears[i - 1])
                    {
                        _gear = -i - 1;
                        _gear = Mathf.Clamp(_gear, -_reverseGears.Length, 0);
                    }
                }
            }

            if (vertical != 0 || horizontal != 0)
            {
                if (_gear == 0) _gear = _speedKMH >= 0 ? 1 : -1;
            }

            return _gear;
        }

        // The more incriminating the terrain on which the vehicle travels, the greater the vehicle's 
        // reverse force, which prevents it from continuing to climb
        public void DecelerationBySlopeAngle(Rigidbody rb)
        {
            var _desacelerationDirecton = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(vehicle.transform.up, Vector3.up), Vehicle.transform.up);
            var _currentAngle = Vector3.Angle(vehicle.transform.up, Vector3.up);
            _currentAngle = Mathf.Clamp(_currentAngle, 0, 90);

            var _decelerationForce = _desacelerationDirecton * AngleDecelerationByAngleCurve.Evaluate(_currentAngle / 90) * decelerationByAngle;

            if (Mathf.Abs(vehicle.VelocityKMH) > CurrentMaxVelocityByDirection)
            {
                _decelerationForce = Vector3.zero;
            }

            rb.AddForce(_decelerationForce);
        }

        private void ValidadeDecelerationBySlopAngleCurve()
        {
            if (angleDecelerationByAngleCurve == null)
            {
                angleDecelerationByAngleCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 0, 0),
                                                                   new Keyframe(0.2f, 0.0f, 0, 0),
                                                                   new Keyframe(1.0f, 1.0f, 0, 0));
            }

            if (angleDecelerationByAngleCurve.length < 1)
            {
                angleDecelerationByAngleCurve.AddKey(0, 0);
            }

            angleDecelerationByAngleCurve.keys[0] = new Keyframe(0, 0);

            angleDecelerationByAngleCurve = MMV_Utils.ClampAnimationCurve(angleDecelerationByAngleCurve, 1, 1, 0, 0);
        }

        /// <summary>
        /// convert speed from meters/second to kilometers/hour
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static float MsToKMH(float speed)
        {
            return speed * 3.6f;
        }

        /// <summary>
        /// convert speed from meters/second to kilometers/hour
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static Vector3 MsToKMH(Vector3 speed)
        {
            return speed * 3.6f;
        }
    }
}