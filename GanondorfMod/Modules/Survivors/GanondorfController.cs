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

        public void Awake() {
            characterBody = gameObject.GetComponent<CharacterBody>();
            childLocator = GetComponentInChildren<ChildLocator>();
            maxGrabbedVal = 0;

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
            }
        }

        public void SetMaxVal(int newVal) {
            if (newVal > maxGrabbedVal) {
                maxGrabbedVal = newVal;
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
    }
}
