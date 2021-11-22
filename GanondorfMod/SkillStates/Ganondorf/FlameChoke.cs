using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using GanondorfMod.SkillStates.Ganondorf;

namespace GanondorfMod.SkillStates
{
    public class FlameChoke : BaseSkillState
    {
        private float grabDuration = 0.25f;
        private float grabStartDuration = 0.25f;
        public static float initialSpeedCoefficient = 4f;
        public static float finalSpeedCoefficient = 1.5f;
        private Vector3 aimRayDir;
        private float grabSpeed;
        private float dropForce = 100f; //???
        private Vector3 movementVector;
        private Transform modelTransform;
        private bool isFinishedGrabPhase;
        private bool windUpComplete;
        private Ray downRay;
        private List<GrabController> grabController;
        private float stopwatch;
        private Animator anim;
        private Vector3 previousPosition;

        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;
        public static float grabEndExplosionRadius = 15f;
        public static float flameChokeDamageCoefficient = Modules.StaticValues.flameChokeDamageCoefficient;
        public static float flameChokeProcCoefficient = 1f;
        public static float slamForce = 5000f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.modelTransform = base.GetModelTransform();
            this.anim = base.GetModelAnimator();
            this.aimRayDir = base.GetAimRay().direction;
            this.isFinishedGrabPhase = false;
            this.windUpComplete = false;
            stopwatch = 0.0f;
            grabController = new List<GrabController>();

            base.PlayAnimation("FullBody, Override", "GrabStart", "flameChoke.playbackRate", grabDuration);
            //Play Sound

            //base.characterMotor.Motor.ForceUnground();
            //Hopefully this makes him yeet across the map at max speed while grabbing.
            //base.characterMotor.velocity = Vector3.zero;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;
            //base.gameObject.layer = LayerIndex.fakeActor.intVal;
            //base.characterMotor.Motor.RebuildCollidableLayers();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (!windUpComplete) {
                //Change state to grabbing anim
                if (base.fixedAge >= grabStartDuration) {
                    windUpComplete = true;
                    anim.SetBool("continueGrabbing", true);
                    anim.SetBool("enemyCaught", false);
                }
            }
            if (windUpComplete && base.fixedAge < grabDuration + grabStartDuration) {
                SpeedBoostOnGrabDuration();
                AttemptGrab(10f);
            }

            if (base.fixedAge >= grabDuration + grabStartDuration) {
                anim.SetBool("continueGrabbing", false);
                this.outer.SetNextStateToMain();
            }
        }

        private void SpeedBoostOnGrabDuration() {
            this.grabSpeed = this.moveSpeedStat * LerpSpeedCoefficient();
            if (base.characterDirection)
            {
                base.characterDirection.forward = this.aimRayDir;
            }
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride
                   = Mathf.Lerp(WizardFoot.dodgeFOV, 60f, base.fixedAge / this.grabDuration);
            }

            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.grabSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, this.aimRayDir), 0f);
                vector = this.aimRayDir * d;

                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
        }

        private float LerpSpeedCoefficient()
        {
            return Mathf.Lerp(FlameChoke.initialSpeedCoefficient, FlameChoke.finalSpeedCoefficient, stopwatch-this.grabStartDuration / this.grabDuration);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.cameraTargetParams) {
                base.cameraTargetParams.fovOverride = -1f;
            }

            Chat.AddMessage(grabController.Count + "");
            if (grabController.Count > 0) {
                foreach (GrabController gCon in grabController)
                {
                    if (gCon)
                    {
                        gCon.Release();
                    }
                }
            }
            
            base.characterBody.bodyFlags &= CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        public void AttemptGrab(float grabRadius)
        {
            Ray aimRay = base.GetAimRay();

            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = false,
                searchOrigin = base.transform.position,
                searchDirection = UnityEngine.Random.onUnitSphere,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = grabRadius,
                maxAngleFilter = 360f
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            List<HurtBox> target = search.GetResults().ToList<HurtBox>();
            foreach (HurtBox singularTarget in target) {
                if (singularTarget)
                {
                    if (singularTarget.healthComponent && singularTarget.healthComponent.body)
                    {
                        if (BodyMeetsGrabConditions(singularTarget.healthComponent.body))
                        {
                            GrabController grabbedEnemy = singularTarget.healthComponent.body.gameObject.AddComponent<GrabController>();
                            grabbedEnemy.pivotTransform = this.FindModelChild("HandL");
                            grabController.Add(grabbedEnemy);
                        }
                    }
                }
            }
        }

        private bool BodyMeetsGrabConditions(CharacterBody targetBody)
        {
            bool meetsConditions = true;

            //if (targetBody.hullClassification == HullClassification.BeetleQueen) meetsConditions = false;

            return meetsConditions;
        }

        public bool ContinueGrabCheck() {
            if (grabController[0]) {
                return true;
            }
            return false;
        }

        public void StartDrop() {

        }

        public void GroundedFinisher() {

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
