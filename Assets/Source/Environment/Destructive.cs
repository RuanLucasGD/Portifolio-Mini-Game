using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructive : MonoBehaviour
{
    public float maxExplosionDistance;
    public Transform center;
    public float destroyForce;
    public float destroyObjectsDelay;

    void Start()
    {
        ExplosionDestoy(-transform.forward);
    }

    public void ExplosionDestoy(Vector3 direction)
    {
        var _rbs = gameObject.GetComponentsInChildren<Rigidbody>(true);

        foreach (var rb in _rbs)
        {
            var _distance = Vector3.Distance(rb.position, center.position);
            var _relativeForce = 1 - (Mathf.Clamp(_distance, 0, maxExplosionDistance) / maxExplosionDistance);
            var _force = _relativeForce * destroyForce;
            var _direction = rb.position - center.position;

            if (_direction.magnitude > 0)
            {
                _direction /= _direction.magnitude;
            }

            rb.AddForce(_direction * _force);
            rb.AddTorque((Quaternion.LookRotation((rb.position - center.position).normalized) * Vector3.forward) * 900);
            Destroy(rb.gameObject, destroyObjectsDelay);
        }
    }
}
