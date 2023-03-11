using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Utils
{
    public class Email : MonoBehaviour
    {
        public float delay;
        public UnityEvent onOpen;

        public void OpenEmailDelayed()
        {
            IEnumerator OpenDelayed()
            {
                yield return new WaitForSeconds(delay);
                OpenEmail();
            }

            StartCoroutine(OpenDelayed());
        }

        public void OpenEmail()
        {
            WebglPlugin.OpenEmailBox();
            onOpen.Invoke();
        }
    }
}


