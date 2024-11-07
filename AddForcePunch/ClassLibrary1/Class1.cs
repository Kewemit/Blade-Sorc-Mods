using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.InputSystem;
using System.Security.AccessControl;
using ThunderRoad.AI.Condition;
using UnityEngine.TextCore.Text;
using static ThunderRoad.TextData;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using JetBrains.Annotations;
namespace PunchDef
{

    public class EnemyUpdate : ThunderBehaviour
    {
        
        float forceAmount = 1000f;
        float forceDuration = 0.5f;
        [SerializeField]
        public static bool wasHit = false;
       
        public void Start() 
        {
         
        }
        public void Update()
        {



            ApplyForcePunch();
        }
        public void ApplyForcePunch()
        {
            Creature creature = GetComponent<Creature>();

            UnityEngine.Debug.Log("Working");
            if (creature != null && creature != Player.currentCreature && wasHit)
            {
                wasHit = false;
                Rigidbody rb = creature.GetComponent<Rigidbody>();
                if (rb != null) UnityEngine.Debug.Log("Rb not null");
                Vector3 vec = new Vector3(10000, 10000, 10000);
                rb.AddForce(vec);
                //StartCoroutine(ApplyForceCoroutine(rb));
            }
        }
        private IEnumerator ApplyForceCoroutine(Rigidbody rb, Creature creature)
        {
            Vector3 forceDir = creature.ragdoll.rootPart.transform.position - transform.position;
            rb.AddForce(forceDir.normalized * forceAmount, ForceMode.Impulse);

            float elapsed = 0f;
            while (elapsed < forceDuration)
            {
                rb.AddForce(forceDir.normalized * forceAmount * Time.deltaTime, ForceMode.Acceleration);
                yield return new WaitForFixedUpdate();
            }
        }
        public class EnemySelect : ThunderScript
        {

            public static ModOptionBool[] booleanOptions = {
            new ModOptionBool("Enabled", enabledMod),
            new ModOptionBool("Disabled", !enabledMod)

            };
            [ModOptionTooltip("Enables or disables the mod depending on choice")]
            [ModOption(defaultValueIndex = 0)]
            public static bool enabledMod = false;


            public override void ScriptEnable()
            {
                base.ScriptEnable();
                EventManager.onCreatureHit += EventManager_onCreatureHit;
                EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;

            }

            private void EventManager_onCreatureSpawn(Creature creature)
            {
                //creature.gameObject.AddComponent<EnemyUpdate>();

            }

            private void EventManager_onCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
            {
                if (creature != null && !creature.isPlayer)
                {
                    EnemyUpdate.wasHit = true;
                    creature.gameObject.AddComponent<EnemyUpdate>();

                }
            }


            public override void ScriptDisable()
            {
                base.ScriptDisable();
                EventManager.onCreatureHit -= EventManager_onCreatureHit;
            }
        }
    }
}