using UnityEngine;
using UnityEngine.Events;

namespace MMV
{
    /// <summary>
    /// Weapon & turret control
    /// </summary>
    public class MMV_ShooterManager : MonoBehaviour
    {
        [System.Serializable]
        public class ShotEffect
        {
            [SerializeField] private string name;
            [SerializeField] private GameObject[] prefabs;
            [SerializeField] private bool asChild;
            [SerializeField] private float lifeTime;

            /// <summary>
            /// Name of the effect, optional
            /// </summary>
            /// <value></value>
            public string Name { get => name; set => name = value; }

            /// <summary>
            /// Objects to spawn on shot, smoke, fire... etc
            /// </summary>
            /// <value></value>
            public GameObject[] Prefabs { get => prefabs; set => prefabs = value; }

            /// <summary>
            /// Life time of earch effect
            /// </summary>
            /// <value></value>
            public float LifeTime { get => lifeTime; set => lifeTime = value; }

            /// <summary>
            /// Set effect as child of projectile spawner when create effect
            /// </summary>
            /// <value></value>
            public bool AsChild { get => asChild; set => asChild = value; }

            public ShotEffect()
            {
                LifeTime = 20;
            }
        }

        /// <summary>
        /// Properties relative to shoot control
        /// </summary>
        [System.Serializable]
        public class WeaponShoot
        {
            [SerializeField] private bool enabled;
            [Space]
            [SerializeField] private Transform spawner;
            [SerializeField] private MMV_Projectile projectile;
            [SerializeField] private float projectileLifeTime;
            [Space]
            [SerializeField] private float shotInterval;
            [SerializeField] private float reloadTime;
            [Space]
            [SerializeField] private int ammunationBySlot;
            [SerializeField] private int ammunationSlots;
            [SerializeField] private bool infinityAmmunation;
            [Space]
            [SerializeField] private UnityEvent onStartReload;
            [SerializeField] private UnityEvent onReloaded;
            [SerializeField] private UnityEvent onShot;
            [SerializeField] private UnityEvent<MMV_Projectile.ProjectileHitInfo> onProjectileHit;
            [SerializeField] private UnityEvent onDestrotyed;
            [Space]
            [SerializeField] private AudioSource shootAudioSource;
            [SerializeField] private AudioClip shootAudioClip;
            [Space]
            public ShotEffect[] shotEffects;

            /// <summary>
            /// Set enable ou disable shoot
            /// </summary>
            /// <value></value>
            public bool Enabled { get => enabled; set => enabled = value; }

            /// <summary>
            /// Transform used to store the position and direction in which the weapon projectile should be instantiated
            /// </summary>
            /// <value></value>
            public Transform Spawner { get => spawner; set => spawner = value; }

            /// <summary>
            /// Prefab used as weapon ammunation
            /// </summary>
            /// <value></value>
            public MMV_Projectile Projectile { get => projectile; set => projectile = value; }

            /// <summary>
            /// Projectile lifetime when instantiated, after which it will be destroyed
            /// </summary>
            /// <returns></returns>
            public float ProjectileLifeTime { get => projectileLifeTime; set => projectileLifeTime = Mathf.Max(1, Mathf.Abs(value)); }

            /// <summary>
            /// Time interval for each shot
            /// </summary>
            /// <returns></returns>
            public float ShotInterval { get => shotInterval; set => shotInterval = Mathf.Max(0.01f, Mathf.Abs(value)); }

            /// <summary>
            /// Time it takes to reload when all ammo runs out
            /// </summary>
            /// <returns></returns>
            public float ReloadTime { get => reloadTime; set => reloadTime = Mathf.Max(0.01f, Mathf.Abs(value)); }

            /// <summary>
            /// Amount of ammo for each shot sequence
            /// </summary>
            /// <returns></returns>
            public int AmmunationBySlot { get => ammunationBySlot; set => ammunationBySlot = Mathf.Max(1, Mathf.Abs(value)); }

            /// <summary>
            /// Number of times the weapon can be reloaded
            /// </summary>
            /// <value></value>
            public int AmmunationSlots { get => ammunationSlots; set => ammunationSlots = value; }

            /// <summary>
            /// If ammo should never run out, leave it enabled
            /// </summary>
            /// <value></value>
            public bool InfinityAmmunation { get => infinityAmmunation; set => infinityAmmunation = value; }

