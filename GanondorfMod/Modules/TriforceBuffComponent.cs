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

        public void Awake() {
            buffCountToApply = 0;
            scepterActive = false;
            isMaxStack = false;
        }

        public void IncrementBuffCount() {
            if (buffCountToApply >= Modules.StaticValues.maxStack) {
                return;
            }

            buffCountToApply++;
            if (buffCountToApply == Modules.StaticValues.maxStack) {
                isMaxStack = true;
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