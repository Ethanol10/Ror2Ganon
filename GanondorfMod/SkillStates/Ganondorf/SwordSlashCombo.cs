using GanondorfMod.Modules;
using GanondorfMod.SkillStates.BaseStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using static GanondorfMod.Modules.Projectiles;

namespace GanondorfMod.SkillStates
{
    public class SwordSlashCombo : BaseSwordMelee
    {

        TriforceBuffComponent buffComponent;
        public override void OnEnter()
        {
            this.hitboxName = "sword";

            this.damageType = DamageType.Generic;
            this.damageCoefficient = Modules.StaticValues.swordSwingDamageCoefficient;
            this.procCoefficient = 1f;
            this.pushForce = 300f;
            this.bonusForce = Vector3.zero;
            this.baseDuration = 1.1f;
            this.attackStartTime = 0.3f;
            this.attackEndTime = 0.5f;
            this.baseEarlyExitTime = 0.2f;
            this.hitStopDuration = 0.05f;
            this.attackRecoil = 0.5f;
            this.hitHopVelocity = 12f / this.attackSpeedStat;

            this.swingSoundString = "swingEffect";
            this.hitSoundString = "swordHitSound";
            this.muzzleString = swingIndex % 2 == 0 ? "SwingLeft" : "SwingRight";
            this.swingEffectPrefab = Modules.Assets.swordSwingEffect;
            this.hitEffectPrefab = Modules.Assets.swordHitImpactEffect;

            //this.impactSound = Modules.Assets.swordHitSoundEvent.index;
            buffComponent = GetComponent<TriforceBuffComponent>();

            base.OnEnter();
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayAttackAnimation();
        }

        protected override void PlaySwingEffect()
        {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            //Increment Buff count
            buffComponent.AddToBuffCount(1);
        }

        //Override the original function.
        protected override void FireAttack()
        {
            if (!hasFired && buffComponent.GetBuffCount() >= Modules.StaticValues.minimumAmountForSwordBeamTriforce) 
            {
                AkSoundEngine.PostEvent(1663150478, base.gameObject);
                Ray aimRay = GetAimRay();
                Modules.Projectiles.swordbeamProjectile.GetComponent<SwordbeamOnHit>().netID = base.characterBody.masterObjectId;
                ProjectileManager.instance.FireProjectile(Modules.Projectiles.swordbeamProjectile,
                    aimRay.origin,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    base.gameObject,
                    Modules.StaticValues.swordBeamDamageCoefficientBase * this.damageStat,
                    0f,
                    base.RollCrit(),
                    DamageColorIndex.Default,
                    null,
                    Modules.StaticValues.swordBeamForce);
            }

            //This sets the has fired back to true.
            base.FireAttack();
            
        }

        protected override void SetNextState()
        {
            int index = this.swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            this.outer.SetNextState(new SwordSlashCombo
            {
                swingIndex = index
            });
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}