using UnityEngine;
using UnityEngine.Events;

namespace MMV
{
    /// <summary>
    /// Creates a configuration file with the player controls to use in the vehicle's camera
    /// </summary>
    [CreateAssetMenu(fileName = "Camera Inputs Settings", menuName = "MMV/Inputs/Camera Inputs Settings", order = 0)]
    public class MMV_CameraInputsSettings : ScriptableObject
    {
        /// <summary>
        /// Inputs used to store keyboard, mouse, and gamepad controls
        /// </summary>
        [System.Serializable]
        public class ControllerType
        {
            [SerializeField] private string horizontalAxis;
            [SerializeField] private string verticalAxis;

            [SerializeField] private bool invertVertical;
            [SerializeField] private bool invertHorizontal;

            [SerializeField] private float horizontalAxisMultiplier;
            [SerializeField] private float verticalAxisMultiplier;

            [SerializeField] private KeyCode changeCamera;

            /// <summary>
            /// Name of the Input Axis responsible for turning the camera horizontally
            /// </summary>
            /// <value></value>
            public string HorizontalAxis { get => horizontalAxis; set => horizontalAxis = value; }

            /// <summary>
            /// Name of the Input Axis responsible for turning the camera horizontally
            /// </summary>
            /// <value></value>
            public string VerticalAxis { get => verticalAxis; set => verticalAxis = value; }

            /// <summary>
            /// Invert the vertical input used on camera rotation
            /// </summary>
            /// <value></value>
            public bool InvertVertical { get => invertVertical; set => invertVertical = value; }

            /// <summary>
            /// Invert the horizontal input used on camera rotation
            /// </summary>
            /// <value></value>
            public bool InvertHorizontal { get => invertHorizontal; set => invertHorizontal = value; }

            /// <summary>
            /// Increase or decrease velocity of camera rotation horizontally
            /// </summary>
            /// <value></value>
            public float HorizontalAxisMultiplier { get => horizontalAxisMultiplier; set => horizontalAxisMultiplier = value; }

            /// <summary>
            /// Increase or decrease velocity of camera rotation vertically
            /// </summary>
            /// <value></value>
            public float VerticalAxisMultiplier { get => verticalAxisMultiplier; set => verticalAxisMultiplier = value; }

            /// <summary>
            /// Key used to change current selected camera of vehicle
            /// </summary>
            /// <value></value>
            public KeyCode ChangeCamera { get => changeCamera; set => changeCamera = value; }

            public ControllerType()
            {
                HorizontalAxis = "Mouse X";
                VerticalAxis = "Mouse Y";

                InvertVertical = false;
                InvertHorizontal = false;

                HorizontalAxisMultiplier = 1;
                VerticalAxisMultiplier = 1;
            }
        }

        [SerializeField] private ControllerType keyboardMouse;
        [SerializeField] private ControllerType gamepad;
        [SerializeField] private UnityEvent onChangeCamera;

        /// <summary>
        /// Player inputs of keyboard or mouse
        /// </summary>
        /// <value></value>
        public ControllerType KeyboardMouse { get => keyboardMouse; set => keyboardMouse = value; }

        /// <summary>
        /// Player inputs of gamepad
        /// </summary>
        /// <value></value>
        public ControllerType Gamepad { get => gamepad; set => gamepad = value; }

        /// <summary>
        /// Current Camera input rotation horizontally
        /// </summary>
        /// <value></value>
        public virtual float VerticalAxis
        {
            get
            {
                var _keyboard = 0f;
                var _gamepad = 0f;
                if (!string.IsNullOrEmpty(KeyboardMouse.VerticalAxis)) _keyboard = Input.GetAxis(KeyboardMouse.VerticalAxis);
                if (!string.IsNullOrEmpty(Gamepad.VerticalAxis)) _gamepad = Input.GetAxis(Gamepad.VerticalAxis);

                _keyboard *= KeyboardMouse.VerticalAxisMultiplier;
                _gamepad *= Gamepad.VerticalAxisMultiplier;

                return Mathf.Abs(_keyboard) >= Mathf.Abs(_gamepad) ? _keyboard : _gamepad;
            }
        }

        /// <summary>
        /// Current Camera input rotation vertically
        /// </summary>
        /// <value></value>
        public virtual float HorizontalAxis
        {
            get
            {
                var _keyboard = 0f;
                var _gamepad = 0f;
                if (!string.IsNullOrEmpty(KeyboardMouse.HorizontalAxis)) _keyboard = Input.GetAxis(KeyboardMouse.HorizontalAxis);
                if (!string.IsNullOrEmpty(Gamepad.HorizontalAxis)) _gamepad = Input.GetAxis(Gamepad.HorizontalAxis);

                _keyboard *= KeyboardMouse.HorizontalAxisMultiplier;
                _gamepad *= Gamepad.HorizontalAxisMultiplier;

                return Mathf.Abs(_keyboard) >= Mathf.Abs(_gamepad) ? _keyboard : _gamepad;
            }
        }

        /// <summary>
        /// Return true if the player press button to change vehicle camera
        /// </summary>
        /// <value></value>
        public virtual bool ChangeCamera
        {
            get
            {
                var _isChanging = Input.GetKeyDown(KeyboardMouse.ChangeCamera) || Input.GetKeyDown(Gamepad.ChangeCamera);

                if (_isChanging)
                {
                    OnChangeCamera.Invoke();
                }

                return _isChanging;
            }
        }

        /// <summary>
        /// Check if player is turning the camera
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTurning => (Mathf.Abs(VerticalAxis) + Mathf.Abs(HorizontalAxis)) > 0;

        /// <summary>
        /// Add delegates to execute when player change the vehicle camera;
        /// </summary>
        /// <value></value>
        public UnityEvent OnChangeCamera { get => onChangeCamera; set => onChangeCamera = value; }
    }
}
