using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utils
{
    public class DisableOnMobile : MonoBehaviour
    {
        public GameObject[] toDeactive;
        public MonoBehaviour[] toDisable;

        void Awake()
        {
            if (WebglPlugin.IsMobile)
            {
                foreach (var obj in toDeactive)
                {
                    obj.SetActive(false);
                }

                foreach (var comp in toDisable)
                {
                    comp.enabled = false;
                }
            }
        }
    }
}


