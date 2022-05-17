using GanondorfMod.Modules.Survivors;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace GanondorfMod.Modules
{
    public class TriforceBuffComponent : MonoBehaviour
    {
        private int buffCountToApply;
        private static bool scepterActive;
        private bool isMaxStack;
        private GanondorfController ganondorfController;
        private float timer;
        private float lastTimeDecayed;
        private bool startDecaying;
        public bool pauseDecay;

        public void Awake() {
            startDecaying = false;
            timer = 0f;
            lastTimeDecayed = 0f;
            buffCountToApply = 0;
            scepterActive = false;
            isMaxStack = false;
            ganondorfController = GetComponent<GanondorfController>();
            pauseDecay = false;
        }

        public void FixedUpdate() {
            //Update whether if we are at max power stacks.
            CheckIfMaxPowerStacks();
            //Increment Timer and tell the amount to decay over time if necessary.
            if (!pauseDecay) 
            {
                DecayTimer();
                //Decay stacks if Decaying is required.
                if (startDecaying)
                {
                    lastTimeDecayed += Time.fixedDeltaTime;
                    if (lastTimeDecayed >= Modules.StaticValues.timeBetweenDecay)
                    {
                        lastTimeDecayed = 0f;
                        buffCountToApply -= Modules.StaticValues.stackAmountToDecay;
                        if (buffCountToApply < 0)
                        {
                            buffCountToApply = 0;
                        }
                    }
                }
            }

            //turn on flame effects if stacks are above the max power stack.
            if (isMaxStack) {
                ganondorfController.BodyAura.Play();
            }
            else {
                ganondorfController.BodyAura.Stop();
            }
        }

        public void DecayTimer() {
            timer += Time.fixedDeltaTime;
            if (timer >= Modules.StaticValues.maxTimeToDecay)
            {
                startDecaying = true;
            }
            else {
                startDecaying = false;
            }
        }

        public bool CheckIfMaxPowerStacks() {
            if (buffCountToApply >= Modules.StaticValues.maxPowerStack)
            {
                isMaxStack = true;
            }
            else {
                isMaxStack = false;
            }
            return isMaxStack;
        }

        public void IncrementBuffCount() {
            if (buffCountToApply >= Modules.StaticValues.maxStack) {
                return;
            }

            buffCountToApply++;
            if (buffCountToApply >= Modules.StaticValues.maxPowerStack) {
                isMaxStack = true;
            }
        }

        public void AddToBuffCount(int inc) {
            buffCountToApply += inc;
            timer = 0f;
            lastTimeDecayed = 0f;
            if (buffCountToApply >= Modules.StaticValues.maxStack) {
                buffCountToApply = Modules.StaticValues.maxStack;
                isMaxStack = true;
            }

            if (buffCountToApply >= Modules.StaticValues.maxPowerStack) {
                isMaxStack = true;
            }
        }

        public void RemoveAmountOfBuff(int decr) {
            buffCountToApply -= decr;
            timer = 0f;
            lastTimeDecayed = 0f;
            if (buffCountToApply <= 0) {
                buffCountToApply = 0;
            }
        }

        public bool GetMaxStackState() {
            return isMaxStack;
        }

        public void SetBuffCount(int newBuffAmnt) {
            if (newBuffAmnt >= Modules.StaticValues.maxStack) {
                buffCountToApply = Modules.StaticValues.maxStack;
            }
            buffCountToApply = newBuffAmnt;
            if (buffCountToApply == Modules.StaticValues.maxStack)
            {
                isMaxStack = true;
            }
            else if (buffCountToApply < Modules.StaticValues.maxStack) {
                isMaxStack = false;
            }
        }

        public int GetBuffCount() {
            if (buffCountToApply > Modules.StaticValues.maxPowerStack) {
                return Modules.StaticValues.maxPowerStack;
            }
            return buffCountToApply;
        }

        public int GetTrueBuffCount() {
            return buffCountToApply;
        }

        public void WipeBuffCount() {
            buffCountToApply = 0;
            isMaxStack = false;
        }

        public void SetScepterActive(bool newState) {
            scepterActive = newState;
        }

        public bool GetScepterState() {
            return scepterActive;
        }
    }    
}