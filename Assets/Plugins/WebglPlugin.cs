using System.Runtime.InteropServices;

public static class WebglPlugin
{
    [DllImport("__Internal")]
    private static extern bool IsMobileDevice();

    [DllImport("__Internal")]
    private static extern void OpenUrl(string url);

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

    public static void OpenLink(string link)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenUrl(link);
#endif
    }
}
