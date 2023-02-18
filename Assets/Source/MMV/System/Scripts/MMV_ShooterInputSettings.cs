using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Create asset file with player inputs configurations for weapons
    /// </summary>
    [CreateAssetMenu(fileName = "Shooter Input Settings", menuName = "MMV/Inputs/Shooter Input Settings", order = 0)]
    public class MMV_ShooterInputSettings : ScriptableObject
    {
        [SerializeField] private KeyCode[] shotkeys;

        public MMV_ShooterInputSettings()
        {
            Shotkeys = new KeyCode[1] { KeyCode.Mouse0 };
        }

        /// <summary>
        /// Check if the player is shooting
        /// </summary>
        /// <value></value>
        public virtual bool IsShooting
        {
            get
            {
                foreach (var k in Shotkeys)
                {
                    if (Input.GetKey(k))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Keys of the player uses to shoot
        /// </summary>
        /// <value></value>
        public KeyCode[] Shotkeys { get => shotkeys; set => shotkeys = value; }
    }
}