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
namespace KillDef
{

    public class EnemyUpdate : MonoBehaviour
    {
        public static Side side;
        public void Update()
        {
            if(EnemySelect.enabledHand == "Left") 
            {
                Player.local.creature.handRight.caster.AllowSpellWheel(handler: this);
                side = Side.Left;
                LeftHandMethod();
            }
            else if(EnemySelect.enabledHand == "Right") 
            {
                Player.local.creature.handLeft.caster.AllowSpellWheel(handler: this);
                side = Side.Right;
                RightHandMethod();
            }
        }
        private void LeftHandMethod() 
        {
            if (Player.local != null && EnemySelect.enabledMod && EnemySelect.enabledHand == "Left")
            {
                if (!Player.local.creature.handLeft.caster.allowSpellWheel) return;
                Player.local.creature.handLeft.caster.DisableSpellWheel(handler: this);
            }
            else
            {
                if (Player.local.creature.handLeft.caster.allowSpellWheel) return;
                Player.local.creature.handLeft.caster.AllowSpellWheel(handler: this);

            }
        }
        private void RightHandMethod()
        {
            if (Player.local != null && EnemySelect.enabledMod) // && EnemySelect.enabledHand == "Right")
            {
              Player.local.creature.handRight.caster.DisableSpellWheel(handler: this);
                if (!Player.local.creature.handRight.caster.allowSpellWheel) return;
            }
            else
            {
                Player.local.creature.handRight.caster.AllowSpellWheel(handler: this);
                if (Player.local.creature.handRight.caster.allowSpellWheel) return;
            }
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
        
        [SerializeField] public static string enabledHand;
        public static ModOptionString[] handOption = {
        new ModOptionString("Left", "Left"),
        new ModOptionString("Right", "Right"),
         };
        [ModOption("Hand side", "Which hand to use", valueSourceName: nameof(handOption), defaultValueIndex = 1)]
            [ModOptionSave]
            private static void HandOption(string value)
        {
            enabledHand = value;
        }

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            EventManager.onCreatureHit += EventManager_onCreatureHit;
            EventManager.onPossess += EventManager_onPossess;
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart) return;
            creature.gameObject.AddComponent<EnemyUpdate>();
        }

        private void EventManager_onCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (creature != null && !creature.isPlayer && enabledMod)// && enabledHand == "Right")
            {
                if (PlayerControl.GetHand(EnemyUpdate.side).alternateUsePressed && creature.currentHealth > 0)
                {
                    creature.Kill();
                }
            }
            /*else if(creature != null && !creature.isPlayer && enabledMod && enabledHand == "Left") 
            {
                if (PlayerControl.GetHand(Side.Left).alternateUsePressed && creature.currentHealth > 0)
                {
                    creature.Kill();
                }
            }
            */
            else
            {
                return;

            }

        }
        public override void ScriptDisable()
        {
            base.ScriptDisable();
            EventManager.onCreatureHit -= EventManager_onCreatureHit;
            EventManager.onPossess -= EventManager_onPossess;
        }


    }
}
