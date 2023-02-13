using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Control Wheels particles emissions
    /// </summary>
    public class MMV_WheelsEffects
    {
        /// <summary>
        /// Calculates the amount of dust particles that the wheel must emit
        /// </summary>
        /// <param name="wheel">The wheel that should emit particles</param>
        /// <returns></returns>
        public float GetWheelDustParticleEmissionRate(MMV_Wheel wheel)
        {
            var _emission = wheel.OnGronded ? 1f : 0f;
            _emission *= wheel.LocalVelocity.magnitude;
            return _emission;
        }

        /// <summary>
        /// Calculates the amount of dust particles that a group of wheels must emit
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="dust"></param>
        public float GetWheelsDustParticleEmissionRate(MMV_Wheel[] wheels)
        {
            var _emission = 1f;
            var _wheelsOnGrounded = false;
            var _higherWheelVelocity = 0f;

            foreach (var w in wheels)
            {
                if (w.OnGronded) _wheelsOnGrounded = true;
                _higherWheelVelocity = Mathf.Max(_higherWheelVelocity, w.LocalVelocity.magnitude);
            }

            if (!_wheelsOnGrounded)
            {
                _emission = 0f;
            }

            _emission *= _higherWheelVelocity;

            return _emission;
        }


        /// <summary>
        /// Applies the emission of dust particles on a given wheel
        /// </summary>
        /// <param name="dust">The dust particle to be controlled</param>
        /// <param name="maxEmission">The intensity of particles that must be created</param>
        public void ControlWheelDustParticleEmission(MMV_Wheel wheel, ParticleSystem dust, float maxEmission)
        {
            var _emissionModule = dust.emission;
            _emissionModule.rateOverTime = GetWheelDustParticleEmissionRate(wheel) * maxEmission;
        }

        /// <summary>
        /// Controls the emission of dust particles to a group of wheels
        /// </summary>
        /// <param name="wheels">The wheel group</param>
        /// <param name="dust">The dust particle to be controlled</param>
        /// <param name="maxEmission">The intensity of particles that must be created</param>
        public void ControlWheelsDustParticleEmission(MMV_Wheel[] wheels, ParticleSystem dust, float maxEmission)
        {
            var _emissionModule = dust.emission;
            _emissionModule.rateOverTime = GetWheelsDustParticleEmissionRate(wheels) * maxEmission;
        }
    }
}
