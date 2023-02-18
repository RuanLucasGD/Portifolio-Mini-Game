using System.Collections.Generic;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Receives vehicle inputs and applies wheel physics and control
    /// </summary>
    [System.Serializable]
    public class MMV_WheeledWheelManager : MMV_WheelManager
    {
        /// <summary>
        /// Controls the spawn of wheel particles
        /// </summary>
        [System.Serializable]
        public class WheelsParticles
        {
            public const int MIN_PARTICLES_EMISSION = 1;
            public const int MAX_PARTICLES_EMISSION = 10;

            [SerializeField] private float emissionIntensity;

            [SerializeField] private List<ParticleSystem> leftWheelsParticles;
            [SerializeField] private List<ParticleSystem> rightWheelsParticles;

            /// <summary>
            /// The intensity of particles that must be created
            /// </summary>
            /// <value></value>
            public float EmissionIntensity { get => emissionIntensity; set => emissionIntensity = value; }

            /// <summary>
            /// Left wheel particles
            /// </summary>
            /// <value></value>
            public List<ParticleSystem> LeftWheelsParticles { get => leftWheelsParticles; set => leftWheelsParticles = value; }

            /// <summary>
            /// Right wheel particles
            /// </summary>
            /// <value></value>
            public List<ParticleSystem> RightWheelsParticles { get => rightWheelsParticles; set => rightWheelsParticles = value; }

            public WheelsParticles()
            {
                emissionIntensity = 1;
            }

            public void SetupParticle()
            {
                foreach (var p in leftWheelsParticles)
                {
                    var _mainModule = p.main;
                    _mainModule.loop = true;
                }

                foreach (var p in rightWheelsParticles)
                {
                    var _mainModule = p.main;
                    _mainModule.loop = true;
                }
            }

            public void UseParticles(MMV_Wheel[] leftWheels, MMV_Wheel[] rightWheels)
            {
                var _effect = new MMV_WheelsEffects();

                var _leftWheelsLength = leftWheels.Length;
                var _rightWheelsLength = rightWheels.Length;

                for (int i = 0; i < _leftWheelsLength; i++)
                {
                    if (leftWheelsParticles[i])
                    {
                        _effect.ControlWheelDustParticleEmission(leftWheels[i], leftWheelsParticles[i], EmissionIntensity);
                    }
                }
                for (int i = 0; i < rightWheels.Length; i++)
                {
                    if (rightWheelsParticles[i])
                    {
                        _effect.ControlWheelDustParticleEmission(rightWheels[i], rightWheelsParticles[i], EmissionIntensity);
                    }
                }
            }
        }

        [SerializeField] private WheelsParticles particles;

        public new MMV_WheeledVehicle Vehicle => (MMV_WheeledVehicle)base.Vehicle;

        public WheelsParticles Particles { get => particles; set => particles = value; }

        public MMV_WheeledWheelManager()
        {
            WheelsLeft = new MMV_Wheel[] { };
            WheelsRight = new MMV_Wheel[] { };
        }

        public override void SetupWheels(MMV_Vehicle vehicle)
        {
            base.SetupWheels(vehicle);

            foreach (var w in WheelsLeft) Apply(w);
            foreach (var w in WheelsRight) Apply(w);

            void Apply(MMV_Wheel w)
            {
                w.SetupWheel(this, vehicle.Rb);
                w.Settings = Settings;
                w.MeshApplyAccelerationRotation = true;
                w.MeshApplySteerRotation = true;
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
            var _brake = _engine.CurrentBrakeForce;
            var _steering = Vehicle.HorizontalInput;
            var _steerByVelocity = SteerByVelocity(Settings, _engine);

            _steering *= _steerByVelocity;

            if (!Vehicle.VehicleControlsEnabled)
            {
                _acc = 0;
                _steering = 0;
            }

            foreach (var w in WheelsLeft) w.UseWheel(_acc, _steering, _brake);
            foreach (var w in WheelsRight) w.UseWheel(_acc, _steering, _brake);

            Particles.UseParticles(WheelsLeft, WheelsRight);
        }
    }
}