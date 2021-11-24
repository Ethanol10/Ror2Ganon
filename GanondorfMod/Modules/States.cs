using GanondorfMod.SkillStates;
using GanondorfMod.SkillStates.BaseStates;
using System.Collections.Generic;
using System;

namespace GanondorfMod.Modules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();

        internal static void RegisterStates()
        {
            entityStates.Add(typeof(BaseMeleeAttack));

            entityStates.Add(typeof(FlameChoke));

            entityStates.Add(typeof(Punch));

            entityStates.Add(typeof(WizardFoot));

            entityStates.Add(typeof(WarlockPunch));
        }
    }
}