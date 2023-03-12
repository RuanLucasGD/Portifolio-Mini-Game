using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Game.Utils;

namespace Game.Mecanics
{
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Tag of panels to turret lookAt.")]
        public InteractiveTrigger interactiveTrigger;
        public MMV.MMV_ShooterManager vehicleWeapon;
        public MMV.MMV_Vehicle vehicle;

        [Header("Moviment Controls")]
        public string Horizontal;
        public string Vertical;
        public KeyCode brakeKey;

        [Space]
        [Range(0.5f, 1)] public float shotPrecision;
        public LayerMask aimObstacles;

        [Space]
        public UnityEvent<GameObject> onSelectPanel;

        private const float GET_IN_VIEW_PANELS_INTERVAL = 1f;
        private const float GET_NEAR_PANELS_INTERVAL = 0.5f;

        private GameObject[] interactablesInView;

        public Transform Target { get; set; }

        public float HorizontalInput => Input.GetAxis(Horizontal);
        public float VerticalInput => Input.GetAxis(Vertical);

        public bool IsOnInteractiveArea { get; private set; }
        public bool AutoSelectPanel { get; set; }

        public bool CanShot { get; set; }
        public bool CanVehicleMove { get; set; }

        public bool IsVehicleMoving => (vehicle.IsAccelerating || vehicle.IsTurning) && CanVehicleMove;

        public bool TargetOnView
        {
            get
            {
                if (!vehicleWeapon || !vehicleWeapon.Rotation.VerticalTransform)
                {
                    return false;
                }

                var _weaponCannon = vehicleWeapon.Rotation.VerticalTransform;
                var _targetPosition = vehicleWeapon.TargetPosition;
                var _dotAngleToTarget = Vector3.Dot(_weaponCannon.forward, (Target.transform.position - _weaponCannon.position).normalized);
                var _distanceToTarget = Vector3.Distance(vehicle.transform.position, Target.transform.position);

                // the greater the distance, the greater the accuracy
                var _precisionByDistance = 1 - Mathf.Max(_dotAngleToTarget, 0);
                _precisionByDistance = Mathf.Max(_precisionByDistance, 1);
                _precisionByDistance = _dotAngleToTarget / _precisionByDistance;

                return _precisionByDistance > shotPrecision;
            }
        }

        public PlayerController()
        {
            shotPrecision = 0.8f;
        }

        void Start()
        {
            interactiveTrigger.onEnter.AddListener(OnEnterOnInteractiveArea);
            interactiveTrigger.onExit.AddListener(OnExitOfInteractiveArea);
            interactablesInView = new GameObject[] { };
            AutoSelectPanel = true;
            CanVehicleMove = true;

            InvokeRepeating(nameof(UpdateInViewPanelsList), GET_IN_VIEW_PANELS_INTERVAL, GET_IN_VIEW_PANELS_INTERVAL);
            InvokeRepeating(nameof(UpdateFindPanelOnScreenCenter), GET_NEAR_PANELS_INTERVAL, GET_NEAR_PANELS_INTERVAL);
        }

        void Update()
        {
            if (!vehicleWeapon)
            {
                return;
            }

            if (!CanVehicleMove)
            {
                vehicle.IsBraking = true;
            }

            if (IsOnInteractiveArea && interactablesInView.Length > 0)
            {
                TurretLookAtPanels();
                ShootWhenInteractWithPanel();
            }
            else
            {
                TurretLookAtForward();
            }

            ControlVehicle();
        }

        private void ControlVehicle()
        {
            var _isBraking = false;

            _isBraking |= Input.GetKey(brakeKey);
            _isBraking |= HorizontalInput == 0 && VerticalInput == 0;
            _isBraking |= !CanVehicleMove;

            vehicle.PlayerInputs(VerticalInput, HorizontalInput, _isBraking);
        }

        private void ShootWhenInteractWithPanel()
        {
            if (!CanShot)
            {
                return;
            }

            if (TargetOnView)
            {
                if (vehicleWeapon.CanShot)
                {
                    vehicleWeapon.Shoot();
                }
            }
        }

        private void TurretLookAtForward()
        {
            vehicleWeapon.TargetPosition = transform.position + (transform.forward * 10);
        }

        private void TurretLookAtPanels()
        {
            if (interactablesInView.Length == 0 || !Target)
            {
                return;
            }

            vehicleWeapon.TargetPosition = Target.transform.position;
        }

        private void UpdateInViewPanelsList()
        {
            if (!IsOnInteractiveArea || !AutoSelectPanel)
            {
                return;
            }

            var _camera = Camera.main;
            var _inView = new List<GameObject>();

            foreach (var p in GameManager.Instance.Interactables)
            {
                if (CameraUtils.IsOnScreen(_camera, p.transform.position))
                {
                    _inView.Add(p.gameObject);
                }
            }

            interactablesInView = _inView.ToArray();
        }

        private void UpdateFindPanelOnScreenCenter()
        {
            if (interactablesInView.Length == 0 || !AutoSelectPanel)
            {
                return;
            }

            var _near = interactablesInView[0];
            var _camera = Camera.main.transform;
            var _minorAngle = Vector3.Angle(_camera.forward, (_near.transform.position - _camera.position).normalized);

            foreach (var p in interactablesInView)
            {
                // angle to camera direction
                var _panelAngle = Vector3.Angle(_camera.forward, (p.transform.position - _camera.position).normalized);

                if (_panelAngle < _minorAngle)
                {
                    _minorAngle = _panelAngle;
                    _near = p;
                }
            }

            if (Target != _near)
            {
                Target = _near.transform;
                onSelectPanel.Invoke(Target.gameObject);

            }
        }

        private void OnEnterOnInteractiveArea(Collider other)
        {
            IsOnInteractiveArea = true;
            UpdateInViewPanelsList();
            UpdateFindPanelOnScreenCenter();
        }

        private void OnExitOfInteractiveArea(Collider other)
        {
            IsOnInteractiveArea = false;
            onSelectPanel.Invoke(null);
        }

        public void Interact(Transform target)
        {
            Target = target;
            onSelectPanel.Invoke(Target.gameObject);
        }
    }
}
