using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace GanondorfMod.Modules
{
    public class TriforceBuffComponent : MonoBehaviour
    {
        private static int maxStack = 100;
        private int buffCountToApply;
        private static bool scepterActive;

        public void Awake() {
            buffCountToApply = 0;
            scepterActive = false;
        }

        public void IncrementBuffCount() {
            if (buffCountToApply >= maxStack) {
                return;
            }

            buffCountToApply++;
        }

        public void SetBuffCount(int newBuffAmnt) {
            if (newBuffAmnt >= maxStack) {
                buffCountToApply = maxStack;
            }
            buffCountToApply = newBuffAmnt;
        }

        public int GetBuffCount() {
            return buffCountToApply;
        }

        public void WipeBuffCount() {
            buffCountToApply = 0;
        }

        public void SetScepterActive(bool newState) {
            scepterActive = newState;
        }

        public bool GetScepterState() {
            return scepterActive;
        }
    }    
}