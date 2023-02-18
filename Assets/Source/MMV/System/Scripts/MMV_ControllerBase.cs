using UnityEngine;
using System.Collections;

namespace MMV
{
    /// <summary>
    /// Base for any controller, whether camera controller or vehicle controller
    /// </summary>
    public class MMV_ControllerBase : MonoBehaviour
    {
        [System.NonSerialized] private MMV_Vehicle vehicle;

        public MMV_Vehicle Vehicle
        {
            get
            {
                if (!vehicle)
                {
                    vehicle = GetComponent<MMV_Vehicle>();

                    if (!vehicle)
                    {
                        Debug.LogError($"The gameObject {name} not have a Vehicle component");
                    }
                }

                return vehicle;
            }

            set => vehicle = value;
        }
    }
}
