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
        public ParticleSystem HandMaxStackL;
        public ParticleSystem HandMaxStackR;
        public ParticleSystem FootLFire;
        public ParticleSystem HandLFire;
        public ParticleSystem FootRFire;
        public ParticleSystem HandRFire;
        public ParticleSystem HandRLightning;
        public ParticleSystem ShoulderRLightning;
        public ParticleSystem FootRInfernoFire;
        public ParticleSystem PullShockwave;
        public ParticleSystem BodyLightning;

        public void Awake() {
            characterBody = gameObject.GetComponent<CharacterBody>();
            childLocator = GetComponentInChildren<ChildLocator>();

            if (childLocator) {
                FootLFire = childLocator.FindChild("FootLFlameParticle").GetComponent<ParticleSystem>();
                HandLFire = childLocator.FindChild("HandLFlameParticle").GetComponent<ParticleSystem>();
                FootRFire = childLocator.FindChild("FootRFlameParticle").GetComponent<ParticleSystem>();
                HandRFire = childLocator.FindChild("HandRFlameParticle").GetComponent<ParticleSystem>();
                HandRLightning = childLocator.FindChild("HandRLightningParticle").GetComponent<ParticleSystem>();
                ShoulderRLightning = childLocator.FindChild("ShoulderRLightningParticle").GetComponent<ParticleSystem>();
                HandMaxStackL = childLocator.FindChild("MaxStackL").GetComponent<ParticleSystem>();
                HandMaxStackR = childLocator.FindChild("MaxStackR").GetComponent<ParticleSystem>();
                FootRInfernoFire = childLocator.FindChild("InfernoGuillotineFlame").GetComponent<ParticleSystem>();
                PullShockwave = childLocator.FindChild("PullShockwaveEffect").GetComponent<ParticleSystem>();
                BodyLightning = childLocator.FindChild("BodyLightning").GetComponent<ParticleSystem>();
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
