using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill.Spell;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static System.Net.Mime.MediaTypeNames;

namespace GravityHammerSpell
{
    public class Updates : ThunderBehaviour
    {
        public Imbue dummyImb;
        public GravityHammer gravHammer;
        public bool isActive = true;

        public string whooshEffectId;
        public EffectData whooshEffectData;
        public float jointPositionSpring = 1000f;
        public float jointPositionDamper = 150f;
        public float jointPositionMaxForce = 100000f;
        public float jointRotationSpring = 1000f;
        public float jointRotationDamper = 50f;
        public float jointRotationMaxForce = 10000f;

        private ConfigurableJoint joint;
        public void Initialize()
        {
            dummyImb = new GameObject("DummyImbue").AddComponent<Imbue>();
            gravHammer = dummyImb.gameObject.AddComponent<GravityHammer>();

            SkillGravityHammer skillData = Catalog.GetData<SkillGravityHammer>("Gravity Hammer");
            gravHammer.skill = skillData;
            gravHammer.imbue = dummyImb;

            UnityEngine.Debug.Log(gravHammer.skill != null ? "Skill is set" : "Skill is null");
            UnityEngine.Debug.Log(dummyImb != null ? "DummyImb is set" : "DummyImb is null");
        }

        private void EventManager_OnSpellUsed(string spellId, Creature creature, Side side)
        {
            if (spellId == "Fire")
            {
                UnityEngine.Debug.Log("fire");
                UnityEngine.Debug.Log("aaaa");
                if (isActive) 
                {
                    UnityEngine.Debug.Log("isActive GravHam");
                    FindAndLockTarget();
                    //gravHammer.Use(gravHammer.skill, dummyImb, true);
                }
                else 
                {
                    UnityEngine.Debug.Log("Deactivated gravham");
                    ReleaseTarget();
                    //gravHammer.Use(gravHammer.skill, dummyImb, false);
                }
            }
        }
        private void FindAndLockTarget()
        {
            // Use AimAssist to find a target within a certain range and angle
            gravHammer.heldEntity = ThunderEntity.AimAssist(new Ray(dummyImb.colliderGroup.imbueShoot.position, dummyImb.colliderGroup.imbueShoot.forward), 10f, 30f);

            if (gravHammer.heldEntity == null)
            {
                UnityEngine.Debug.LogError("No entity found to lock onto.");
                return;
            }

            // Setting up physics and joint properties based on the entity type
            float massModifier = gravHammer.heldEntity is Item item ? Mathf.Sqrt(item.physicBody.mass) : 1f;

            // Create a new GameObject to act as the joint attachment point
            GameObject jointPoint = new GameObject("GravityJointPoint");
            jointPoint.transform.position = gravHammer.heldEntity.transform.position;
            jointPoint.transform.rotation = gravHammer.heldEntity.transform.rotation;

            // Create and configure the joint
            joint = jointPoint.AddComponent<ConfigurableJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = gravHammer.heldEntity.RootPhysicBody;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = gravHammer.heldEntity is Item heldItem ? heldItem.GetLocalCenter() : Vector3.zero;

            // Configure joint drives for position and rotation
            JointDrive positionDrive = new JointDrive
            {
                positionSpring = jointPositionSpring * massModifier,
                positionDamper = jointPositionDamper * massModifier,
                maximumForce = jointPositionMaxForce * massModifier
            };

            JointDrive rotationDrive = new JointDrive
            {
                positionSpring = jointRotationSpring * massModifier,
                positionDamper = jointRotationDamper * massModifier,
                maximumForce = jointRotationMaxForce * massModifier
            };

            joint.xDrive = positionDrive;
            joint.yDrive = positionDrive;
            joint.zDrive = positionDrive;
            joint.slerpDrive = rotationDrive;

            // Play the "whoosh" effect if available
            if (whooshEffectData != null)
            {
                var effect = whooshEffectData.Spawn(gravHammer.heldEntity.Center, gravHammer.heldEntity.transform.rotation, gravHammer.heldEntity.RootTransform);
                effect?.Play();
            }

            UnityEngine.Debug.Log("Target locked and joint created.");
        }

        private void ReleaseTarget()
        {
            // Destroy the joint to release the entity
            if (joint != null)
            {
                Destroy(joint.gameObject);
                joint = null;
                UnityEngine.Debug.Log("Gravity Hammer joint released.");
            }
        }

        public void Update()
        {
            EventManager.OnSpellUsed += EventManager_OnSpellUsed;


        }


    }

    public class ThunderUpdates : ThunderScript
    {
        public override void ScriptEnable()
        {
            base.ScriptEnable();
            EventManager.onPossess += EventManager_onPossess;

        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if(eventTime == EventTime.OnEnd) 
            {
                creature.GetOrAddComponent<Updates>();

            }
        }

        public override void ScriptDisable()
        {
            base.ScriptDisable();
        }
    }
}
