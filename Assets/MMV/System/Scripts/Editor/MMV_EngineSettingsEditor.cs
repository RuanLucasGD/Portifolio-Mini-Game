using UnityEngine;
using UnityEditor;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_EngineSettings))]
    public class MMV_EngineSettingsEditor : UnityEditor.Editor
    {
        private MMV_EngineSettings engine;

        private int lastForwardGearsLenght;
        private int lastReverseGearsLenght;

        private void OnEnable()
        {
            engine = (MMV_EngineSettings)target;

            lastForwardGearsLenght = engine.ForwardGears.Length;
            lastReverseGearsLenght = engine.ReverseGears.Length;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            AccelerationField();
            GearsField();
        }

        private void AccelerationField()
        {
            engine.AccelerationCurve = MMV_Utils.ClampAnimationCurve(engine.AccelerationCurve, 1, 1, 0, 0);
            engine.MaxAcceleration = Mathf.Clamp(engine.MaxAcceleration, MMV_EngineSettings.MIN_ACCELERATION_FORCE, MMV_EngineSettings.MAX_ACCELERATION_FORCE);
            engine.Slowdown = Mathf.Clamp(engine.Slowdown, MMV_EngineSettings.MIN_SLOWDOWN, engine.MaxBrakeForce);

            engine.MaxForwardVelocity = Mathf.Clamp(engine.MaxForwardVelocity, MMV_EngineSettings.MIN_SPEED, MMV_EngineSettings.MAX_SPEED);
            engine.MaxReverseVelocity = Mathf.Clamp(engine.MaxReverseVelocity, MMV_EngineSettings.MIN_SPEED, MMV_EngineSettings.MAX_SPEED);
        }

        private void GearsField()
        {
            if (lastForwardGearsLenght != engine.ForwardGears.Length)
            {
                engine.GenerateForwardGears(engine.ForwardGears.Length);
                lastForwardGearsLenght = engine.ForwardGears.Length;
            }
            if (lastReverseGearsLenght != engine.ReverseGears.Length)
            {
                engine.GenerateReverseGears(engine.ReverseGears.Length);
                lastReverseGearsLenght = engine.ReverseGears.Length;
            }

            engine.ForwardGears = ClampGears(engine.ForwardGears, engine.MaxForwardVelocity);
            engine.ReverseGears = ClampGears(engine.ReverseGears, engine.MaxReverseVelocity);
        }

        private float[] ClampGears(float[] gears, float maxSpeed)
        {
            if (gears[0] < maxSpeed) gears[0] = Mathf.Max(gears[0], MMV_EngineSettings.MIN_SPEED);
            if (gears[gears.Length - 1] < maxSpeed) gears[gears.Length - 1] = maxSpeed;

            for (int i = 1; i < gears.Length - 1; i++)
            {
                if (gears[i] < maxSpeed)
                {
                    gears[i] = Mathf.Clamp(gears[i], gears[i - 1] + 0.1f, gears[i + 1] - 0.1f);
                }
            }

            return gears;
        }
    }
}
