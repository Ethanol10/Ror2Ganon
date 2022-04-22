using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using On.RoR2;
using RoR2;

namespace GanondorfMod.Modules.Survivors
{
    internal class GanondorfDisplaySwordController : MonoBehaviour
    {
        public Transform bustLoc;
        public Transform handLoc;
        public Transform meshLoc;
        public Transform targetTrans;
        public Animator anim;

        private bool isInHand;
        private bool startTimer;
        private float stopwatch;
        private const float grabbedinterval = 0.4f;

        public void Awake()
        {
            anim = GetComponent<Animator>();
            Hooks();
            isInHand = false;
            stopwatch = 0f;
            startTimer = false;
        }

        public void Start() 
        {
            uint skillSelected = RoR2.Loadout.RequestInstance().bodyLoadoutManager.GetSkillVariant(RoR2.BodyCatalog.FindBodyIndex("GanondorfBody"), 0);
            Debug.Log(skillSelected);
            if (skillSelected == 0)
            {
                isInHand = false;
                anim.SetBool("SwordEquipped", false);
                startTimer = true;
            }
            else if(skillSelected == 1) 
            {
                isInHand = true;
                anim.SetBool("SwordEquipped", true);
                startTimer = true;
            }
        }

        public void Update()
        {
            if (startTimer)
            {
                stopwatch += Time.deltaTime;
                if (stopwatch >= grabbedinterval)
                {
                    SetTransformTarget();
                    stopwatch = 0f;
                    startTimer = false;
                }
            }

            if (meshLoc && targetTrans)
            {
                meshLoc.rotation = targetTrans.rotation;
                meshLoc.position = targetTrans.position;
            }

            if (!anim) 
            {
                anim = GetComponent<Animator>();
            }
        }

        public void SetTransformTarget()
        {
            if (isInHand)
            {
                targetTrans = handLoc;
            }
            else 
            {
                targetTrans = bustLoc;
            }
        }

        public void OnDestroy() 
        {
            On.RoR2.Loadout.BodyLoadoutManager.SetSkillVariant -= Loadout_BodyLoadoutManager_SetSkillVariant;
        }

        private void Hooks() 
        {
            On.RoR2.Loadout.BodyLoadoutManager.SetSkillVariant += Loadout_BodyLoadoutManager_SetSkillVariant;
        }

        private void Loadout_BodyLoadoutManager_SetSkillVariant(On.RoR2.Loadout.BodyLoadoutManager.orig_SetSkillVariant orig, RoR2.Loadout.BodyLoadoutManager self, RoR2.BodyIndex bodyIndex, int skillSlot, uint skillVariant) 
        {
            orig(self, bodyIndex, skillSlot, skillVariant);

            if (RoR2.BodyCatalog.FindBodyIndex("GanondorfBody") == bodyIndex) 
            {
                if (skillSlot == 0 && skillVariant == 1)
                {
                    isInHand = true;
                    anim.SetBool("SwordEquipped", true);
                    startTimer = true;
                }
                else if (skillSlot == 0 && skillVariant == 0) 
                {
                    isInHand = false;
                    anim.SetBool("SwordEquipped", false);
                    startTimer = true;
                }
            }
            
        }
    }
}
