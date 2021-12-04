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
        public ParticleSystem FootLFire;
        public ParticleSystem HandLFire;
        public ParticleSystem FootRFire;
        public ParticleSystem HandRFire;
        public ParticleSystem HandRLightning;
        public ParticleSystem ShoulderRLightning;

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
            }

            Debug.Log(FootLFire);
            Debug.Log(HandLFire);
            Debug.Log(FootRFire);
            Debug.Log(HandRFire);
            Debug.Log(HandRLightning);
            Debug.Log(ShoulderRLightning);
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
