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
        public UnityEvent onRecreate;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialProperties;

        private float currentIntensity;

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

        protected override void Update()
        {
            base.Update();
            UpdateMaterialColor();
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

            if (!IsPressing && !IsOver && !IsSelected)
            {
                var _decreaseSpeed = Mathf.Min(currentIntensity * Time.deltaTime * colorTransitionSpeed, currentIntensity);
                currentIntensity -= _decreaseSpeed;

                if (currentIntensity < 0.01f)
                {
                    currentIntensity = 0;
                }
            }
        }

        protected override void SetSelected()
        {
            if (currentIntensity < selectedColorIntensity)
            {
                currentIntensity = selectedColorIntensity;
            }

            onSelect.Invoke(this);
        }

        protected override void SetPressed()
        {
            currentIntensity = pressedColorIntensity;
            onPress.Invoke(this);
        }

        protected override void SetOver()
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
            onRecreate.Invoke();
        }
    }
}
