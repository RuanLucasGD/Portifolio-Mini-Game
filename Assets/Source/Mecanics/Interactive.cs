using UnityEngine;
using UnityEngine.Events;

namespace Game.Mecanics
{
    public class Interactive : MonoBehaviour
    {
        public UnityAction onInteract;

        public bool IsSelected { get; set; }
        public bool IsPressing { get; private set; }
        public bool IsOver { get; private set; }
        public bool DestroyOnInteract { get; set; }

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

        protected virtual void Update()
        {
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

            if (IsSelected)
            {
                SetSelected();
            }
        }

        protected virtual void SetPressed() { }
        
        protected virtual void SetOver() { }

        protected virtual void SetSelected() { }
    }
}
