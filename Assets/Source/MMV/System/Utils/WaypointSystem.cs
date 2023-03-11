using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMV.Example
{
    public class WaypointSystem : MonoBehaviour
    {
        [Header("Add child objects to be the waypoints")]

        private int waypointsLength;
        private Transform[] path;
        private Transform[] childres;

        private const float GET_PATH_INTERVAL = 1f;
        private const float WAYPOINT_SPHERE_POINT_RADIUS = 1f;

        private void OnDrawGizmos()
        {
            DrawPath();
        }

        private void DrawPath()
        {
            if (path == null || path.Length == 0)
            {
                StartCoroutine(GetPathCoroutine(GET_PATH_INTERVAL));
                return;
            }

            for (int i = 0; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i].position, path[i + 1 < path.Length ? i + 1 : 0].position);
                Gizmos.DrawWireSphere(path[i].position, WAYPOINT_SPHERE_POINT_RADIUS);
            }
        }

        private IEnumerator GetPathCoroutine(float interval)
        {
            yield return new WaitForSeconds(interval);

            if (path == null || path.Length != GetComponentsInChildren<Transform>().Length - 1)
            {
                path = GetPath();
            }

            StartCoroutine(GetPathCoroutine(interval));
        }

        public Transform[] GetPath()
        {
            var _allTransforms = GetComponentsInChildren<Transform>();
            var _waypoints = new List<Transform>();

            foreach (var t in _allTransforms)
            {
                if (t != transform)
                {
                    _waypoints.Add(t);
                }
            }

            return _waypoints.ToArray();
        }
    }
}
