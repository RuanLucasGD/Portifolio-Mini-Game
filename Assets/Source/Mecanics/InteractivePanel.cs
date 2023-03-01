using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Mecanics
{
    public class InteractivePanel : Interactive
    {
        public float colorTransitionSpeed;
        [Space]
        public float selectedColorIntensity;
        public float overColorIntensity;
        public float pressedColorIntensity;

        [Space]
        public GameObject spawnOnDestroy;
        public Transform center;
        public float recreateAfterTime;

        [Space]
        public UnityEvent<InteractivePanel> onOver;
        public UnityEvent<InteractivePanel> onSelect;
        public UnityEvent<InteractivePanel> onPress;
        public UnityEvent<InteractivePanel> onInteractionCompleted;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialProperties;

        private bool isSelected;
        private float currentIntensity;

        public bool IsPressing { get; private set; }
        public bool IsOver { get; private set; }
        public bool DestroyOnInteract { get; set; }

        public InteractivePanel()
        {
            colorTransitionSpeed = 10;
        }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            materialProperties = new MaterialPropertyBlock();
            onInteract += OnInteract;
        }

        void Update()
        {
            UpdateMaterialColor();

            var _isPressingScreen = IsTouching(out var hit);
            var _isOverPanel = hit.transform == transform;
            var _isPressingPanel = _isPressingScreen && _isOverPanel;

            if (_isPressingPanel != IsPressing)
            {
                IsPressing = _isPressingScreen;
                if (IsPressing)
                {
                    SetPressed();
                }
            }

            if (_isOverPanel != IsOver)
            {
                IsOver = _isOverPanel;

                if (IsOver)
                {
                    SetOver();
                }
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
            if (!meshRenderer || currentIntensity < 0.01f)
            {
                return;
            }

            if (currentIntensity > 0.01f)
            {
                materialProperties.SetFloat("_Intensity", currentIntensity);
                meshRenderer.SetPropertyBlock(materialProperties);
            }

            if (!IsPressing && !IsOver)
            {
                var _decreaseSpeed = Mathf.Min(currentIntensity * Time.deltaTime * colorTransitionSpeed, currentIntensity);
                currentIntensity -= _decreaseSpeed;

                if (currentIntensity < 0.01f)
                {
                    currentIntensity = 0;
                }
            }
        }

        private void SetSelected()
        {
            if (currentIntensity < selectedColorIntensity)
            {
                currentIntensity = selectedColorIntensity;
            }

            onSelect.Invoke(this);
        }

        private void SetPressed()
        {
            currentIntensity = pressedColorIntensity;
            onPress.Invoke(this);
        }

        private void SetOver()
        {
            if (currentIntensity < overColorIntensity)
            {
                currentIntensity = overColorIntensity;
            }

            onOver.Invoke(this);
        }

        private void OnInteract()
        {
            if (DestroyOnInteract && spawnOnDestroy)
            {
                gameObject.SetActive(false);
                var _spawned = Instantiate(spawnOnDestroy, transform.position, transform.rotation);
                Destroy(_spawned, recreateAfterTime);
                GameManager.Instance.StartCoroutine(Reenable());

                DestroyOnInteract = false;
            }

            onInteractionCompleted.Invoke(this);
        }

        private IEnumerator Reenable()
        {
            yield return new WaitForSeconds(recreateAfterTime);
            gameObject.SetActive(true);
        }
    }
}
