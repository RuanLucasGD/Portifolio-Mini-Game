using UnityEngine;
using UnityEngine.UI;

namespace MMV
{
    /// <summary>
    /// Manage player vehicle HUD
    /// </summary>
    public class MMV_VehicleHud : MonoBehaviour
    {
        /// <summary>
        /// UI elements representing vehicle weapon
        /// </summary>
        [System.Serializable]
        public class VehicleWeapons
        {
            [SerializeField] private string name;
            [SerializeField] private MMV_ShooterManager weapon;
            [SerializeField] private Image aimIcon;
            [Space]
            [SerializeField] private Slider reloadProgress;
            [SerializeField] private Slider shotIntervalProgress;
            [Space]
            [SerializeField] private Text ammoAmount;
            [SerializeField] private Text slotsAmount;
            [Space]
            [SerializeField] private Image vehicleWeaponIcon;

            /// <summary>
            /// Weapon of vehicle
            /// </summary>
            /// <value></value>
            public MMV_ShooterManager Weapon { get => weapon; set => weapon = value; }

            /// <summary>
            /// UI icon that tracks weapon crosshair movement
            /// </summary>
            /// <value></value>
            public Image AimIcon { get => aimIcon; set => aimIcon = value; }

            /// <summary>
            /// Bar that storage weapon loading progress
            /// </summary>
            /// <value></value>
            public Slider ReloadProgress { get => reloadProgress; set => reloadProgress = value; }

            /// <summary>
            /// Bar that stores the weapon's fire cooldown time
            /// </summary>
            /// <value></value>
            public Slider ShotIntervalProgress { get => shotIntervalProgress; set => shotIntervalProgress = value; }

            /// <summary>
            /// UI text displaying weapon ammo amount
            /// </summary>
            /// <value></value>
            public Text AmmoAmount { get => ammoAmount; set => ammoAmount = value; }

            /// <summary>
            /// UI text that show the vehicle ammunation slots amount
            /// </summary>
            /// <value></value>
            public Text SlotsAmount { get => slotsAmount; set => slotsAmount = value; }

            /// <summary>
            /// Weapon icon
            /// </summary>
            /// <value></value>
            public Image VehicleWeaponIcon { get => vehicleWeaponIcon; set => vehicleWeaponIcon = value; }
        }

        /// <summary>
        /// UI elements representing the vehicle
        /// </summary>
        [System.Serializable]
        public class Vehicle
        {
            [SerializeField] private MMV_Vehicle playerVehicle;
            [Space]
            [SerializeField] private RectTransform bodyIcon;
            [Space]
            [SerializeField] private Text velocity;
            [SerializeField] private Text gear;

            /// <summary>
            /// Vehicle target
            /// </summary>
            public MMV_Vehicle PlayerVehicle { get => playerVehicle; set => playerVehicle = value; }

            /// <summary>
            /// UI gear text component
            /// </summary>
            /// <value></value>
            public Text Gear { get => gear; set => gear = value; }

            /// <summary>
            /// UI velocity text component
            /// </summary>
            /// <value></value>
            public Text Velocity { get => velocity; set => velocity = value; }

            /// <summary>
            /// UI icon representing the vehicle body
            /// </summary>
            /// <value></value>
            public RectTransform BodyIcon { get => bodyIcon; set => bodyIcon = value; }
        }

        [SerializeField] private MMV_StandardCameraController cameraController;
        [SerializeField] private Vehicle vehicle;
        [SerializeField] private VehicleWeapons[] weapons;

        /// <summary>
        /// Camera controller of vehicle
        /// </summary>
        /// <value></value>
        public MMV_StandardCameraController CameraController { get => cameraController; set => cameraController = value; }

        /// <summary>
        /// UI elements representing vehicle weapon
        /// </summary>
        /// <value></value>
        public VehicleWeapons[] WeaponsUI { get => weapons; set => weapons = value; }

        /// <summary>
        ///  UI elements representing the vehicle
        /// </summary>
        /// <value></value>
        public Vehicle VehicleUI { get => vehicle; set => vehicle = value; }

        void FixedUpdate()
        {
            UpdateWeaponsAimMarker();
        }

        private void Update()
        {
            UpdateWeaponsStatus();
            UpdateVehicleStatus();
            UpdateVehicleIcon();
        }

