using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MMV
{
    /// <summary>
    /// Standart vehicle camera controller
    /// </summary>
    public class MMV_StandardCameraController : MonoBehaviour
    {
        /// <summary>
        /// Camera configuration
        /// </summary>
        [System.Serializable]
        public class CameraNode
        {
            [SerializeField] private string name;
            [SerializeField] private Camera camera;
            [Space]
            [SerializeField] private bool alignOnVehicle;
            [SerializeField] private bool staticPosition;
            [SerializeField] private float offset;
            [SerializeField] private float height;
            [Space]
            [SerializeField] private float turnSpeedMultiplier;
            [Space]
            [SerializeField] private bool checkCameraCollision;
            [Space]
            [SerializeField] private float maxHorizontalAngle;
            [SerializeField] private float minVerticalAngle;
            [SerializeField] private float maxVerticalAngle;
            [Space]
            [SerializeField] private GameObject cameraHud;
            [SerializeField] private AudioSource cameraAudio;
            [Space]
            [SerializeField] private MMV_CameraShakeSettings cameraShakeSettings;
            [Space]
            [SerializeField] private UnityEvent onSetThisCamera;

            /// <summary>
            /// Reference name of the camera node, optional property
            /// </summary>
            /// <value></value>
            public string Name { get => name; set => name = value; }

            /// <summary>
            /// Camera that will be controlled
            /// </summary>
            /// <value></value>
            public Camera Camera { get => camera; set => camera = value; }

            /// <summary>
            /// When enabled, camera not move
            /// </summary>
            /// <value></value>
            public bool StaticPosition { get => staticPosition; set => staticPosition = value; }

            /// <summary>
            /// Distance from the camera to the vehicle that will be followed
            /// </summary>
            /// <value></value>
            public float Offset { get => offset; set => offset = Mathf.Max(Mathf.Abs(value), 1); }

            /// <summary>
            /// Height of camera relative to the vehicle that will be followed
            /// </summary>
            /// <value></value>
            public float Height { get => height; set => height = Mathf.Abs(value); }

            /// <summary>
            /// Decrease camera turn speed
            /// </summary>
            /// <value></value>
            public float TurnSpeedMultiplier { get => turnSpeedMultiplier; set => turnSpeedMultiplier = value; }

            /// <summary>
            /// When enabled, the camera avoid obstacles that are in front 
            /// </summary>
            /// <value></value>
            public bool CheckCameraCollision { get => checkCameraCollision; set => checkCameraCollision = value; }

            /// <summary>
            /// Works only if the camera are child of some object, limitate camera max horizontal angle
            /// </summary>
            /// <value></value>
            public float MaxHorizontalAngle { get => maxHorizontalAngle; set => maxHorizontalAngle = Mathf.Abs(value); }

            /// <summary>
            /// Works only if the camera are child of some object, limitate camera max down vertical angle
            /// </summary>
            /// <value></value>
            public float MinVerticalAngle { get => minVerticalAngle; set => minVerticalAngle = Mathf.Abs(value); }

            /// <summary>
            /// Works only if the camera are child of some object, limitate camera max up vertical angle
            /// </summary>
            /// <value></value>
            public float MaxVerticalAngle { get => maxVerticalAngle; set => maxVerticalAngle = Mathf.Abs(value); }

            /// <summary>
            /// When enable, the camera align with the vehicle Up direction, works only if the camera is not child of the v
            /// </summary>
            /// <value></value>
            public bool AlignOnVehicle { get => alignOnVehicle; set => alignOnVehicle = value; }

            /// <summary>
            /// Called when player change to this camera
            /// </summary>
            /// <value></value>
            public UnityEvent OnSetThisCamera { get => onSetThisCamera; set => onSetThisCamera = value; }

            /// <summary>
            /// UI Camera Hud
            /// </summary>
            /// <value></value>
            public GameObject CameraHud { get => cameraHud; set => cameraHud = value; }

            /// <summary>
            /// Local camera position in the vehicle
            /// </summary>
            /// <value></value>
            public Vector3 DefaultCameraLocalPosition { get; set; }

            /// <summary>
            /// Camera shake behaviour
            /// </summary>
            /// <value></value>
            public MMV_CameraShakeSettings CameraShakeSettings { get => cameraShakeSettings; set => cameraShakeSettings = value; }

            /// <summary>
            /// Audio of the camera
            /// </summary>
            /// <value></value>
            public AudioSource CameraAudio { get => cameraAudio; set => cameraAudio = value; }

            public CameraNode()
            {
                StaticPosition = false;
                Offset = 15;
                Height = 7;
                CheckCameraCollision = true;
                MaxHorizontalAngle = 180;
                MinVerticalAngle = 30;
                MaxVerticalAngle = 80;
                TurnSpeedMultiplier = 1;
            }
        }

        /// <summary>
        /// Camera rotation velocity
        /// </summary>
        [System.Serializable]
        public class Movimentation
        {
            [SerializeField] private float turnSpeed;
            [SerializeField] private float smoothRotation;

            /// <summary>
            /// Camera rotation speed
            /// </summary>
            /// <returns></returns>
            public float TurnSpeed { get => turnSpeed; set => turnSpeed = Mathf.Abs(value); }

            /// <summary>
            /// Smooth rotation
            /// </summary>
            /// <returns></returns>
            public float SmoothRotation { get => smoothRotation; set => smoothRotation = Mathf.Abs(value); }

            public Movimentation()
            {
                TurnSpeed = 1;
                SmoothRotation = 10;
            }
        }

        /// <summary>
        /// Detect walls and obstacles
        /// </summary>
        [System.Serializable]
        public class CameraCollision
        {
            [SerializeField] private LayerMask layer;
            [SerializeField] private bool ignoreVehicle;

            /// <summary>
            /// Layer of obstacles to camera avoid
            /// </summary>
            /// <value></value>
            public LayerMask Layer { get => layer; set => layer = value; }

            /// <summary>
            /// Ignore vehicle colliders when is on front of camera
            /// </summary>
            /// <value></value>
            public bool IgnoreVehicle { get => ignoreVehicle; set => ignoreVehicle = value; }

            public CameraCollision()
            {
                IgnoreVehicle = true;
                Layer = 1;
            }
        }

        /// <summary>
        /// Vehicle weapons that will be controlled by camera crosshairs
        /// </summary>
        [System.Serializable]
        public class VehicleWeapons
        {
            [SerializeField] private bool controlWeapons;
            [SerializeField] private LayerMask targetsLayer;
            [SerializeField] private MMV_ShooterManager[] weapons;

            /// <summary>
            /// When active, the camera control weapons of vehicle to aim on front of camera
            /// </summary>
            public bool ControlWeapons { get => controlWeapons; set => controlWeapons = value; }

            /// <summary>
            /// Layer of objects that weapons should target
            /// </summary>
            /// <value></value>
            public LayerMask TargetsLayer { get => targetsLayer; set => targetsLayer = value; }

            /// <summary>
            /// Weapons of vehicle 
            /// </summary>
            /// <value></value>
            public MMV_ShooterManager[] Weapons { get => weapons; set => weapons = value; }

            public VehicleWeapons()
            {
                ControlWeapons = true;
                TargetsLayer = 1;
            }
        }

        [SerializeField] private bool disableMouseOnStart;
        [SerializeField] private bool controlsEnabled;
        [SerializeField] private MMV.MMV_Vehicle vehicle;
        [SerializeField] private MMV_CameraInputsSettings inputs;
        [SerializeField] private UnityEvent<int> onChangeCamera;
        [SerializeField] private UnityEvent<bool> onSetEnabled;
        [SerializeField] private UnityEvent<bool> onSetControlsEnabled;
        [SerializeField] private Movimentation cameraMovimentation;
        [SerializeField] private CameraCollision detectCollision;
        [SerializeField] private CameraNode[] cameras;
        [SerializeField] private VehicleWeapons weapons;

        private int currentCameraIndex;
        private float cameraShakeTime;
        private Vector3 cameraShakeOffset;

        private Quaternion currentCameraShakeRotation;
        private Quaternion currentRotation;

        private float randomSeedH;
        private float randomSeedV;

        private List<Collider> vehicleColliders;

        public const float CAMERA_COLLISION_HIT_OFFSET = 0.1f;
        public const int DEFAULT_WEAPONS_AIM_DISTANCE = 10000;

        /// <summary>
        /// Current used camera
        /// </summary>
        public CameraNode SelectedCamera => Cameras[currentCameraIndex];

        /// <summary>
        /// Get index of current camera
        /// </summary>
        public int CurrentCameraIndex => currentCameraIndex;

        /// <summary>
        /// Direction of camera aim
        /// </summary>
        public Vector3 CameraDirectionForward => CameraRotation * Vector3.forward;

        /// <summary>
        /// Up direction of camera
        /// </summary>
        public Vector3 CameraDirectionUp => CameraRotation * Vector3.up;

        /// <summary>
        /// Camera target rotation in world position
        /// </summary>
        /// <returns></returns>
        public Quaternion CameraRotation
        {
            get
            {
                var _rotation = currentRotation;
                if (SelectedCamera.AlignOnVehicle || SelectedCamera.Camera.transform.parent)
                {
                    _rotation *= Quaternion.Euler(Vehicle.transform.eulerAngles.x, 0, Vehicle.transform.eulerAngles.z);
                }

                return _rotation;
            }
        }

        /// <summary>
        /// The vehicle target to camera follow
        /// </summary>
        /// <value></value>
        public MMV_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        /// <summary>
        /// Player inputs controls
        /// </summary>
        /// <value></value>
        public MMV_CameraInputsSettings Inputs { get => inputs; set => inputs = value; }

        /// <summary>
        /// Detect obstacles in front of camera
        /// </summary>
        /// <value></value>
        public CameraCollision DetectCollision { get => detectCollision; set => detectCollision = value; }

        /// <summary>
        /// All cameras to control
        /// </summary>
        /// <value></value>
        public CameraNode[] Cameras { get => cameras; set => cameras = value; }

        /// <summary>
        /// Weapons of vehicle to control
        /// </summary>
        /// <value></value>
        public VehicleWeapons Weapons { get => weapons; set => weapons = value; }

        /// <summary>
        /// Vehicle colliders to ignore when the vehicle is on front of camera
        /// </summary>
        /// <value></value>
        public List<Collider> IgnoreColliders
        {
            get
            {
                if (vehicleColliders == null || vehicleColliders.Count == 0)
                {
                    vehicleColliders = new List<Collider>(Vehicle.GetComponentsInChildren<Collider>());
                }

                return vehicleColliders;
            }
            set => vehicleColliders = value;
        }

        /// <summary>
        /// Camera movimentation and rotation
        /// </summary>
        /// <value></value>
        public Movimentation CameraMovimentation { get => cameraMovimentation; set => cameraMovimentation = value; }

        /// <summary>
        /// When game start, set cursor enabled or disabled
        /// </summary>
        /// <value></value>
        public bool DisableMouseOnStart { get => disableMouseOnStart; set => disableMouseOnStart = value; }

        /// <summary>
        /// Set enabled camera controls with mouse, keyboard or gamepad
        /// </summary>
        /// <value></value>
        public bool ControlsEnabled
        {
            get => controlsEnabled;
            set
            {
                if (value != controlsEnabled)
                {
                    controlsEnabled = value;

                    if (OnSetControlsEnabled != null)
                    {
                        OnSetControlsEnabled.Invoke(value);
                    }
                }
            }
        }

        /// <summary>
        /// Called when current camera is changed
        /// </summary>
        /// <value></value>
        public UnityEvent<int> OnChangeCamera { get => onChangeCamera; set => onChangeCamera = value; }

        /// <summary>
        /// Called when the component is disabled or enabled
        /// </summary>
        /// <value></value>
        public UnityEvent<bool> OnSetEnabled { get => onSetEnabled; set => onSetEnabled = value; }

        /// <summary>
        /// Called when camera controls is disabled or disabled
        /// </summary>
        /// <value></value>
        public UnityEvent<bool> OnSetControlsEnabled { get => onSetControlsEnabled; set => onSetControlsEnabled = value; }

        public MMV_StandardCameraController()
        {
            ControlsEnabled = true;
            DisableMouseOnStart = true;

            Cameras = new CameraNode[1] { new CameraNode() };
            Cameras[0].Name = "Main Camera";

            currentCameraShakeRotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            // enable current camera on enable camera controller
            foreach (var c in Cameras)
            {
                var _cameraEnabled = c.Camera == SelectedCamera.Camera;
                var _audioListener = c.Camera.GetComponentInChildren<AudioListener>();

                c.Camera.enabled = _cameraEnabled;

                if (c.CameraHud)
                {
                    c.CameraHud.SetActive(_cameraEnabled);
                }

                if (_audioListener)
                {
                    _audioListener.enabled = _cameraEnabled;
                }

                if (c.CameraHud)
                {
                    c.CameraHud.SetActive(_cameraEnabled);
                }

                if (c.CameraAudio)
                {
                    c.CameraAudio.enabled = true;

                    if (_cameraEnabled)
                    {
                        c.CameraAudio.Play();
                    }
                }
            }

            OnSetEnabled.Invoke(true);
        }

        private void OnDisable()
        {
            // disable all cameras on disable camera controller
            foreach (var c in Cameras)
            {
                if (c.Camera)
                {
                    c.Camera.enabled = false;
                    var _audioListener = c.Camera.GetComponentInChildren<AudioListener>();

                    if (_audioListener)
                    {
                        _audioListener.enabled = false;
                    }

                    if (c.CameraHud)
                    {
                        c.CameraHud.SetActive(false);
                    }

                    if (c.CameraAudio)
                    {
                        c.CameraAudio.Stop();
                    }
                }
            }

            OnSetEnabled.Invoke(false);
        }

        void Awake()
        {

        }

        void Start()
        {

            foreach (var c in cameras)
            {
                c.DefaultCameraLocalPosition = c.Camera.transform.localPosition;
            }

            ResetCameraShake();

            SetCursorActive(!DisableMouseOnStart);
            SetCamera(0);
        }

        private void FixedUpdate()
        {
            ClampHorizontalCameraRotation();
            ClampVerticalCameraRotation();
            UpdateCameraRotation();
        }

        private void Update()
        {
            if (!SelectedCamera.StaticPosition)
            {
                SelectedCamera.Camera.transform.parent = null;
            }

            UpdateCameraShake();
            UpdateCameraPosition();


            if (Weapons.ControlWeapons)
            {
                UpdateWeaponsTargetPosition();
            }
        }

        private void LateUpdate()
        {
            if (ControlsEnabled)
            {
                ControlCamera(Inputs.VerticalAxis, Inputs.HorizontalAxis, Inputs.ChangeCamera);
            }
        }

        private void UpdateCameraShake()
        {
            var _shakeSettings = SelectedCamera.CameraShakeSettings;

            if (!_shakeSettings || !SelectedCamera.Camera)
            {
                return;
            }

            var _shakeLength = _shakeSettings.Length;
            var _shakeVelocity = _shakeSettings.ShakeVelocity;


            // calculate shake life time
            if (cameraShakeTime < _shakeLength)
            {
                cameraShakeTime += Time.deltaTime;
                cameraShakeTime = Mathf.Clamp(cameraShakeTime, 0, _shakeLength);
            }

            var _shakeTime = cameraShakeTime / _shakeLength;
            var _perlinSeedX = ((_shakeTime * _shakeVelocity) + randomSeedH);
            var _perlinSeedY = ((_shakeTime * _shakeVelocity) + randomSeedV);

            var _horizontalNoise = Mathf.PerlinNoise(_perlinSeedX, _perlinSeedX);
            var _verticalNoise = Mathf.PerlinNoise(_perlinSeedY, _perlinSeedY);

            // convert noise range (0 - 1) to (-1 - 1)
            _horizontalNoise = (_horizontalNoise * 2) - 1;
            _verticalNoise = (_verticalNoise * 2) - 1;

            var _currentShakeForce = Mathf.Clamp01(_shakeSettings.ShakeForce.Evaluate(_shakeTime));
            _verticalNoise *= _currentShakeForce;
            _horizontalNoise *= _currentShakeForce;

            // random horizontal and vertical rotation in radians
            var _horizontalCameraShakeRotation = Mathf.Deg2Rad * _horizontalNoise * _shakeSettings.MaxRotation;
            var _verticalCameraShakeRotation = Mathf.Deg2Rad * _verticalNoise * _shakeSettings.MaxRotation;

            // horizontal rotation
            var _shakeRotation = Quaternion.LookRotation(new Vector3(Mathf.Sin(_horizontalCameraShakeRotation), 0, Mathf.Cos(_horizontalCameraShakeRotation)));

            // vertical rotation
            _shakeRotation *= Quaternion.LookRotation(new Vector3(0, Mathf.Sin(_verticalCameraShakeRotation), Mathf.Cos(_verticalCameraShakeRotation)));

            currentCameraShakeRotation = _shakeRotation;

            var _cameraRight = Vector3.right * _shakeSettings.MaxOffsetX * _horizontalNoise;
            var _cameraUp = Vector3.up * _shakeSettings.MaxOffsetY * _verticalNoise;
            var _cameraBackward = Vector3.forward * _shakeSettings.MaxOffsetZ * Mathf.Abs(_verticalNoise);
            cameraShakeOffset = _cameraRight + _cameraBackward + _cameraUp;

            // avoid vector exception
            for (int i = 0; i < 3; i++)
            {
                if (float.IsNaN(cameraShakeOffset[i]) || float.IsInfinity(cameraShakeOffset[i])) cameraShakeOffset[i] = 0;
            }
        }

        private Vector3 CameraTargetLookAt()
        {
            var _up = Vector3.up;
            var _target = Vehicle.transform.position + (_up * SelectedCamera.Height);
            return _target;
        }

        private void CheckCameraCollision(ref Vector3 cameraPosition)
        {
            if (LineCast(CameraTargetLookAt(), cameraPosition, out var hit, DetectCollision.Layer))
            {
                var _hitDirection = hit.point - cameraPosition;
                _hitDirection /= _hitDirection.magnitude;
                cameraPosition = hit.point + (_hitDirection * CAMERA_COLLISION_HIT_OFFSET);
            }
        }

        private void ClampHorizontalCameraRotation()
        {
            var _parent = SelectedCamera.Camera.transform.parent;
            var _clampedRotation = currentRotation;

            if (_parent)
            {
                var _maxHorizontal = SelectedCamera.MaxHorizontalAngle;
                var _cameraForward = CameraDirectionForward;
                var _parentForward = Vector3.ProjectOnPlane(_parent.forward, CameraDirectionUp);
                var _angle = Vector3.Angle(_parentForward, _cameraForward);

                if (_angle > _maxHorizontal)
                {
                    var unclampedX = _clampedRotation.eulerAngles.x;
                    var cameraForwardOffset = (_cameraForward - _parent.forward);

                    cameraForwardOffset /= _angle;

                    cameraForwardOffset *= _maxHorizontal;
                    _clampedRotation = Quaternion.LookRotation(_parent.forward + cameraForwardOffset);
                    _clampedRotation = Quaternion.Euler(unclampedX, _clampedRotation.eulerAngles.y, _clampedRotation.eulerAngles.z);
                }

                currentRotation = _clampedRotation;
            }
        }

        private void ClampVerticalCameraRotation()
        {
            var _parent = SelectedCamera.Camera.transform.parent;
            var _clampedRotation = currentRotation;

            var _minVertical = SelectedCamera.MinVerticalAngle;
            var _maxVertical = SelectedCamera.MaxVerticalAngle;
            var _cameraForward = CameraDirectionForward;

            var _cameraTransform = SelectedCamera.Camera.transform;
            var _cameraLocalEulerX = currentRotation.eulerAngles.x;

            if (_cameraLocalEulerX < 180)
            {
                _cameraLocalEulerX = Mathf.Clamp(_cameraLocalEulerX, 0, Mathf.Abs(_maxVertical));
            }
            else
            {
                _cameraLocalEulerX = Mathf.Clamp(_cameraLocalEulerX, 360 - Mathf.Abs(_minVertical), 360);
            }

            currentRotation = Quaternion.Euler(_cameraLocalEulerX, currentRotation.eulerAngles.y, 0);
        }

        private void UpdateCameraRotation()
        {
            var _cameraTransform = SelectedCamera.Camera.transform;
            var _transitionSpeed = Mathf.Clamp01(Time.deltaTime * CameraMovimentation.SmoothRotation);

            var _rotation = CameraRotation * currentCameraShakeRotation;

            _cameraTransform.rotation = Quaternion.Lerp(_cameraTransform.rotation, _rotation, _transitionSpeed);
        }

        private void UpdateCameraPosition()
        {
            if (SelectedCamera.StaticPosition)
            {
                // apply camera shake even if it is static
                SelectedCamera.Camera.transform.localPosition = SelectedCamera.DefaultCameraLocalPosition + cameraShakeOffset;
                return;
            }

            var _height = SelectedCamera.Height;
            var _distance = SelectedCamera.Offset;
            var _checkCameraCollision = SelectedCamera.CheckCameraCollision && !SelectedCamera.StaticPosition;
            var _cameraTransform = SelectedCamera.Camera.transform;
            var _cameraForward = _cameraTransform.forward;

            var _cameraPosition = CameraTargetLookAt() + (-_cameraForward * _distance);

            if (_checkCameraCollision)
            {
                CheckCameraCollision(ref _cameraPosition);
            }

            _cameraTransform.position = _cameraPosition + _cameraTransform.TransformDirection(cameraShakeOffset);
        }

        private void UpdateWeaponsTargetPosition()
        {
            var _weaponsTargetPosition = GetWeaponsTargetPosition();
            foreach (var w in Weapons.Weapons)
            {
                if (w)
                {
                    w.TargetPosition = _weaponsTargetPosition;
                }
            }
        }

        private Vector3 GetWeaponsTargetPosition()
        {
            var _cameraTransform = SelectedCamera.Camera.transform;
            var _parent = _cameraTransform.parent;

            var _targetPos = _cameraTransform.position + (CameraDirectionForward * DEFAULT_WEAPONS_AIM_DISTANCE);

            if (LineCast(SelectedCamera.Camera.transform.position, _targetPos, out var hit, Weapons.TargetsLayer))
            {
                _targetPos = hit.point;
            }

            return _targetPos;
        }

        /// <summary>
        /// Control camera movimentation
        /// </summary>
        /// <param name="vertical">Rotate to up or down</param>
        /// <param name="horizontal">Rotate to left or right</param>
        /// <param name="isChangingCamera">Set to next camera</param>
        public void ControlCamera(float vertical, float horizontal, bool isChangingCamera = false)
        {
            var _fov = SelectedCamera.Camera.fieldOfView / 10;
            var _turnSpeed = CameraMovimentation.TurnSpeed * _fov;

            var _currentVerticalInput = vertical * _turnSpeed * SelectedCamera.TurnSpeedMultiplier;
            var _currentHorizontalInput = horizontal * _turnSpeed * SelectedCamera.TurnSpeedMultiplier;

            currentRotation = Quaternion.Euler(currentRotation.eulerAngles + new Vector3(_currentVerticalInput, _currentHorizontalInput, 0));

            if (isChangingCamera)
            {
                SetCamera(currentCameraIndex != Cameras.Length - 1 ? currentCameraIndex + 1 : 0);
            }
        }

        // Simple raycast that should ignore vehicle colliders
        public bool LineCast(Vector3 startPos, Vector3 endPos, out RaycastHit hit, LayerMask layer)
        {
            hit = new RaycastHit();
            var _rayDirection = endPos - startPos;
            var _hits = Physics.RaycastAll(startPos, _rayDirection.normalized, _rayDirection.magnitude, layer);

            foreach (var h in _hits)
            {
                bool isCollidingOnVehicle = false;
                foreach (var i in IgnoreColliders)
                {
                    if (h.transform.gameObject == i.gameObject)
                    {
                        isCollidingOnVehicle = true;
                        break;
                    }
                }

                if (!isCollidingOnVehicle)
                {
                    hit = h;
                    return true;
                }
            }


            return _hits.Length > 0;
        }

        /// <summary>
        /// Set current vehicle camera
        /// </summary>
        /// <param name="index">Index of the camera</param>
        public void SetCamera(int index)
        {
            index = Mathf.Clamp(index, 0, Cameras.Length - 1);

            if (CurrentCameraIndex != index)
            {
                OnChangeCamera.Invoke(currentCameraIndex);
            }

            currentCameraIndex = index;

            for (int i = 0; i < Cameras.Length; i++)
            {
                var _cameraEnabled = i == index;
                var _audioListener = Cameras[i].Camera.GetComponentInChildren<AudioListener>();

                Cameras[i].Camera.enabled = _cameraEnabled;

                if (Cameras[i].CameraHud)
                {
                    Cameras[i].CameraHud.SetActive(_cameraEnabled);
                }

                if (Cameras[i].CameraAudio)
                {
                    if (_cameraEnabled)
                    {
                        Cameras[i].CameraAudio.Play();
                    }
                    else
                    {
                        Cameras[i].CameraAudio.Stop();
                    }
                }

                if (_audioListener)
                {
                    _audioListener.enabled = _cameraEnabled;
                }

                if (_cameraEnabled)
                {
                    Cameras[i].OnSetThisCamera.Invoke();
                }
            }

            ResetCameraShake();


        }

        public void StartCameraShake()
        {
            cameraShakeTime = 0;
            // generate a random direction to rotate/move the camera in shake effect
            randomSeedH = Random.Range(0, 100);
            randomSeedV = Random.Range(0, 100);
        }

        public void ResetCameraShake()
        {
            cameraShakeTime = 1;
        }

        /// <summary>
        /// Change current camera to next
        /// </summary>
        public void SetNextCamera() => SetCamera(currentCameraIndex + 1);

        /// <summary>
        /// Change current camera to previous
        /// </summary>
        public void SetPreviousCamera() => SetCamera(currentCameraIndex - 1);

        /// <summary>
        /// Set cursor active
        /// </summary>
        /// <param name="active"></param>
        public void SetCursorActive(bool active)
        {
            if (active)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}