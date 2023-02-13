using UnityEngine;
using UnityEngine.Events;

namespace MMV
{
    /// <summary>
    /// Manage all mobile controls for the player's vehicle
    /// </summary>
    public class MMV_UI_MobileControlManager : MonoBehaviour
    {
        /// <summary>
        /// Group of vehicle weapons that will be used by the UI buttons for firing
        /// </summary>
        [System.Serializable]
        public class WeaponGroup
        {
            [SerializeField] private string name;
            [SerializeField] private bool control;
            [SerializeField] private MMV_ShooterManager[] weapons;

            /// <summary>
            /// Name of the weapons group
            /// </summary>
            /// <value></value>
            public string Name { get => name; set => name = value; }

            /// <summary>
            /// The player does control the weapons ?
            /// </summary>
            /// <value></value>
            public bool Control { get => control; set => control = value; }

            /// <summary>
            /// Weapons
            /// </summary>
            /// <value></value>
            public MMV_ShooterManager[] Weapons { get => weapons; set => weapons = value; }

            public WeaponGroup()
            {
                Control = true;
            }
        }

        [SerializeField] private bool useMobileControls;
        [SerializeField] private MMV_Vehicle vehicle;
        [SerializeField] private MMV_StandardCameraController cameraController;
        [SerializeField] private WeaponGroup[] vehicleWeapons;

        [Header("UI")]

        public MMV_StandardMobileJoystick joystick;
        public MMV_MobileTouch cameraControllerTouch;

        [Header("Mobile Controls parameters")]
        [SerializeField] private float cameraSensitivity;
        [SerializeField] private bool invertCameraVertical;
        [SerializeField] private bool invertCameraHorizontal;

        [Header("Events")]

        public UnityEvent onUseMobileControls;
        public BoolEvent onInvertCameraVertical;
        public BoolEvent onInvertCameraHorizontal;
        public FloatEvent onChangeCameraSensitivity;
        /// <summary>
        /// Sensitivity of camera movement using mobile controller buttons
        /// </summary>
        /// <value></value>
        public float CameraSensitivity
        {
            get => cameraSensitivity;
            set
            {
                cameraSensitivity = value;
                onChangeCameraSensitivity.Invoke(cameraSensitivity);
            }
        }

        /// <summary>
        /// Invert vertical mobile camera controls
        /// </summary>
        /// <value></value>
        public bool InvertCameraVertical
        {
            get => invertCameraVertical;
            set
            {
                invertCameraVertical = value;
                onInvertCameraVertical.Invoke(invertCameraVertical);
            }
        }

        /// <summary>
        /// Invert horizontal mobile camera controls
        /// </summary>
        /// <value></value>
        public bool InvertCameraHorizontal
        {
            get => invertCameraHorizontal;
            set
            {
                invertCameraHorizontal = value;
                onInvertCameraHorizontal.Invoke(invertCameraHorizontal);
            }
        }

        /// <summary>
        /// Vehicle to be controlled
        /// </summary>
        /// <value></value>
        public MMV_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        /// <summary>
        /// Vehicle camera controller
        /// </summary>
        /// <value></value>
        public MMV_StandardCameraController CameraController { get => cameraController; set => cameraController = value; }

        /// <summary>
        /// Weapons groups of the vehicle
        /// </summary>
        /// <value></value>
        public WeaponGroup[] VehicleWeapons { get => vehicleWeapons; set => vehicleWeapons = value; }

        public MMV_UI_MobileControlManager()
        {
            cameraSensitivity = 1;
        }

        private void OnEnable()
        {
            if (WebglPlugin.IsMobile)
            {
                useMobileControls = true;
            }

            if (!useMobileControls)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!Vehicle)
            {
                return;
            }

            if (CameraController)
            {
                CameraController.ControlsEnabled = false;
                CameraController.DisableMouseOnStart = false;

                CameraController.SetCursorActive(true);
            }

