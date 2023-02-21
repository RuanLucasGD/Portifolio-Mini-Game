using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Mecanics;

namespace Game
{
    public class InteractivePanel : MonoBehaviour
    {
        public float colorTransitionSpeed;
        [Space]
        public float selectedColorIntensity;
        public float overColorIntensity;
        public float pressedColorIntensity;
        [Space]

        private PlayerTurretController playerTurretController;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialProperties;

        private bool isSelected;
        private float currentIntensity;

        public InteractivePanel()
        {
            colorTransitionSpeed = 10;
        }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            materialProperties = new MaterialPropertyBlock();
            playerTurretController = FindObjectOfType<PlayerTurretController>();

            if (!playerTurretController)
            {
                Debug.LogError($"{typeof(PlayerTurretController).Name} not finded on scene");
            }
            else
            {
                playerTurretController.onSelectPanel.AddListener(OnPlayerSelectPanel);
            }
        }

        void Update()
        {
            UpdateMaterialColor();

            var _isPressing = IsTouching(out var hit);
            var _isOverPanel = hit.transform == transform;

            if (_isPressing && _isOverPanel)
            {
                SetPressed();
            }

            if (!_isPressing && _isOverPanel)
            {

                SetOver();
            }

            if (isSelected)
            {
                SetSelected();
            }
        }

        private bool IsTouching(out RaycastHit hit)
        {
            hit = default(RaycastHit);
            var _isTouch = Input.touchCount > 0;
            var _isPressing = _isTouch || Input.GetKey(KeyCode.Mouse0);
            var _pressPosition = _isTouch ? new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 0) : Input.mousePosition;
            _pressPosition.z = Camera.main.nearClipPlane;

            var _touchRay = Camera.main.ScreenPointToRay(_pressPosition);
            Physics.Raycast(_touchRay, out hit, 2000);

            return _isPressing;
        }

        private void UpdateMaterialColor()
        {
            if (currentIntensity > 0.01f)
            {
                var _decreaseSpeed = Mathf.Min(currentIntensity * Time.deltaTime * colorTransitionSpeed, currentIntensity);
                currentIntensity -= _decreaseSpeed;

                if (currentIntensity < 0.01f)
                {
                    currentIntensity = 0;
                }

                if (meshRenderer)
                {
                    materialProperties.SetFloat("_Intensity", currentIntensity);
                    meshRenderer.SetPropertyBlock(materialProperties);
                }
            }
        }

        private void OnPlayerSelectPanel(GameObject selectedPanel)
        {
            isSelected = selectedPanel == gameObject;

            if (playerTurretController && !playerTurretController.IsOnInteractiveArea)
            {
                isSelected = false;
            }
        }

        private void SetSelected()
        {
            if (currentIntensity < selectedColorIntensity)
            {
                currentIntensity = selectedColorIntensity;
            }
        }

        private void SetPressed()
        {
            currentIntensity = pressedColorIntensity;
        }

        private void SetOver()
        {
            if (currentIntensity < overColorIntensity)
            {
                currentIntensity = overColorIntensity;
            }
        }
    }
}
