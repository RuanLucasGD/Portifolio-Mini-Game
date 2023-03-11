using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Mecanics
{
    public class CameraController : MonoBehaviour
    {
        public MMV.MMV_TrackedVehicle target;
        public float distance;
        public float height;
        public Vector3 direction;

        [Space]
        public float dragSensitive;
        public float maxDragDistance;
        public float resedTimerVelocity;
        public float resetDragDelay;

        private Vector3 currentDrag;
        private Vector3 lastMousePosition;
        private bool isResetingDrag;
        private float activeResetTimer;

        public bool IsTouchingScreen { get; set; }              // called by Trigger Event Component on game UI
        public bool IsMouseTouchingScreen { get; private set; }
        public bool UseTouchScreen { get; set; }                // called on mobile ui manager on game UI

        private Vector2 ScreenProporcion => new Vector2((float)Screen.width / Screen.height, (float)Screen.height / Screen.width);
        private float DistanceByScreenSize => ScreenProporcion.magnitude * distance;
        private bool VehicleIsAccelerating => target.IsTurning || target.IsAccelerating;

        public CameraController()
        {
            distance = 5;
            height = 10;
            direction = new Vector3(1, 2, 1);
            dragSensitive = 0.1f;
            resedTimerVelocity = 5;
            resetDragDelay = 1;
        }

        private void Start()
        {
            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        private void Update()
        {
            UpdateMouseDrag();
            UpdateCameraPosition();
        }

        private void LateUpdate()
        {
            ResetDrag();
        }

        private void ResetDrag()
        {
            // focus on vehicle when player accelerate vehicle
            if (VehicleIsAccelerating)
            {
                isResetingDrag = true;
            }

            if (!IsTouchingScreen && !IsMouseTouchingScreen)
            {
                if (!isResetingDrag)
                {
                    if (activeResetTimer < resetDragDelay)
                    {
                        activeResetTimer += Time.deltaTime;
                    }
                    else
                    {
                        activeResetTimer = 0;
                        isResetingDrag = true;
                    }
                }
            }

            if (isResetingDrag)
            {
                currentDrag -= currentDrag * Mathf.Clamp(Time.deltaTime * resedTimerVelocity, 0, currentDrag.magnitude);
            }
        }

        private void UpdateCameraRotation()
        {
            transform.LookAt(target.transform.position);
        }

        private void UpdateCameraPosition()
        {
            var _cameraDirection = direction * DistanceByScreenSize;
            transform.position = target.transform.position + _cameraDirection + currentDrag;
        }

        private void AddDragVelocity(Vector2 screenDrag)
        {
            if (VehicleIsAccelerating)
            {
                return;
            }

            var screenToWorldVelocity = new Vector3();
            screenToWorldVelocity = transform.right * screenDrag.x;
            screenToWorldVelocity += transform.forward * screenDrag.y;
            screenToWorldVelocity = Vector3.ProjectOnPlane(screenToWorldVelocity, Vector3.up);
            screenToWorldVelocity *= dragSensitive;
            currentDrag += screenToWorldVelocity;
            currentDrag = Vector3.ClampMagnitude(currentDrag, maxDragDistance);
            activeResetTimer = 0f;
            isResetingDrag = false;
        }

        private void UpdateMouseDrag()
        {
            IsMouseTouchingScreen = Input.GetKey(KeyCode.Mouse1);

            if (IsMouseTouchingScreen && !UseTouchScreen)
            {
                AddDragVelocity(lastMousePosition - Input.mousePosition);
            }

            lastMousePosition = Input.mousePosition;
        }

        public void ResetCameraDragImmediately()
        {
            isResetingDrag = true;
        }

        public void AddMobileDragScreen(Vector2 screenDrag)
        {
            if (UseTouchScreen)
            {
                AddDragVelocity(screenDrag);
            }
        }
    }
}
