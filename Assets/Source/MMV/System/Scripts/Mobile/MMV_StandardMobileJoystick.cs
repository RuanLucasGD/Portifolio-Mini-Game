using UnityEngine;
using UnityEngine.EventSystems;

namespace MMV
{
    /// <summary>
    /// Component for creating a joystick directional pad for mobile
    /// </summary>
    public class MMV_StandardMobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public RectTransform center;
        public RectTransform joystick;
        public float radius;

        private bool isPressed;
        private Vector3 joystickDirection;

        private PointerEventData pointerData;

        /// <summary>
        /// Returns true when the joystick is pressed
        /// </summary>
        private bool IsPressed => isPressed;

        /// <summary>
        /// The direction the player wants to move
        /// </summary>
        public Vector3 JoystickDirection => joystickDirection;

        // Update is called once per frame
        void LateUpdate()
        {
            if (!center || !joystick)
            {
                return;
            }

            ControlJoystick(out joystickDirection);
        }

        public void OnPointerUp(PointerEventData e)
        {
            isPressed = false;
            pointerData = e;
        }

        public void OnPointerDown(PointerEventData e)
        {
            isPressed = true;
            pointerData = e;
        }

        private void ControlJoystick(out Vector3 outDirection)
        {
            outDirection = new Vector3();

            if (isPressed)
            {
                var _dir = (Vector3)pointerData.position - center.position;

                joystick.position = center.position + _dir;

                if (joystick.anchoredPosition.magnitude > radius)
                {
                    joystick.anchoredPosition = joystick.anchoredPosition.normalized * radius;
                }
            }
            else
            {
                joystick.position = center.position;
            }

            outDirection.x = joystick.anchoredPosition.x;
            outDirection.z = joystick.anchoredPosition.y;

            outDirection.x /= radius;
            outDirection.z /= radius;
        }
    }
}
