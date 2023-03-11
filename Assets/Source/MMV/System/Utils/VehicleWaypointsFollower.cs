using UnityEngine;

namespace MMV.Example
{
    /// <summary>
    /// Causes a tracked vehicle to follow a path of waypoints
    /// </summary>
    public class VehicleWaypointsFollower : MMV_ControllerBase
    {
        public WaypointSystem waypointSystem;
        public float waypointSensorDistance;    // the distance near the current waypoint to switch to the next

        private Transform[] path;
        private MMV_ShooterManager[] vehicleWeapons;

        private int currentWaypointIndex;

        public VehicleWaypointsFollower()
        {
            waypointSensorDistance = 2f;
        }

        void Start()
        {
            if (!waypointSystem)
            {
                Debug.LogError(typeof(WaypointSystem).Name + " not finded on scene.");
                return;
            }

            path = waypointSystem.GetPath();

            if (Vehicle)
            {
                vehicleWeapons = Vehicle.GetComponentsInChildren<MMV_ShooterManager>();
            }
        }

        void Update()
        {
            if (path == null || path.Length == 0)
            {
                return;
            }

            WaypointsCheck();
            ControlVehicle();
        }

        private void WaypointsCheck()
        {
            if (Vector3.Distance(transform.position, path[currentWaypointIndex].position) < waypointSensorDistance)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex > path.Length - 1)
                {
                    currentWaypointIndex = 0;
                }
            }
        }

        private void ControlVehicle()
        {
            var _moveTo = waypointSystem.GetPath()[currentWaypointIndex].position;
            Vehicle.MoveTo(_moveTo, 0, false, false);

            foreach (var w in vehicleWeapons)
            {
                w.TargetPosition = _moveTo;
            }
        }
    }
}

