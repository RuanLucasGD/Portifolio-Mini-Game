using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MMV
{
    [System.Serializable] public class BoolEvent : UnityEvent<bool> { }
    [System.Serializable] public class FloatEvent : UnityEvent<float> { }

    public class MMV_Utils
    {
        public static AnimationCurve ClampAnimationCurve(AnimationCurve curve, float maxX, float maxY, float minX, float minY)
        {
            var newKeyframes = new List<Keyframe>();

            for (int i = 0; i < curve.length; i++)
            {
                var k = curve[i];

                k.time = Mathf.Clamp(k.time, minX, maxX);
                k.value = Mathf.Clamp(k.value, minY, maxY);

                newKeyframes.Add(k);
            }

            curve = new AnimationCurve(newKeyframes.ToArray());

            return curve;
        }
    }
}