        private void UpdateWeaponsAimMarker()
        {
            if (!cameraController)
            {
                return;
            }

            var _camera = cameraController.SelectedCamera.Camera;

            if (!_camera)
            {
                return;
            }

            foreach (var w in WeaponsUI)
            {
                if (w.AimIcon)
                {
                    w.AimIcon.transform.position = AimPositionOnScreen(_camera, w.Weapon, out var isOnScren);
                    w.AimIcon.gameObject.SetActive(isOnScren);
                }
            }
        }

        private void UpdateWeaponsStatus()
        {
            foreach (var w in weapons)
            {
                if (w.ReloadProgress) UpdateBar(w.ReloadProgress, w.Weapon.ReloadProgress, 1);
                if (w.ShotIntervalProgress) UpdateBar(w.ShotIntervalProgress, w.Weapon.ShotIntervalProgress, 1);
                if (w.AmmoAmount) w.AmmoAmount.text = w.Weapon.AmmunationAmount.ToString();
                if (w.SlotsAmount) w.SlotsAmount.text = w.Weapon.AmmunationSlotsAmount.ToString();
            }
        }

        private void UpdateVehicleStatus()
        {
            if (!VehicleUI.PlayerVehicle)
            {
                return;
            }

            if (VehicleUI.Velocity) VehicleUI.Velocity.text = VehicleUI.PlayerVehicle.VelocityKMH.ToString("0");
            if (VehicleUI.Gear) VehicleUI.Gear.text = VehicleUI.PlayerVehicle.CurrentGear.ToString();
        }

        private void UpdateBar(Slider bar, float current, float max)
        {
            if (bar.minValue != 0 || bar.maxValue != max)
            {
                bar.minValue = 0;
                bar.maxValue = max;
            }

            bar.value = current;
        }

        private Vector3 AimPositionOnScreen(Camera camera, MMV_ShooterManager weapon, out bool isOnScreen)
        {
            var _weaponReference = weapon.Weapon.Spawner;
            var _maxAimDistance = MMV_StandardCameraController.DEFAULT_WEAPONS_AIM_DISTANCE;
            var _weaponTargetPosition = _weaponReference.position + (_weaponReference.forward * _maxAimDistance);
            var _aimLayer = cameraController.Weapons.TargetsLayer;

            if (cameraController.LineCast(_weaponReference.position, _weaponTargetPosition, out var hit, _aimLayer))
            {
                _weaponTargetPosition = hit.point;
            }

            var _lookToTarget = Quaternion.LookRotation((_weaponTargetPosition - camera.transform.position).normalized);
            var _lookDirection = _lookToTarget * Vector3.forward;
            var _aimPos = camera.transform.position + _lookDirection;
            var _positionOnUI = camera.WorldToScreenPoint(_aimPos);

            isOnScreen = AimTargetIsOnScreen(camera, _aimPos);

            return _positionOnUI;
        }

        private void UpdateVehicleIcon()
        {
            if (!VehicleUI.PlayerVehicle || !cameraController || !cameraController.SelectedCamera.Camera)
            {
                return;
            }

            var _body = VehicleUI.BodyIcon;
            var _cameraRot = cameraController.SelectedCamera.Camera.transform.eulerAngles.y;

            if (_body)
            {
                _body.rotation = Quaternion.Euler(_body.eulerAngles.x, _body.eulerAngles.y, -VehicleUI.PlayerVehicle.transform.eulerAngles.y + _cameraRot);
            }

            foreach (var w in WeaponsUI)
            {
                if (w.VehicleWeaponIcon)
                {
                    if (w.Weapon && w.Weapon.Weapon.Spawner)
                    {
                        var _currentRot = w.VehicleWeaponIcon.transform.rotation;
                        _currentRot = Quaternion.Euler(_currentRot.x, _currentRot.y, -w.Weapon.Weapon.Spawner.eulerAngles.y + _cameraRot);
                        w.VehicleWeaponIcon.transform.rotation = _currentRot;
                    }
                }
            }
        }

        private bool AimTargetIsOnScreen(Camera camera, Vector3 aimPos)
        {
            var _positionOnScreen = camera.WorldToScreenPoint(aimPos);

            if (Vector3.Dot(camera.transform.forward, (aimPos - camera.transform.position).normalized) < 0)
            {
                return false;
            }

            return (_positionOnScreen.x >= 0 && _positionOnScreen.x <= Screen.width) &&
                   (_positionOnScreen.y >= 0 && _positionOnScreen.y <= Screen.height);
        }
    }
}