            /// <summary>
            /// audiosource responsible for playing the shot sound
            /// </summary>
            /// <value></value>
            public AudioSource ShootAudioSource { get => shootAudioSource; set => shootAudioSource = value; }

            /// <summary>
            /// Shot audio clip
            /// </summary>
            /// <value></value>
            public AudioClip ShootAudioClip { get => shootAudioClip; set => shootAudioClip = value; }

            /// <summary>
            /// Called when weapon spawn a projectile
            /// </summary>
            /// <value></value>
            public UnityEvent OnShot { get => onShot; set => onShot = value; }

            /// <summary>
            /// Called when te projectile of the weapon hits on object
            /// </summary>
            /// <value></value>
            public UnityEvent<MMV_Projectile.ProjectileHitInfo> OnProjectileHit { get => onProjectileHit; set => onProjectileHit = value; }

            /// <summary>
            /// Execute actions when projectile's destroyed
            /// </summary>
            /// <value></value>
            public UnityEvent OnProjectileDestroyed { get => onDestrotyed; set => onDestrotyed = value; }

            /// <summary>
            /// Called when weapon is reloaded
            /// </summary>
            /// <value></value>
            public UnityEvent OnReloaded { get => onReloaded; set => onReloaded = value; }

            /// <summary>
            /// Called when weapon start reload
            /// </summary>
            /// <value></value>
            public UnityEvent OnStartReload { get => onStartReload; set => onStartReload = value; }

            public WeaponShoot()
            {
                Enabled = true;
                ProjectileLifeTime = 10f;
                ShotInterval = 0.2f;
                ReloadTime = 2f;
                AmmunationBySlot = 60;
                AmmunationSlots = 2;
                InfinityAmmunation = false;
            }
        }

        /// <summary>
        /// Control weapon movimentation
        /// </summary>
        [System.Serializable]
        public class WeaponRotation
        {
            [SerializeField] private bool turnWeaponEnabled;
            [Space]
            [SerializeField] private Transform horizontalTransform;
            [SerializeField] private Transform verticalTransform;
            [SerializeField] private float turnSpeedVertical;
            [SerializeField] private float turnSpeedHorizontal;
            [Space]
            [Range(0, 180), SerializeField] private float minVerticalAngle;
            [Range(0, 180), SerializeField] private float maxVerticalAngle;
            [Range(0, 180), SerializeField] private float maxHorizontalAngle;

            /// <summary>
            /// When enabled the weapon will always aim at the target position
            /// </summary>
            /// <value></value>
            public bool TurnWeaponEnabled { get => turnWeaponEnabled; set => turnWeaponEnabled = value; }

            /// <summary>
            /// Used to rotate the weapon horizontally towards the target
            /// </summary>
            /// <value></value>
            public Transform HorizontalTransform { get => horizontalTransform; set => horizontalTransform = value; }

            /// <summary>
            /// Used to rotate the weapon vertically towards the target
            /// </summary>
            /// <value></value>
            public Transform VerticalTransform { get => verticalTransform; set => verticalTransform = value; }

            /// <summary>
            /// Horizontal rotation speed
            /// </summary>
            /// <value></value>
            public float TurnSpeedVertical { get => turnSpeedVertical; set => turnSpeedVertical = value; }

            /// <summary>
            /// Vertical rotation speed
            /// </summary>
            /// <value></value>
            public float TurnSpeedHorizontal { get => turnSpeedHorizontal; set => turnSpeedHorizontal = value; }

            /// <summary>
            /// Max vertical angle down
            /// </summary>
            /// <returns></returns>
            public float MinVerticalAngle { get => minVerticalAngle; set => minVerticalAngle = Mathf.Clamp(value, 0, 180); }

            /// <summary>
            /// Weapon Depression / Maximum angle of weapon facing down
            /// </summary>
            /// <returns></returns>
            public float MaxVerticalAngle { get => maxVerticalAngle; set => maxVerticalAngle = Mathf.Clamp(value, 0, 180); }

            /// <summary>
            /// Maximum weapon angle facing up
            /// </summary>
            /// <returns></returns>
            public float MaxHorizontalAngle { get => maxHorizontalAngle; set => maxHorizontalAngle = Mathf.Clamp(value, 0, 180); }

