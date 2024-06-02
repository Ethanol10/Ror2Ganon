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

        //Sword reenable bools
        public bool primaryWasSwapped;
        public bool secondaryWasSwapped;
        public bool utilityWasSwapped;
        public bool specialWasSwapped;
        public bool specialScepterWasSwapped;


        public bool swordFullyCharged = false;
        public bool chargingSword = false;

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

            swordFullyCharged = false;

            primaryWasSwapped = false;
            secondaryWasSwapped = false;
            utilityWasSwapped = false;
            specialWasSwapped = false;
            specialScepterWasSwapped = false;
        }

        public void Start()
        {
            if (AkSoundEngine.IsInitialized()) 
            {
                AkSoundEngine.SetRTPCValue("Volume_GanonVoice", Modules.Config.voiceVolume.Value);
                AkSoundEngine.SetRTPCValue("Volume_GanonSFX", Modules.Config.sfxVolume.Value);
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
            //Disable all Sword skills if equipped.
            if (characterBody.skillLocator.primary.skillNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_PRIMARY_SWORD_NAME") 
            {
                primaryWasSwapped = true;
                characterBody.skillLocator.primary.SetSkillOverride(characterBody.skillLocator.primary, Ganondorf.punchPrimary, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (characterBody.skillLocator.secondary.skillNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME") 
            {
                secondaryWasSwapped = true;
                characterBody.skillLocator.secondary.SetSkillOverride(characterBody.skillLocator.secondary, Ganondorf.wizardsFoot, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (characterBody.skillLocator.utility.skillNameToken == GanondorfPlugin.developerPrefix + "")
            {
                utilityWasSwapped = true;
                characterBody.skillLocator.utility.SetSkillOverride(characterBody.skillLocator.utility, Ganondorf.flameChoke, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (characterBody.skillLocator.special.skillNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_OBLITERATE_SWORD_NAME")
            {
                specialWasSwapped = true;
                characterBody.skillLocator.special.SetSkillOverride(characterBody.skillLocator.special, Ganondorf.warlockPunch, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (characterBody.skillLocator.special.skillNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SCEPTER_OBLITERATE_SWORD_NAME")
            {
                specialScepterWasSwapped = true;
                characterBody.skillLocator.special.SetSkillOverride(characterBody.skillLocator.special, Ganondorf.warlockPunchScepter, GenericSkill.SkillOverridePriority.Loadout);
            }

            meshLoc.gameObject.SetActive(false);
        }

        public void ReenableSword()
        {
            anim.SetBool("SwordEquipped", false);
            isBodySwordEnabled = true;
            if (primaryWasSwapped)
            {
                primaryWasSwapped = false;
                characterBody.skillLocator.primary.UnsetSkillOverride(characterBody.skillLocator.primary, Ganondorf.punchPrimary, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (secondaryWasSwapped) 
            {
                secondaryWasSwapped = false;
                characterBody.skillLocator.secondary.UnsetSkillOverride(characterBody.skillLocator.secondary, Ganondorf.wizardsFoot, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (utilityWasSwapped)
            {
                utilityWasSwapped = false;
                characterBody.skillLocator.utility.UnsetSkillOverride(characterBody.skillLocator.utility, Ganondorf.flameChoke, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (specialWasSwapped)
            {
                specialWasSwapped = false;
                characterBody.skillLocator.special.UnsetSkillOverride(characterBody.skillLocator.special, Ganondorf.warlockPunch, GenericSkill.SkillOverridePriority.Loadout);
            }
            if (specialScepterWasSwapped)
            {
                specialScepterWasSwapped = false;
                characterBody.skillLocator.secondary.UnsetSkillOverride(characterBody.skillLocator.special, Ganondorf.warlockPunchScepter, GenericSkill.SkillOverridePriority.Loadout);
            }
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
