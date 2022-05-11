using EntityStates;
using GanondorfMod.Modules.Networking;
using GanondorfMod.Modules.Survivors;
using R2API.Networking;
using R2API.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.SkillStates
{
    internal class ObliterateBeginCharging : BaseSkillState
    {
        //We want to freeze ganon in place
        //Allow him to look around while charging, use the same charging effect as Serrated Whirlwind
        //When it comes time, blast in a line in front of ganon
        //end state.
        internal GanondorfController ganoncon;

        internal float damage;
        internal float maxDamage;
        internal float rampingDamageIncrement;

        internal GameObject obliteratorIndicatorInstance;

        internal bool sentRequest;
        internal bool isFullyCharged;

        internal Ray downRay;

        public override void OnEnter()
        {
            base.OnEnter();

            isFullyCharged = false;
            sentRequest = false;
            new ChargingSwordNetworkRequest(characterBody.masterObjectId, true).Send(NetworkDestination.Clients);
            base.PlayCrossfade("FullBody, Override", "StartForwardSmash", 0.1f);

            damage = Modules.StaticValues.obliterateDamageCoefficient * this.damageStat;
            maxDamage = Modules.StaticValues.obliterateDamageCoefficient * this.damageStat * Modules.StaticValues.obliterateFinalDamageMultiplier ;
            rampingDamageIncrement = (maxDamage - damage) / (Modules.StaticValues.obliterateTimeToMaxCharge / base.attackSpeedStat);
            CreateIndicator();
            ganoncon = base.GetComponent<GanondorfController>();
            ganoncon.SwapToSword();
        }

        public override void OnExit()
        {
            base.OnExit();
            Destroy(this.obliteratorIndicatorInstance);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (base.inputBank.skill4.down)
                {
                    UpdateIndicator();
                    base.characterDirection.moveVector = base.GetAimRay().direction.normalized;
                    if (damage <= maxDamage)
                    {
                        damage += rampingDamageIncrement * Time.fixedDeltaTime;
                        if (damage >= maxDamage)
                        {
                            isFullyCharged = true;
                        }
                    }
                    if (damage >= maxDamage && isFullyCharged)
                    {
                        if (!sentRequest)
                        {
                            sentRequest = true;
                            AkSoundEngine.PostEvent(2184839552, base.gameObject);
                            new ChargingSwordNetworkRequest(characterBody.masterObjectId, false).Send(NetworkDestination.Clients);
                            new FullyChargedSwordNetworkRequest(characterBody.masterObjectId, true).Send(NetworkDestination.Clients);
                        }
                    }
                }

                if (!base.inputBank.skill4.down)
                {
                    new ChargingSwordNetworkRequest(characterBody.masterObjectId, false).Send(NetworkDestination.Clients);
                    new FullyChargedSwordNetworkRequest(characterBody.masterObjectId, false).Send(NetworkDestination.Clients);
                    base.outer.SetNextState(new ObliterateEnd
                    {
                        damage = damage
                    });
                }
            }
        }

        private void CreateIndicator()
        {
            if (Modules.Assets.obliteratorIndicator)
            {
                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = base.transform.position
                };

                this.obliteratorIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(Modules.Assets.obliteratorIndicator, this.downRay.origin, Quaternion.identity);
                this.obliteratorIndicatorInstance.transform.localScale = new Vector3(10f, 10f, 10f);
            }
        }

        private void UpdateIndicator() 
        {
            this.downRay = new Ray
            {
                direction = Vector3.down,
                origin = base.transform.position
            };

            if (this.obliteratorIndicatorInstance) 
            {
                this.obliteratorIndicatorInstance.transform.position = this.downRay.origin;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
        }
    }
}
