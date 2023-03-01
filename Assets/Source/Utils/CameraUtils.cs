using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utils
{
    public static class CameraUtils
    {
        public static bool IsOnScreen(Camera camera, Vector3 point)
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
    }

}

