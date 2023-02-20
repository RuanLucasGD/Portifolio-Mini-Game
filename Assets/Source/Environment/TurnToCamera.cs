using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnToCamera : MonoBehaviour
{

    void Start()
    {
        var mainCamera = Camera.main.transform;

        if (!mainCamera)
        {
            return;
        }

        var _lookAtDirection = Vector3.ProjectOnPlane(-mainCamera.forward, Vector3.up);
        transform.rotation = Quaternion.LookRotation(_lookAtDirection);
    }
}
