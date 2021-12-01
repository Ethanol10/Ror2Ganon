using System;

namespace GanondorfMod.Modules
{
    internal static class StaticValues
    {
        internal static string desc = "Ganondorf, the Great King of Evil has arrived! <color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
            + "< ! > Lay down a simple punch and send foes flying, or dash through enemies." + Environment.NewLine + Environment.NewLine
            + "< ! > Use kicks to swiftly deal with aerial foes." + Environment.NewLine + Environment.NewLine
            + "< ! > Use Flame Choke to grab multiple enemies in a row, disabling them in the process." + Environment.NewLine + Environment.NewLine
            + "< ! > Wizard's Foot is a great mobility tool, and a way to send enemies flying." + Environment.NewLine + Environment.NewLine
            + "< ! > Soak up damage and retaliate with Warlock Punch." + Environment.NewLine + Environment.NewLine;


        //Damage coefficients govern how much the base damage should be multiplied
        internal const float punchDamageCoefficient = 2f;

        internal const float lightKickDamageCoefficient = 0.5f;

        internal const float heavyKickDamageCoefficient = 3f;

        internal const float dashDamageCoefficient = 4.0f;

        internal const float flameChokeDamageCoefficient = 5.0f;

        internal const float wizardFootDamageCoefficient = 3.5f;

        internal const float warlockPunchDamageCoefficient = 15.0f;

        internal const float warlockMemeDamage = 3.0f;
    }
}