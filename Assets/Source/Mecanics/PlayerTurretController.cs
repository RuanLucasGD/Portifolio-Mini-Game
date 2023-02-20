using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Mecanics
{
    public class PlayerTurretController : MonoBehaviour
    {
        [Tooltip("Tag of panels to turret lookAt.")]
        public string panelsTag;
        public InteractiveTrigger interactiveTrigger;
        public MMV.MMV_ShooterManager vehicleWeapon;

        private const float GET_IN_VIEW_PANELS_INTERVAL = 1f;
        private const float GET_NEAR_PANELS_INTERVAL = 0.5f;

        private GameObject[] allPanels;
        private GameObject[] inViewPanels;
        private GameObject nearPanel;

        public bool IsOnInteractiveArea { get; private set; }

        void Start()
        {
            interactiveTrigger.onEnter.AddListener(OnEnter);
            interactiveTrigger.onExit.AddListener(OnExit);
            allPanels = GameObject.FindGameObjectsWithTag(panelsTag);
            inViewPanels = new GameObject[] { };

            InvokeRepeating(nameof(UpdateInViewPanelsList), GET_IN_VIEW_PANELS_INTERVAL, GET_IN_VIEW_PANELS_INTERVAL);
            InvokeRepeating(nameof(UpdateFindNearPanel), GET_NEAR_PANELS_INTERVAL, GET_NEAR_PANELS_INTERVAL);
        }

        void Update()
        {
            if (!vehicleWeapon)
            {
                return;
            }

            if (IsOnInteractiveArea)
            {
                TurretLookAtPanels();
            }
            else
            {
                TurretLookAtForward();
            }
        }

        private void TurretLookAtForward()
        {
            vehicleWeapon.TargetPosition = transform.position + (transform.forward * 10);
        }

        private void TurretLookAtPanels()
        {
            if (inViewPanels.Length == 0 || !nearPanel)
            {
                return;
            }

            vehicleWeapon.TargetPosition = nearPanel.transform.position;
        }

        private void UpdateInViewPanelsList()
        {
            if (!IsOnInteractiveArea)
            {
                return;
            }

            var _camera = Camera.main;
            var _inView = new List<GameObject>();

            foreach (var p in allPanels)
            {
                if (IsOnScreen(_camera, p.transform.position))
                {
                    _inView.Add(p);
                }
            }

            inViewPanels = _inView.ToArray();
        }

        private void UpdateFindNearPanel()
        {
            if (inViewPanels.Length == 0)
            {
                return;
            }

            var _near = inViewPanels[0];
            var _distance = Vector3.Distance(transform.position, _near.transform.position);

            foreach (var p in inViewPanels)
            {
                var _newDistance = Vector3.Distance(transform.position, p.transform.position);
                if (_newDistance < _distance)
                {
                    _distance = _newDistance;
                    _near = p;
                }
            }

            nearPanel = _near;
        }

        private bool IsOnScreen(Camera camera, Vector3 point)
        {
            if (!camera)
            {
                return false;
            }

            if (Vector3.Dot(camera.transform.forward, (point - camera.transform.position).normalized) < 0)
            {
                return false;
            }

            var _screenPoint = camera.WorldToScreenPoint(point);

            return (_screenPoint.x > 0) &&
                   (_screenPoint.y > 0) &&
                   (_screenPoint.x < Screen.width) &&
                   (_screenPoint.y < Screen.height);
        }

        private void OnEnter(Collider other)
        {
            IsOnInteractiveArea = true;
        }

        private void OnExit(Collider other)
        {
            IsOnInteractiveArea = false;
        }
    }
}


