using EntityStates;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Networking;
using GanondorfMod.Modules.Survivors;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
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
        internal TriforceBuffComponent triforceComponent;
        internal GanondorfController ganoncon;

        internal float damage;
        internal float maxDamage;
        internal float rampingDamageIncrement;

        internal float explosionNum;
        internal float maxExplosion;
        internal float rampingeExplosionNumIncrement;

        internal float distance;
        internal float maxDistance;
        internal float rampingDistanceIncrement;

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

            AkSoundEngine.PostEvent(4208541365, base.gameObject);

            damage = Modules.StaticValues.obliterateDamageCoefficient * this.damageStat;
            maxDamage = Modules.StaticValues.obliterateDamageCoefficient * this.damageStat * Modules.StaticValues.obliterateFinalDamageMultiplier ;
            rampingDamageIncrement = (maxDamage - damage) / (Modules.StaticValues.obliterateTimeToMaxCharge / base.attackSpeedStat);

            explosionNum = 1f;
            maxExplosion = Modules.StaticValues.obliterateMaxExplosionCount;
            rampingeExplosionNumIncrement = (maxExplosion - explosionNum) / (Modules.StaticValues.obliterateTimeToMaxCharge / base.attackSpeedStat);

            distance = 0f;
            maxDistance = Modules.StaticValues.obliterateMaxDistanceCovered;
            rampingDistanceIncrement = (maxDistance - distance) / (Modules.StaticValues.obliterateTimeToMaxCharge / base.attackSpeedStat);

            CreateIndicator();
            ganoncon = base.GetComponent<GanondorfController>();
            ganoncon.SwapToSword();

            triforceComponent = base.GetComponent<TriforceBuffComponent>();
            triforceComponent.pauseDecay = true;
            if (NetworkServer.active) 
            {
                characterBody.SetBuffCount(Modules.Buffs.damageAbsorberBuff.buffIndex, 1);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Destroy(this.obliteratorIndicatorInstance);
        }

        public override void Update()
        {
            base.Update();
            if (base.inputBank.skill4.down) 
            {
                UpdateIndicator();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (base.inputBank.skill4.down)
                {
                    base.characterDirection.moveVector = base.GetAimRay().direction.normalized;
                    if (damage <= maxDamage)
                    {
                        damage += rampingDamageIncrement * Time.fixedDeltaTime;
                    }
                    if (explosionNum <= maxExplosion) 
                    {
                        explosionNum += rampingeExplosionNumIncrement * Time.fixedDeltaTime;
                    }
                    if (distance <= maxDistance)
                    {
                        distance += rampingDistanceIncrement * Time.fixedDeltaTime;
                    }

                    if ((damage >= maxDamage) || (explosionNum >= maxExplosion) || (distance >= maxDistance))
                    {
                        isFullyCharged = true;
                    }                        
                    if ((damage >= maxDamage || explosionNum >= maxExplosion || distance >= maxDistance) && isFullyCharged)
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

                    triforceComponent = base.GetComponent<TriforceBuffComponent>();
                    triforceComponent.pauseDecay = false;
                    base.outer.SetNextState(new ObliterateEnd
                    {
                        damage = damage,
                        noOfExplosions = explosionNum,
                        distance = distance
                    });
                }
            }
        }

        private void CreateIndicator()
        {
            if (Modules.Assets.obliteratorIndicator)
            {
                this.obliteratorIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(Modules.Assets.obliteratorIndicator, this.downRay.origin, Quaternion.identity);
                this.obliteratorIndicatorInstance.transform.parent = base.characterDirection.targetTransform;
                this.obliteratorIndicatorInstance.transform.localScale = new Vector3(Modules.StaticValues.obliterateRadiusPerExplosion, 10f, distance);
                this.obliteratorIndicatorInstance.transform.localPosition = new Vector3(0f, 0f, distance / 2.0f);
                this.obliteratorIndicatorInstance.transform.localRotation = Quaternion.identity;
            }
        }

        private void UpdateIndicator() 
        {

            if (this.obliteratorIndicatorInstance) 
            {
                this.obliteratorIndicatorInstance.transform.localScale = new Vector3(Modules.StaticValues.obliterateRadiusPerExplosion, 10f, distance);
                this.obliteratorIndicatorInstance.transform.localPosition = new Vector3(0f, 0f, distance / 2.0f);
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
