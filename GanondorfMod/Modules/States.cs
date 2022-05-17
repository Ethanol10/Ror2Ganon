using GanondorfMod.SkillStates;
using GanondorfMod.SkillStates.BaseStates;
using System.Collections.Generic;
using System;
using GanondorfMod.SkillStates.Ganondorf;

namespace GanondorfMod.Modules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();

        internal static void RegisterStates()
        {
            entityStates.Add(typeof(SpawnState));

            entityStates.Add(typeof(BaseMeleeAttack));

            entityStates.Add(typeof(FlameChoke));

            entityStates.Add(typeof(Punch));
            entityStates.Add(typeof(SwordSlashCombo));

            entityStates.Add(typeof(WizardFoot));
            entityStates.Add(typeof(RecklessCharge));
            entityStates.Add(typeof(SwordThrow));
            entityStates.Add(typeof(SwordCharge));

            entityStates.Add(typeof(DarkDive));

            entityStates.Add(typeof(WarlockPunch));

            entityStates.Add(typeof(WarlockPunchScepter));

            entityStates.Add(typeof(InfernoGuillotine));

            entityStates.Add(typeof(InfernoGuillotineScepter));

            entityStates.Add(typeof(ObliterateBeginCharging));
            entityStates.Add(typeof(ObliterateEnd));
            entityStates.Add(typeof(ScepterObliterateBeginCharging));
        }
    }
}