            public WeaponRotation()
            {
                TurnWeaponEnabled = true;
                TurnSpeedVertical = 40;
                TurnSpeedHorizontal = 40;

                MinVerticalAngle = 7;
                MaxVerticalAngle = 45;
                MaxHorizontalAngle = 180;
            }
        }

        /// <summary>
        /// Shot effect applied to vehicle
        /// </summary>
        [System.Serializable]
        public class Recoil
        {
            [SerializeField] private float recoilForce;
            [SerializeField] private Rigidbody vehicle;
            [Space]
            [SerializeField] private float animationLength;
            [SerializeField] private AnimationCurve animation;
            [SerializeField] private Transform recoilTransform;

            /// <summary>
            /// Force of the shot applied in the vehicle
            /// </summary>
            /// <value></value>
            public float RecoilForce { get => recoilForce; set => recoilForce = value; }

            /// <summary>
            /// Vehicle receiving the force of the shot
            /// </summary>
            /// <value></value>
            public Rigidbody Vehicle { get => vehicle; set => vehicle = value; }

            /// <summary>
            /// Animation Length
            /// </summary>
            /// <value></value>
            public float AnimationLength { get => animationLength; set => animationLength = value; }

            /// <summary>
            /// Animation recoil curve
            /// </summary>
            /// <value></value>
            public AnimationCurve Animation { get => animation; set => animation = value; }

            /// <summary>
            /// Object that will receive the recoil animation
            /// </summary>
            /// <value></value>
            public Transform AnimatedPart { get => recoilTransform; set => recoilTransform = value; }

            public Recoil()
            {
                RecoilForce = 500;
                AnimationLength = 1f;
                Animation = new AnimationCurve(new Keyframe(0.0f, 0.0f),
                                               new Keyframe(0.1f, 1.0f),
                                               new Keyframe(0.4f, 0.3f),
                                               new Keyframe(1.0f, 0.0f));
            }
        }

        [SerializeField] private WeaponShoot weapon;
        [SerializeField] private WeaponRotation rotation;
        [SerializeField] private Recoil recoil;

        private float currentRecoilTime;
        private float currentReloadTime;
        private float currentShotIntervalTime;
        private int ammunationAmount;
        private int currentAmmunationSlots;
        private Vector3 targetPosition;
        private Vector3 recoilPartPositionOffset;

        /// <summary>
        /// Properties relative to shoot control
        /// </summary>
        public WeaponShoot Weapon { get => weapon; set => weapon = value; }

        /// <summary>
        /// Control weapon movimentation
        /// </summary>
        public WeaponRotation Rotation { get => rotation; set => rotation = value; }

        /// <summary>
        /// Shot effect applied to vehicle
        /// </summary>
        public Recoil WeaponRecoil { get => recoil; set => recoil = value; }

        /// <summary>
        /// Position of the world the weapon should aim at
        /// </summary>
        /// <value></value>
        public Vector3 TargetPosition { get => targetPosition; set => targetPosition = value; }

        /// <summary>
        /// Number of shots that can be fired
        /// </summary>
        /// <value></value>
        public int AmmunationAmount { get => ammunationAmount; set => ammunationAmount = value; }

        /// <summary>
        /// Number of times you can reload the weapon
        /// </summary>
        /// <value></value>
        public int AmmunationSlotsAmount { get => currentAmmunationSlots; set => currentAmmunationSlots = Mathf.Clamp(value, 0, Weapon.AmmunationSlots); }

        /// <summary>
        /// Returns true if the weapon has no more ammo
        /// </summary>
        public bool AmmunationEmpty => AmmunationAmount == 0;

        /// <summary>
        /// Returns true if the weapon is reloading ammunition
        /// </summary>
        public bool IsReloading => CurrentReloadTime < Weapon.ReloadTime;

        /// <summary>
        /// Return progress of reload, 0 to 1
        /// </summary>
        /// <returns></returns>
        public float ReloadProgress => Mathf.Clamp01(CurrentReloadTime / Weapon.ReloadTime);

        /// <summary>
        /// Return the reaming time to finish reload weapon
        /// </summary>
        public float ReamingReloadTime => Mathf.Max(Weapon.ReloadTime - CurrentReloadTime, 0);

