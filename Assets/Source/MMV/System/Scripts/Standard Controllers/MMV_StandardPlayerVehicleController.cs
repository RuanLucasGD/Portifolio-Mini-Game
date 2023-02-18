using System;
using UnityEngine;

namespace MMV
{
    /// <summary>
    /// Control the movement of a vehicle using player inputs
    /// </summary>
    public class MMV_StandardPlayerVehicleController : MMV_ControllerBase
    {

        [SerializeField] private MMV_VehicleInputsSettings input;

        private MMV_Vehicle vehicle;

        /// <summary>
        /// Inputs to crontroll MBT vehicle
        /// </summary>
        /// <value></value>
        public MMV_VehicleInputsSettings Input { get => input; set => input = value; }

        private void Awake()
        {
            vehicle = GetComponentInChildren<MMV_Vehicle>();
        }

        void Update()
        {
            if (!vehicle || !vehicle.VehicleControlsEnabled)
            {
                return;
            }

            vehicle.PlayerInputs(input.AccelerationInput, input.SteerInput, input.BrakingInput);
        }
    }
}

