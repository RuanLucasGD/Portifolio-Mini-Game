using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Game.UI
{
    [RequireComponent(typeof(EventTrigger))]
    public class UI_Touch : MonoBehaviour
    {
        public UnityEvent<Vector2> onDrag;
        public UnityEvent<bool> isDraging;
        public UnityEvent<bool> useMobileTouch;
   
        private Vector2 lastTouchPosition;
        private Vector2 dragVelocity;

        public bool IsTouching { get; private set; }

        /// <summary>
        /// Called from Trigger Component event
        /// </summary>
        public void UpdateTouch(BaseEventData e)
        {
            if (!(e is PointerEventData))
            {
                return;
            }

            if (IsTouching != true)
            {
                IsTouching = true;
                lastTouchPosition = ((PointerEventData)e).position;
                isDraging.Invoke(true);
            }

            var _event = (PointerEventData)e;

            dragVelocity = lastTouchPosition - _event.position;
            lastTouchPosition = _event.position;

            onDrag.Invoke(dragVelocity);
        }

        public void Drop()
        {
            IsTouching = false;
            isDraging.Invoke(false);
        }
    }
}
