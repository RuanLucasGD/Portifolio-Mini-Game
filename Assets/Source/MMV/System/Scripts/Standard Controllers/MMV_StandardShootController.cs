using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Vehicle shooting controller for the player
    /// </summary>
    public class MMV_StandardShootController : MMV_ControllerBase
    {
        [SerializeField] private MMV_ShooterInputSettings inputs;

        //-----------------------------------------

        private MMV_ShooterManager shooterManager;

        /// <summary>
        /// User input to shoot
        /// </summary>
        /// <value></value>
        public MMV_ShooterInputSettings Inputs => inputs;

        public MMV_ShooterManager ShooterManager { get => shooterManager; set => shooterManager = value; }

        // Start is called before the first frame update
        void Awake()
        {
            ShooterManager = GetComponent<MMV_ShooterManager>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!ShooterManager)
            {
                return;
            }

            if (inputs.IsShooting)
            {
                ShooterManager.Shoot();
            }
        }
    }
}