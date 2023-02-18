using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Generate camera shake configuration asset file
    /// </summary>
    [CreateAssetMenu(fileName = "Camera Shake Settings", menuName = "MMV/Camera Shake", order = 0)]
    public class MMV_CameraShakeSettings : ScriptableObject
    {
        [Header("General")]
        [SerializeField, Min(0.1f)] private float length;
        [SerializeField, Min(0)] private float shakeVelocity;
        [SerializeField] private AnimationCurve shakeForce;

        [Header("Rotation")]
        [SerializeField, Min(0)] private float maxRotation;

        [Header("Offset Position")]
        [SerializeField, Min(0)] private float maxOffsetX;
        [SerializeField, Min(0)] private float maxOffsetY;
        [SerializeField, Min(0)] private float maxOffsetZ;

        /// <summary>
        /// Life time of camera shake effect
        /// </summary>
        /// <value></value>
        public float Length { get => length; set => length = value; }

        /// <summary>
        /// Speed of shake effect
        /// </summary>
        /// <value></value>
        public float ShakeVelocity { get => shakeVelocity; set => shakeVelocity = value; }

        /// <summary>
        /// Range of shake movement/rotation based on life time of the effect
        /// </summary>
        /// <value></value>
        public AnimationCurve ShakeForce { get => shakeForce; set { shakeForce = value; ValidadeCameraShakeForce(); } }

        /// <summary>
        /// Max Camera rotation on shake
        /// </summary>
        /// <value></value>
        public float MaxRotation { get => maxRotation; set => maxRotation = value; }

        /// <summary>
        /// Max position offset to move camera to right or left
        /// </summary>
        /// <value></value>
        public float MaxOffsetX { get => maxOffsetX; set => maxOffsetX = value; }

        /// <summary>
        /// Max position offset to move camera to up or down
        /// </summary>
        /// <value></value>
        public float MaxOffsetY { get => maxOffsetY; set => maxOffsetY = value; }

        /// <summary>
        /// Max position offset to move camera to forward or backward
        /// </summary>
        /// <value></value>
        public float MaxOffsetZ { get => maxOffsetZ; set => maxOffsetZ = value; }

        public MMV_CameraShakeSettings()
        {
            Length = 1;
            ShakeVelocity = 10;
            MaxRotation = 30;
            MaxOffsetX = 0.2f;
            MaxOffsetY = 0.2f;
            MaxOffsetZ = 1.0f;


            OnValidate();
        }

        private void OnValidate()
        {
            ValidadeCameraShakeForce();
        }

        private void ValidadeCameraShakeForce()
        {
            if (shakeForce == null)
            {
                shakeForce = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.4f, 0.3f), new Keyframe(1, 0));
            }

            while (shakeForce.keys.Length < 2)
            {
                shakeForce.AddKey(0, 0);
            }

            if (shakeForce[0].value != 1)
            {
                shakeForce.RemoveKey(0);
                shakeForce.AddKey(new Keyframe(0, 1));
            }
            if (shakeForce[shakeForce.length - 1].value != 1)
            {
                shakeForce.RemoveKey(shakeForce.length - 1);
                shakeForce.AddKey(new Keyframe(1, 0));
            }

            shakeForce = MMV_Utils.ClampAnimationCurve(shakeForce, 1, 1, 0, 0);
        }
    }
}
