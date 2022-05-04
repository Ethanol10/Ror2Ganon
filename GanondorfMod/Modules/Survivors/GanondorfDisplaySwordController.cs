using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using On.RoR2;
using RoR2;
using static RoR2.Loadout;

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

        private bool[] swordField;

        public void Awake()
        {
            anim = GetComponent<Animator>();
            Hooks();
            isInHand = false;
            stopwatch = 0f;
            startTimer = false;
            swordField = new bool[4];
        }

        public void Start() 
        {
            uint skillSelected = RoR2.Loadout.RequestInstance().bodyLoadoutManager.GetSkillVariant(RoR2.BodyCatalog.FindBodyIndex("GanondorfBody"), 0);
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
            On.RoR2.Loadout.BodyLoadoutManager.GetSkillVariant -= Loadout_BodyLoadoutManager_GetSkillVariant;
        }

        private void Hooks() 
        {
            On.RoR2.Loadout.BodyLoadoutManager.GetSkillVariant += Loadout_BodyLoadoutManager_GetSkillVariant;
        }

        private uint Loadout_BodyLoadoutManager_GetSkillVariant(On.RoR2.Loadout.BodyLoadoutManager.orig_GetSkillVariant orig, BodyLoadoutManager self, BodyIndex bodyIndex, int skillSlot)
        {
            uint skillVariant = orig(self, bodyIndex, skillSlot);

            if (RoR2.BodyCatalog.FindBodyIndex("GanondorfBody") == bodyIndex) 
            {
                if (skillSlot == 0)
                {
                    swordField[skillSlot] = skillVariant == 1 ? true : false;
                }
                if (skillSlot == 1)
                {
                    swordField[skillSlot] = skillVariant == 3 ? true : false;
                }
                if (skillSlot == 2)
                {
                    swordField[skillSlot] = skillVariant == 3 ? true : false;
                }
                if (skillSlot == 3)
                {
                    swordField[skillSlot] = skillVariant == 2 ? true : false;
                }

                for (int i = 0; i < swordField.Length; i++)
                {
                    if (swordField[i])
                    {
                        SetToHand();
                        return skillVariant;
                    }
                }

                SetToBody();
            }

            return skillVariant;
        }

        private void SetToHand() 
        {
            isInHand = true;
            anim.SetBool("SwordEquipped", true);
            startTimer = true;
        }

        private void SetToBody()
        {
            isInHand = false;
            anim.SetBool("SwordEquipped", false);
            startTimer = true;
        }
    }
}