        /// <summary>
        /// Current reload time
        /// </summary>
        /// <value></value>
        public float CurrentReloadTime { get => currentReloadTime; private set => currentReloadTime = value; }

        /// <summary>
        /// Returns true if the weapon is preparing for the next shot.
        /// </summary>
        public bool IsOnShotInterval => currentShotIntervalTime < Weapon.ShotInterval;

        /// <summary>
        /// Returns progress of reaming time to shot
        /// </summary>
        /// <returns></returns>
        public float ShotIntervalProgress => Mathf.Clamp01(currentShotIntervalTime / Weapon.ShotInterval);

        /// <summary>
        /// Returns reaming time to next shot
        /// </summary>
        /// <returns></returns>
        public float ReamingShotInterval => Mathf.Max(Weapon.ShotInterval - currentShotIntervalTime, 0);

        /// <summary>
        /// Returns true if the weapon can fire
        /// </summary>
        public bool CanShot => !IsReloading && !IsOnShotInterval && !AmmunationEmpty && Weapon.Enabled;

        void Start()
        {
            // load weapon ammunation
            currentAmmunationSlots = Weapon.AmmunationSlots;
            if (currentAmmunationSlots > 0)
            {
                AmmunationAmount = Weapon.AmmunationBySlot;
            }

            currentRecoilTime = recoil.AnimationLength;
            CurrentReloadTime = Weapon.ReloadTime;
            currentShotIntervalTime = Weapon.ShotInterval;

            if (recoil.AnimatedPart)
            {
                recoilPartPositionOffset = recoil.AnimatedPart.localPosition;
            }
        }

        void Update()
        {
            if (IsReloading)
            {
                if (AmmunationSlotsAmount > 0)
                {
                    CurrentReloadTime += Time.deltaTime;
                }

                if (!IsReloading)
                {
                    ReloadWeapon();
                }
            }

            if (IsOnShotInterval)
            {
                currentShotIntervalTime += Time.deltaTime;
            }

            UpdateRecoilAnimation();
        }

        private void FixedUpdate()
        {
            TurnWeapon(Time.fixedDeltaTime);
        }

        private void TurnWeapon(float deltaTime)
        {
            if (!Rotation.TurnWeaponEnabled)
            {
                return;
            }

            // turn horizontally to target position 
            if (Rotation.HorizontalTransform)
            {
                var _horizontalParent = Rotation.HorizontalTransform.parent;

                if (_horizontalParent)
                {
                    var _target = _horizontalParent.InverseTransformPoint(TargetPosition);
                    var _directionToTarget = _target - Rotation.HorizontalTransform.localPosition;
                    var _lookAt = Quaternion.LookRotation(_directionToTarget);

                    var _turnSpeed = Rotation.TurnSpeedHorizontal * deltaTime;
                    var _transitionRotation = Quaternion.RotateTowards(Rotation.HorizontalTransform.localRotation, _lookAt, _turnSpeed);
                    var _clampedHorizontalAngle = ClampEulerAngle(_transitionRotation.eulerAngles.y, Rotation.MaxHorizontalAngle, Rotation.MaxHorizontalAngle);

                    _transitionRotation = Quaternion.Euler(0, _clampedHorizontalAngle, 0);
                    Rotation.HorizontalTransform.localRotation = _transitionRotation;
                }
            }

            // turn vertically to target position 
            if (Rotation.VerticalTransform)
            {
                var _verticalParent = Rotation.VerticalTransform.parent;

                if (_verticalParent)
                {
                    var _target = _verticalParent.InverseTransformPoint(TargetPosition);
                    var _directionToTarget = _target - Rotation.VerticalTransform.localPosition;
                    var _lookAt = Quaternion.LookRotation(_directionToTarget);

                    var _turnSpeed = Rotation.TurnSpeedVertical * deltaTime;
                    var _transitionRotation = Quaternion.RotateTowards(Rotation.VerticalTransform.localRotation, _lookAt, _turnSpeed);
                    var _clampedVerticalAngle = ClampEulerAngle(_transitionRotation.eulerAngles.x, Rotation.MaxHorizontalAngle, Rotation.MinVerticalAngle);

                    _transitionRotation = Quaternion.Euler(_clampedVerticalAngle, 0, 0);
                    Rotation.VerticalTransform.localRotation = _transitionRotation;
                }
            }
        }

