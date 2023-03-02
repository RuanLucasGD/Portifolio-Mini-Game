using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Utils
{
    public class UrlLink : MonoBehaviour
    {
        public string link;
        public float delay;

        public UnityEvent onOpen;

        private bool open;
        private float delayTimer;

        private void Update()
        {
            if (!open)
            {
                return;
            }

            if (delayTimer < delay)
            {
                delayTimer += Time.deltaTime;

                if (delayTimer > delay)
                {
                    OpenLink();
                    delayTimer = 0;
                    open = false;
                }
            }
        }

        public void OpenLinkDelayed()
        {
            open = true;
        }

        public void OpenLink()
        {
            onOpen.Invoke();

#if UNITY_WEBGL && !UNITY_EDITOR
            WebglPlugin.OpenLink(link);
#else
            try
            {
                Process.Start(link);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    link = link.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", link);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", link);
                }
                else
                {
                    throw;
                }
            }
#endif
        }
    }
}
