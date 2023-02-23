using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GraphicsManager : MonoBehaviour
{
    public CartoonShaderManager graphicsSettings;

    private void Awake()
    {
        if (!graphicsSettings)
        {
            return;
        }

        var _isMobile = WebglPlugin.IsMobile;

        Application.targetFrameRate = _isMobile ? 60 : 120;

        graphicsSettings.UpdateAllShaders();

        SetEnablePostProcessing(!_isMobile);
    }
    public void SetEnablePostProcessing(bool enabled)
    {
        if (!Camera.main)
        {
            return;
        }

        var camera = FindObjectOfType<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

        if (!camera)
        {
            return;
        }

        camera.renderPostProcessing = enabled;
    }
}
