using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ThunderRoad;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace BarrelSpell
{
    public class barrelSpell : MonoBehaviour
    {
        float spawnCooldown = 0.21f;
        float shotSpeed = 30f;

        bool hasSpawned = false;
        bool wasPressed = false;


        //string optionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "\\BladeAndSorcery_Data\\StreamingAssets\\Mods\\ItemSpawnerSpell", "options.txt");
        [SerializeField] static string spawnItemID = "Anvil";
        public static ModOptionString[] spawnOption = {
        new ModOptionString("Barrel1", "Barrel1"),
        new ModOptionString("Anvil", "Anvil"),
        new ModOptionString("Pottery_01", "Pottery_01"),
        new ModOptionString("Pottery_02", "Pottery_02"),
        new ModOptionString("Sack01", "Sack01"),
        new ModOptionString("StoneCitadel2", "StoneCitadel2"),
        new ModOptionString("Arrow", "Arrow")

       // new ModOptionString

    };

        [ModOption("Item", "What item to spawn", valueSourceName: nameof(spawnOption), defaultValueIndex = 1)]
        [ModOptionSave]
        private static void SpawnOption(string value)
        {
            spawnItemID = value;
        }



        public void Update()
        {            
            
        }

        public void ShootBarrel(Transform firingHand)
        {
            if (hasSpawned)
            {
                Debug.Log("A barrel is already spawned!");
                return;
            }

            hasSpawned = true; // Set to true to prevent further spawns until reset
            //string itemId = "Barrel1";
            Catalog.GetData<ItemData>("anvil").SpawnAsync(item =>
            {
            if (item != null)
            {
                Rigidbody itemRB = item.GetComponent<Rigidbody>();
                if (itemRB == null)
                {
                    itemRB = item.gameObject.AddComponent<Rigidbody>();
                }

                if (firingHand != null && itemRB != null)
                {
                    Vector3 spawnPos = firingHand.position + firingHand.forward * 0.4f;
                    item.transform.position = spawnPos;

                    Quaternion rotationOffset = Quaternion.Euler(10, 0, 0);
                    item.transform.rotation = Quaternion.LookRotation(firingHand.forward) * rotationOffset;

                    Vector3 forwardDirection = firingHand.forward;
                    Debug.Log("Forward Direction: " + forwardDirection);
                    itemRB.AddForce(forwardDirection * shotSpeed, ForceMode.VelocityChange);
                    Debug.Log("Barrel spawned with force applied.");
                }
                else
                {
                    Debug.LogError("Firing hand or Rigidbody not found!");
                }

                StartCoroutine(WaitForSec());
                }
                else
                {
                    Debug.LogError($"Item with ID {spawnItemID} could not be found in the catalog!");
                }
            });
        }

        IEnumerator WaitForSec()
        {
            Debug.Log("Waiting for cooldown...");
            yield return new WaitForSeconds(spawnCooldown);
            hasSpawned = false; // Reset the spawn flag
            Debug.Log("Barrel can be spawned again.");
        }
    }

    public class barrelScript : ThunderScript
    {
        string checkSpell = "Fire";//"ItemSummonSpell";

        public static ModOptionBool[] booleanOptions =
        {
            new ModOptionBool("Enabled", enabledMod),
            new ModOptionBool("Disabled", !enabledMod)
        };

        [ModOptionTooltip("Enables or disables the mod depending on choice")]
        [ModOption(defaultValueIndex = 0)]
        public static bool enabledMod = false;

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            EventManager.onPossess += EventManager_onPossess;
            EventManager.OnSpellUsed += EventManager_OnSpellUsed;
        }

        private void EventManager_OnSpellUsed(string spellId, Creature creature, Side side)
        {
            if (spellId == checkSpell)
            {
                bool isPressed = UnityEngine.InputSystem.Keyboard.current[Key.Y].wasPressedThisFrame; //PlayerControl.GetHand(Side.Right).castPressed;
                if (isPressed) UnityEngine.Debug.Log("pressed");
                Transform firingHand = (side == Side.Right) ? Player.local.handRight.transform : Player.local.handLeft.transform;
                //Debug.Log("SpellSelected");
                var barrel = creature.gameObject.GetComponent<barrelSpell>();
                if (barrel != null && isPressed)
                {
                    barrel.ShootBarrel(firingHand);
                }
                else
                {
                    Debug.Log("barrelSpell component not found");
                }
            }
        }
        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            creature.gameObject.AddComponent<barrelSpell>();


        }   
        public override void ScriptDisable()
        {
            base.ScriptDisable();
            EventManager.onPossess -= EventManager_onPossess;
            EventManager.OnSpellUsed -= EventManager_OnSpellUsed;
        }
    }
}
