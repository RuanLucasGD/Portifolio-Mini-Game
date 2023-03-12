using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Utils;

namespace Game.Mecanics
{
    public class GameManager : MonoBehaviour
    {
        public MMV.MMV_Vehicle playerVehicle;
        private PlayerController playerTurretController;

        private InteractivePanel selectedInteractiveObject;

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

            playerTurretController = playerVehicle.GetComponent<PlayerController>();

            if (!playerTurretController)
            {
                Debug.LogError($"Add a {typeof(PlayerController).Name} component on player vehicle");
            }

            Interactables = FindObjectsOfType<InteractivePanel>();
        }

        private void Start()
        {
            if (!playerVehicle || !playerTurretController)
            {
                return;
            }

            playerTurretController.vehicleWeapon.Weapon.OnProjectileDestroyed.AddListener(InteractWithSelectedPanel);
            playerTurretController.vehicleWeapon.Weapon.OnShot.AddListener(OnShot);
            playerTurretController.onSelectPanel.AddListener(OnVehicleSelectInteractable);

            foreach (var i in Interactables)
            {
                i.onPress.AddListener(OnPressPanel);
                i.onRecreate.AddListener(DisableShot);
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
            selectedInteractiveObject = panel;

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
            selectedInteractiveObject.onInteract();
        }

        private void OnShot()
        {
            playerTurretController.CanShot = false;
        }

        private void OnVehicleSelectInteractable(GameObject interactive)
        {
            foreach (var i in Interactables)
            {
                if (interactive)
                {
                    if (i is InteractivePanel)
                    {
                        i.IsSelected = i.gameObject == interactive.gameObject;
                    }
                }
                else    // no interactable selected
                {
                    i.IsSelected = false;
                }
            }
        }
    }
}


