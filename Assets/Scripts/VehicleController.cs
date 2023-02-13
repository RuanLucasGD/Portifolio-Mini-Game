using UnityEngine;

namespace Game
{
    public class VehicleController : MonoBehaviour
    {
    [System.Serializable]
        public class VehicleInputs
        {
            public string Horizontal;
            public string Vertical;
            public KeyCode brakeKey;
        }

        public VehicleInputs Inputs;

        public MMV.MMV_Vehicle Vehicle { get; private set; }

        public float HorizontalInput => Input.GetAxis(Inputs.Horizontal);
        public float VerticalInput => Input.GetAxis(Inputs.Vertical);

        private void Awake()
        {
            Vehicle = GetComponent<MMV.MMV_Vehicle>();
        }

        void Update()
        {
            ControlVehicle();
        }

        private void ControlVehicle()
        {
            var _isBraking = false;

            _isBraking |= Input.GetKey(Inputs.brakeKey);
            _isBraking |= HorizontalInput == 0 && VerticalInput == 0;

            Vehicle.PlayerInputs(VerticalInput, HorizontalInput, _isBraking);
        }
    }
}