            onUseMobileControls.Invoke();
            onInvertCameraVertical.Invoke(InvertCameraVertical);
            onInvertCameraHorizontal.Invoke(InvertCameraHorizontal);
            onChangeCameraSensitivity.Invoke(CameraSensitivity);
        }

        private void Start()
        {
            if (!FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
            {
                new GameObject("Event System", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            SetEnableVehicleControlers(Vehicle, CameraController, false);
        }

        private void Update()
        {
            if (!Vehicle) return;
            if (CameraController) ControlCamera();
            if (joystick) ControlVehicleUsingJoystick();
        }

        private void ControlCamera()
        {
            if (!cameraControllerTouch)
            {
                return;
            }

            if (cameraControllerTouch.IsPressed)
            {
                var _dir = cameraControllerTouch.TouchDirection;
                var _horizontal = _dir.x * cameraSensitivity;
                var _vertical = _dir.y * cameraSensitivity;

                if (invertCameraHorizontal) _horizontal *= -1;
                if (invertCameraVertical) _vertical *= -1;

                CameraController.ControlCamera(_vertical, _horizontal);
            }
        }

        private void ControlVehicleUsingJoystick()
        {
            if (!joystick || !Vehicle)
            {
                return;
            }

            // The control system consists of taking the direction returned from the joystick and converting 
            // it into a direction in which the vehicle should go.
            ControlVehicle(Vehicle, VehicleAccelerationDirection(joystick.JoystickDirection));
        }

        private Vector3 VehicleAccelerationDirection(Vector3 joystickDirection)
        {
            var _camera = CameraController ? CameraController.SelectedCamera.Camera.transform : Camera.main.transform;
            var _forward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up) * joystickDirection.z;
            var _right = Vector3.ProjectOnPlane(_camera.right, Vector3.up) * joystickDirection.x;
            return _forward + _right;
        }

        private void ControlVehicle(MMV_Vehicle vehicle, Vector3 accelerationDirection)
        {
            if (Mathf.Abs(accelerationDirection.x) < 0.2f) accelerationDirection.x = 0f;
            if (Mathf.Abs(accelerationDirection.z) < 0.2f) accelerationDirection.z = 0f;
            if (accelerationDirection.magnitude < 0.2) accelerationDirection = Vector3.zero;
            vehicle.MoveTo(vehicle.transform.position + accelerationDirection, 0.2f, true, false);
        }

        /// <summary>
        /// enable or disable all controllers (Classes that inherit from MMV_ControllerBase) of a vehicle and its camera controller
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="cameraController"></param>
        /// <param name="enabled"></param>
        public void SetEnableVehicleControlers(MMV_Vehicle vehicle, MMV_StandardCameraController cameraController, bool enabled)
        {
            var _vehicleControllers = vehicle.GetComponentsInChildren<MMV_ControllerBase>();
            foreach (var c in _vehicleControllers) c.enabled = enabled;
        }

        /// <summary>
        /// ID of weapons group (0, 1, 2....)
        /// </summary>
        /// <param name="weaponGroup"></param>
        public void Shot(int weaponGroup)
        {
            if (VehicleWeapons.Length == 0)
            {
                return;
            }

            weaponGroup = Mathf.Clamp(weaponGroup, 0, VehicleWeapons.Length - 1);

            foreach (var w in VehicleWeapons[weaponGroup].Weapons)
            {
                if (w)
                {
                    w.Shoot();
                }
            }
        }

        /// <summary>
        /// Set te next vehicle camera
        /// </summary>
        public void ChangeCamera()
        {
            if (!CameraController)
            {
                return;
            }

            if (CameraController.CurrentCameraIndex + 1 < CameraController.Cameras.Length)
            {
                CameraController.SetCamera(CameraController.CurrentCameraIndex + 1);
            }
            else
            {
                CameraController.SetCamera(0);
            }
        }
    }
}