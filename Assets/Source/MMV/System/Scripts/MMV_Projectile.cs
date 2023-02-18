using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MMV
{
    /// <summary>
    /// Projectile that must be fired by the weapon of MMV vehicles
    /// </summary>
    public class MMV_Projectile : MonoBehaviour
    {
        /// <summary>
        /// Storage projectile hit informations
        /// </summary>
        [System.Serializable]
        public struct ProjectileHitInfo
        {
            public Vector3 HitPoint { set; get; }
            public GameObject GameObject { set; get; }
            public Collider Collider { set; get; }
            public Vector3 Normal { set; get; }
        }

        /// <summary>
        /// Effect to spawn when projectile hit
        /// </summary>
        [System.Serializable]
        public class HitSpawn
        {
            [SerializeField] private string name;
            [SerializeField] private GameObject[] prefabs;
            [SerializeField] private float lifeTime;

            /// <summary>
            /// Nome of the hit effect
            /// </summary>
            /// <value></value>
            public string Name { get => name; set => name = value; }

            /// <summary>
            /// Objects to spawn on projectile hit
            /// </summary>
            /// <value></value>
            public GameObject[] Prefabs { get => prefabs; set => prefabs = value; }

            /// <summary>
            /// Life time of each hit effect spawned
            /// </summary>
            /// <value></value>
            public float LifeTime { get => lifeTime; set => lifeTime = value; }

            public HitSpawn()
            {
                LifeTime = 20;
            }
        }

        [SerializeField] private float velocity;

        [Space]
        [SerializeField] private LayerMask hitlayer;
        [SerializeField] private UnityEvent<ProjectileHitInfo> onHit;
        [SerializeField] private HitSpawn[] spawnOnHit;

        [Space]
        [SerializeField] private bool addExplosionForce;
        [SerializeField] private LayerMask explosionInteractionLayer;
        [SerializeField] private float explosionForce;
        [SerializeField] private float explosionRadius;

        private Vector3 lastPosition;

        /// <summary>
        /// Projectile move velocity
        /// </summary>
        /// <value></value>
        public float Velocity { get => velocity; set => velocity = value; }

        /// <summary>
        /// Detect hit layer
        /// </summary>
        /// <value></value>
        public LayerMask HitLayer { get => hitlayer; set => hitlayer = value; }

        /// <summary>
        /// Execute actions when projectile hit
        /// </summary>
        /// <value></value>
        public UnityEvent<ProjectileHitInfo> OnHit { get => onHit; set => onHit = value; }

        /// <summary>
        /// Effects to spawn on projectile hit
        /// </summary>
        /// <value></value>
        public HitSpawn[] SpawnOnHit { get => spawnOnHit; set => spawnOnHit = value; }

        /// <summary>
        /// If enabled, when the projectile hits on object that be have a rigidbody and collider, add explosion force on object
        /// </summary>
        /// <value></value>
        public bool AddExplosionForce { get => addExplosionForce; set => addExplosionForce = value; }

        /// <summary>
        /// Layer of objects that be receive explosion force
        /// </summary>
        /// <value></value>
        public LayerMask ExplosionInteractionLayer { get => explosionInteractionLayer; set => explosionInteractionLayer = value; }

        /// <summary>
        /// Explosion force applied on objects when projectile hit
        /// </summary>
        /// <value></value>
        public float ExplosionForce { get => explosionForce; set => explosionForce = value; }

        /// <summary>
        /// Radius of explosion
        /// </summary>
        /// <value></value>
        public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }

        public MMV_Projectile()
        {
            HitLayer = -1;
            Velocity = 1300;
            AddExplosionForce = true;
            ExplosionInteractionLayer = -1;
            ExplosionRadius = 10;
            ExplosionForce = 10000;
        }

        void Start()
        {
            lastPosition = transform.position;
        }

        void Update()
        {
            transform.Translate(Vector3.forward * Time.deltaTime * Velocity);
        }

        private void FixedUpdate()
        {
            CheckProjectileHits();
        }

        private void CheckProjectileHits()
        {
            if (Physics.Linecast(lastPosition, transform.position, out var hit, HitLayer))
            {
                HitProjectile(hit);
            }

            lastPosition = transform.position;
        }

        private void HitProjectile(RaycastHit hit)
        {
            SpawnHitEffects(hit.point);
            AddExplosionForceAddPosition(hit.point);

            OnHit.Invoke(new ProjectileHitInfo()
            {
                GameObject = hit.transform.gameObject,
                Collider = hit.collider,
                HitPoint = hit.point,
                Normal = hit.normal
            });

            Destroy(gameObject);
        }

        private void SpawnHitEffects(Vector3 hitPos)
        {
            foreach (var s in SpawnOnHit)
            {
                foreach (var obj in s.Prefabs)
                {
                    if (obj)
                    {
                        var _spawn = Instantiate(obj.gameObject, hitPos, transform.rotation);
                        Destroy(_spawn, s.LifeTime);
                    }
                }
            }
        }

        private void AddExplosionForceAddPosition(Vector3 center)
        {
            if (!AddExplosionForce)
            {
                return;
            }

            var _colliders = Physics.OverlapSphere(center, ExplosionRadius, ExplosionInteractionLayer);
            var _rigidBodies = new List<Rigidbody>();

            foreach (var c in _colliders)
            {
                if (c.TryGetComponent<Rigidbody>(out var rb))
                {
                    _rigidBodies.Add(rb);
                }
            }

            foreach (var rb in _rigidBodies)
            {
                var distance = Mathf.Clamp(Vector3.Distance(rb.position, center), 0, explosionRadius);
                var force = (distance / ExplosionRadius) * ExplosionForce;
                rb.AddExplosionForce(force, center, ExplosionRadius);
            }
        }
    }
}