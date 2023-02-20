using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Mecanics
{
    public class InteractiveTrigger : MonoBehaviour
    {
        public string[] interactTags;
        public UnityEvent<Collider> onEnter;
        public UnityEvent<Collider> onExit;

        public InteractiveTrigger()
        {
            interactTags = new string[2] { "Default", "Player" };
        }

        private void OnTriggerEnter(Collider other)
        {

            Interact(other, interactTags, () => onEnter.Invoke(other));
        }

        private void OnTriggerExit(Collider other)
        {
            Interact(other, interactTags, () => onExit.Invoke(other));
        }

        private void Interact(Collider other, string[] interactTags, UnityAction onInteract)
        {
            foreach (var t in interactTags)
            {
                if (t == other.tag)
                {
                    onInteract.Invoke();
                    break;
                }
            }
        }
    }
}