        private float ClampEulerAngle(float currentAngle, float min, float max)
        {
            max = Mathf.Abs(max);
            min = Mathf.Abs(min);

            if (currentAngle < 180)
            {
                currentAngle = Mathf.Clamp(currentAngle, 0, max);
            }
            else
            {
                currentAngle = Mathf.Clamp(currentAngle, 360 - min, 360);
            }

            return currentAngle;
        }

        private void UpdateRecoilAnimation()
        {
            if (recoil.AnimatedPart)
            {
                if (currentRecoilTime < recoil.AnimationLength)
                {
                    currentRecoilTime += Time.deltaTime;

                    if (currentRecoilTime > recoil.AnimationLength)
                    {
                        currentRecoilTime = recoil.AnimationLength;
                    }

                    var _offsetAnimation = recoil.Animation.Evaluate(currentRecoilTime / recoil.AnimationLength);
                    var _newPosition = recoilPartPositionOffset + -(Vector3.forward * _offsetAnimation);
                    recoil.AnimatedPart.localPosition = _newPosition;
                }
            }
        }

        private void ApplyRecoil(WeaponShoot weapon)
        {
            currentRecoilTime = 0;

            if (WeaponRecoil.RecoilForce > 0)
            {
                if (WeaponRecoil.Vehicle && weapon.Spawner)
                {
                    WeaponRecoil.Vehicle.AddForceAtPosition(-weapon.Spawner.forward * WeaponRecoil.RecoilForce, weapon.Spawner.position, ForceMode.Impulse);
                }
            }
        }

        private void SpawnShotEffects()
        {
            foreach (var e in Weapon.shotEffects)
            {
                foreach (var p in e.Prefabs)
                {
                    var _obj = Instantiate(p, Weapon.Spawner.position, Weapon.Spawner.rotation);

                    if (e.AsChild)
                    {
                        _obj.transform.parent = Weapon.Spawner;
                    }

                    Destroy(_obj, e.LifeTime);
                }
            }
        }

        private void ReloadWeapon()
        {
            if (AmmunationSlotsAmount > 0)
            {
                AmmunationAmount = Weapon.AmmunationBySlot;

                if (!Weapon.InfinityAmmunation)
                {
                    AmmunationSlotsAmount--;
                }

                Weapon.OnReloaded.Invoke();
            }
        }

        /// <summary>
        /// Shoot
        /// </summary>
        public void Shoot()
        {
            if (!Weapon.Spawner || !enabled)
            {
                return;
            }

            if (CanShot)
            {
                currentShotIntervalTime = 0f;

                var _spawnPosition = Weapon.Spawner.position;
                var _spawnRotation = Weapon.Spawner.rotation;
                var _lifeTime = Weapon.ProjectileLifeTime;
                var _infinityAmmunation = Weapon.InfinityAmmunation;
                var _projectile = Instantiate(Weapon.Projectile.gameObject, _spawnPosition, _spawnRotation);
                var _audioSource = Weapon.ShootAudioSource;
                var _audioClip = Weapon.ShootAudioClip;

                var _projectileComponent = _projectile.GetComponent<MMV_Projectile>();

                _projectileComponent.OnHit.AddListener((hitInfo) => Weapon.OnProjectileHit.Invoke(hitInfo));
                _projectileComponent.OnDestroyed.AddListener(() => Weapon.OnProjectileDestroyed.Invoke());

                Destroy(_projectile.gameObject, _lifeTime);
                ApplyRecoil(weapon);
                SpawnShotEffects();

                if (_audioSource && _audioClip)
                {
                    _audioSource.PlayOneShot(_audioClip);
                }

                AmmunationAmount--;

                Weapon.OnShot.Invoke();
            }
            else
            {
                if (AmmunationEmpty && !IsReloading)
                {
                    CurrentReloadTime = 0f;

                    if (AmmunationSlotsAmount > 0)
                    {
                        Weapon.OnStartReload.Invoke();
                    }
                }
            }
        }

        public void RestoreAllAmmunationSlots()
        {
            currentAmmunationSlots = Weapon.AmmunationSlots;
        }
    }
}