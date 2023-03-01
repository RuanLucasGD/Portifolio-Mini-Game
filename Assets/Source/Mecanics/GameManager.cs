using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Utils;

namespace Game.Mecanics
{
    public class GameManager : MonoBehaviour
    {
        public MMV.MMV_Vehicle playerVehicle;
        private VehicleController playerVehicleController;
        private PlayerTurretController playerTurretController;

        private InteractivePanel selectedPanel;

        private UrlLink[] links;

        private static GameManager instance;

        public InteractivePanel[] Interactables { get; private set; }

        public static GameManager Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<GameManager>();

                    if (!instance)
                    {
                        instance = new GameObject("Game Manager").AddComponent<GameManager>();
                        instance.playerVehicle = FindObjectOfType<MMV.MMV_Vehicle>();
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (!playerVehicle)
            {
                Debug.LogError("Player vehicle not assigned on Game Manager");
                return;
            }

            playerTurretController = playerVehicle.GetComponent<PlayerTurretController>();
            playerVehicleController = playerVehicle.GetComponent<VehicleController>();

            if (!playerTurretController)
            {
                Debug.LogError($"Add a {typeof(PlayerTurretController).Name} component on player vehicle");
            }

            if (!playerVehicleController)
            {
                Debug.LogError($"Add a {typeof(VehicleController).Name} component on player vehicle");
            }

            links = FindObjectsOfType<UrlLink>();
            Interactables = FindObjectsOfType<InteractivePanel>();
        }

        private void Start()
        {
            playerTurretController.vehicleWeapon.Weapon.OnProjectileDestroyed.AddListener(InteractWithSelectedPanel);
            playerTurretController.vehicleWeapon.Weapon.OnShot.AddListener(OnShot);

            foreach (var i in Interactables)
            {
                i.onPress.AddListener(OnPressPanel);
            }

            foreach (var l in links)
            {
                l.onOpen.AddListener(DisableShot);
            }
        }

        private void Update()
        {
            if (!playerTurretController.CanVehicleMove)
            {
            }
        }

        private void DisableShot()
        {
            playerTurretController.CanShot = false;
            playerTurretController.CanVehicleMove = true;
            playerTurretController.AutoSelectPanel = true;

        }

        private void EnableShot()
        {
            playerTurretController.CanShot = true;
            playerTurretController.CanVehicleMove = false;
            playerTurretController.AutoSelectPanel = false;
        }

        private void OnPressPanel(InteractivePanel panel)
        {
            selectedPanel = panel;

            if (playerTurretController.IsOnInteractiveArea)
            {
                panel.DestroyOnInteract = true;
                playerTurretController.Target = panel.center;
                EnableShot();
            }
            else
            {
                panel.DestroyOnInteract = false;
                playerTurretController.CanVehicleMove = false;
                playerTurretController.AutoSelectPanel = false;
                panel.onInteract();
            }
        }

        private void InteractWithSelectedPanel()
        {
            selectedPanel.onInteract();
        }

        private void OnShot()
        {
            playerTurretController.CanShot = false;
        }
    }
}


