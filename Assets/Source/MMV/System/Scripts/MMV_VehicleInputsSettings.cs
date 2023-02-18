using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Create asset configuration inputs to control vehicle 
    /// </summary>
    [CreateAssetMenu(fileName = "Vehicle Inputs Settings", menuName = "MMV/Inputs/Vehicle Inputs Settings", order = 0)]
    public class MMV_VehicleInputsSettings : ScriptableObject
    {
        /// <summary>
        /// Storage all input axis and key to control vehicle
        /// </summary>
        [System.Serializable]
        public class ControlType
        {
            [SerializeField] private string steerInputAxis;
            [SerializeField] private string accelerationInputAxis;
            [SerializeField] private KeyCode brakeKey;

            /// <summary>
            /// Name of input axis of steer
            /// </summary>
            /// <value></value>
            public string SteerInputAxis { get => steerInputAxis; set => steerInputAxis = value; }

            /// <summary>
            /// Name of input axis of acceleration 
            /// </summary>
            /// <value></value>
            public string AccelerationInputAxis { get => accelerationInputAxis; set => accelerationInputAxis = value; }

            /// <summary>
            /// Input key / button of vehicle brake
            /// </summary>
            /// <value></value>
            public KeyCode BrakeKey { get => brakeKey; set => brakeKey = value; }

            public ControlType()
            {
                SteerInputAxis = "Horizontal";
                AccelerationInputAxis = "Vertical";
            }
        }

        [SerializeField] private ControlType keyboard;
        [SerializeField] private ControlType gamepad;

        /// <summary>
        /// Keyboard axis and keys settings
        /// </summary>
        /// <value></value>
        public ControlType Keyboard { get => keyboard; set => keyboard = value; }

        /// <summary>
        /// Gamepad axis and keys settings
        /// </summary>
        /// <value></value>
        public ControlType Gamepad { get => gamepad; set => gamepad = value; }

        /// <summary>
        /// Current acceleration
        /// </summary>
        /// <value></value>
        public virtual float AccelerationInput
        {
            get
            {
                var _keyboard = 0f;
                var _gamepad = 0f;
                if (!string.IsNullOrEmpty(Keyboard.AccelerationInputAxis)) _keyboard = Input.GetAxis(Keyboard.AccelerationInputAxis);
                if (!string.IsNullOrEmpty(Gamepad.AccelerationInputAxis)) _gamepad = Input.GetAxis(Gamepad.AccelerationInputAxis);
                return Mathf.Abs(_keyboard) > Mathf.Abs(_gamepad) ? _keyboard : _gamepad;
            }
        }

        /// <summary>
        /// Current steering
        /// </summary>
        /// <value></value>
        public virtual float SteerInput
        {
            get
            {
                var _keyboard = 0f;
                var _gamepad = 0f;
                if (!string.IsNullOrEmpty(Keyboard.SteerInputAxis)) _keyboard = Input.GetAxis(Keyboard.SteerInputAxis);
                if (!string.IsNullOrEmpty(Gamepad.SteerInputAxis)) _gamepad = Input.GetAxis(Gamepad.SteerInputAxis);
                return Mathf.Abs(_keyboard) > Mathf.Abs(_gamepad) ? _keyboard : _gamepad;
            }
        }

        /// <summary>
        /// Check if the player is braking
        /// </summary>
        /// <returns></returns>
        public virtual bool BrakingInput => Input.GetKey(Keyboard.BrakeKey) || Input.GetKey(Gamepad.BrakeKey);
    }
}