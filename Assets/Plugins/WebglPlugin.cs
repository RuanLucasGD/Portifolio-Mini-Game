using System.Runtime.InteropServices;

public static class WebglPlugin
{
    [DllImport("__Internal")]
    private static extern bool IsMobileDevice();

    public static bool IsMobile
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsMobileDevice();
#else
            return false;
#endif
        }
    }
}
