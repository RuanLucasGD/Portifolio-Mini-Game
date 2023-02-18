using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MMV
{
    /// <summary>
    /// Use on some UI button to capture player touch data
    /// </summary>
    public class MMV_MobileTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent onClick;
        public UnityEvent onReleased;
        public UnityEvent onPressed;

        private bool isPressed;
        private Vector2 dragDirection;
        private Vector2 lastPointPosition;
        private PointerEventData pointerData;

        /// <summary>
        /// Check if the touch of the camera is being pressed
        /// </summary>
        public bool IsPressed => isPressed;

        /// <summary>
        /// When touch is pressed, returns the direction in which the user drags the finger on the screen
        /// </summary>
        /// <returns></returns>
        public Vector2 TouchDirection => dragDirection;

        private void Update()
        {
            dragDirection = Vector2.zero;

            if (IsPressed)
            {
                dragDirection = pointerData.position - lastPointPosition;
                lastPointPosition = pointerData.position;

                onPressed.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            isPressed = false;
            pointerData = e;

            onReleased.Invoke();
        }

        public void OnPointerDown(PointerEventData e)
        {
            isPressed = true;
            pointerData = e;
            lastPointPosition = e.position;
            onClick.Invoke();
        }
    }
}
