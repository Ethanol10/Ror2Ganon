using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GanondorfMod.Modules.Survivors
{
    public class GanondorfController : MonoBehaviour
    {
        public CharacterBody characterBody;
        public ChildLocator childLocator;
        public int maxGrabbedVal;
        public float maxDamage;
        public ParticleSystem BodyAura;
        public ParticleSystem FootLFire;
        public ParticleSystem HandLFire;
        public ParticleSystem FootRFire;
        public ParticleSystem HandRFire;
        public ParticleSystem HandRLightning;
        public ParticleSystem ShoulderRLightning;
        public ParticleSystem FootRInfernoFire;
        public ParticleSystem PullShockwave;
        public ParticleSystem BodyLightning;
        public ParticleSystem PunchCharge;
        public ParticleSystem HandLSpeedLines;
        public ParticleSystem HandRSpeedLines;
        public ParticleSystem KneeRSpeedLines;
        public ParticleSystem InfernoKickFalling;

        //Sword Positioning
        public Transform meshLoc;
        public Transform bustLoc;
        public Transform handLoc;
        public Transform handRight;
        public Transform targetLoc;
        public bool isInHand;
        public bool isBodySwordEnabled;
        public bool isInRightHand;
        public Animator anim;
        public float stopwatch = 0f;
        public bool startWatch;

        public void Awake() {
            characterBody = gameObject.GetComponent<CharacterBody>();
            childLocator = GetComponentInChildren<ChildLocator>();
            HurtBoxGroup hurtBoxGroup = characterBody.hurtBoxGroup;
            
            anim = hurtBoxGroup.gameObject.GetComponent<Animator>();

            //If childlocator exists
            if (childLocator) {
                //Cringe.
                FootLFire = childLocator.FindChild("FootLFlameParticle").GetComponent<ParticleSystem>();
                HandLFire = childLocator.FindChild("HandLFlameParticle").GetComponent<ParticleSystem>();
                FootRFire = childLocator.FindChild("FootRFlameParticle").GetComponent<ParticleSystem>();
                HandRFire = childLocator.FindChild("HandRFlameParticle").GetComponent<ParticleSystem>();
                BodyAura = childLocator.FindChild("BodyAuraMaxStack").GetComponent<ParticleSystem>();
                HandRLightning = childLocator.FindChild("HandRLightningParticle").GetComponent<ParticleSystem>();
                ShoulderRLightning = childLocator.FindChild("ShoulderRLightningParticle").GetComponent<ParticleSystem>();
                FootRInfernoFire = childLocator.FindChild("InfernoGuillotineFlame").GetComponent<ParticleSystem>();
                PullShockwave = childLocator.FindChild("PullShockwaveEffect").GetComponent<ParticleSystem>();
                BodyLightning = childLocator.FindChild("BodyLightning").GetComponent<ParticleSystem>();
                PunchCharge = childLocator.FindChild("PunchChargeEffect").GetComponent<ParticleSystem>();
                HandLSpeedLines = childLocator.FindChild("HandLSpeedLines").GetComponent<ParticleSystem>();
                HandRSpeedLines = childLocator.FindChild("HandRSpeedLines").GetComponent<ParticleSystem>();
                KneeRSpeedLines = childLocator.FindChild("KneeRSpeedLines").GetComponent<ParticleSystem>();
                InfernoKickFalling = childLocator.FindChild("InfernoKickFalling").GetComponent<ParticleSystem>();

                meshLoc = childLocator.FindChild("SwordMeshContainer");
                bustLoc = childLocator.FindChild("SwordBustLoc");
                handLoc = childLocator.FindChild("SwordHandLLoc");
                handRight = childLocator.FindChild("SwordHandRLoc");

                //Figuring out what skill is equipped to set a default 
                if (characterBody.skillLocator.primary.skillNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_PRIMARY_SWORD_NAME")
                {
                    targetLoc = handLoc;
                    isInHand = true;
                    anim.SetBool("SwordEquipped", true);
                }
                else
                {
                    isInHand = false;
                    targetLoc = bustLoc;
                    anim.SetBool("SwordEquipped", false);
                }

                isBodySwordEnabled = true;
                isInRightHand = false;
            }
        }

        public void Update()
        {
            if (targetLoc && meshLoc) 
            {
                meshLoc.position = targetLoc.position;
                meshLoc.rotation = targetLoc.rotation;
            }
        }

        public void SetMaxVal(int newVal) {
            if (newVal > maxGrabbedVal) {
                maxGrabbedVal = newVal;
            }
        }

        public void SetMaxDamage(float newVal) {
            if (newVal > maxDamage) {
                maxDamage = newVal;
            }
        }

        public void DisableAllParticles() {
            FootLFire.Stop();
            HandLFire.Stop();
            FootRFire.Stop();
            HandRFire.Stop();
            HandRLightning.Stop();
            ShoulderRLightning.Stop();
        }

        public void TempDisableSword() 
        {
            anim.SetBool("SwordEquipped", false);
            isBodySwordEnabled = false;
            characterBody.skillLocator.primary.SetSkillOverride(characterBody.skillLocator.primary, Ganondorf.punchPrimary, GenericSkill.SkillOverridePriority.Loadout);
            meshLoc.gameObject.SetActive(false);
        }

        public void ReenableSword() 
        {
            anim.SetBool("SwordEquipped", false);
            isBodySwordEnabled = true;
            characterBody.skillLocator.primary.UnsetSkillOverride(characterBody.skillLocator.primary, Ganondorf.punchPrimary, GenericSkill.SkillOverridePriority.Loadout);
            meshLoc.gameObject.SetActive(true);
        }

        public void SwapToRightHand() 
        {
            anim.SetBool("SwordEquipped", false);
            isInRightHand = true;
            isInHand = false;
            targetLoc = handRight;
        }

        public void SwapToSword() 
        {
            anim.SetBool("SwordEquipped", true);
            isInHand = true;
            isInRightHand = false;
            SetTransformTarget();
        }

        public void SwapToFist()
        {
            anim.SetBool("SwordEquipped", false);
            isInHand = false;
            isInRightHand = false;
            SetTransformTarget();
        }

        public void SetTransformTarget()
        {
            if (isInHand)
            {
                targetLoc = handLoc;
            }
            else
            {
                targetLoc = bustLoc;
            }
        }
    }
}
