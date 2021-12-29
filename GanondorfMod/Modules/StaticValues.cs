using System;
using static RoR2.CharacterBody;

namespace GanondorfMod.Modules
{
    internal static class StaticValues
    {
        //Damage coefficients govern how much the base damage should be multiplied
        internal const float punchDamageCoefficient = 1.5f;
        internal const float lightKickDamageCoefficient = 0.5f;
        internal const float heavyKickDamageCoefficient = 2f;
        internal const float dashDamageCoefficient = 2.5f;

        internal const float flameChokeDamageCoefficient = 3.0f;
        internal const float flameChokeAltDamageCoefficient = 1.8f;
        internal const float flameChokeDamageReducer = 0.2f;
        internal const int flameChokeBlastCountBase = 2;
        internal const float flameChokeBlastInterval = 0.15f;

        internal const float wizardFootDamageCoefficient = 1.5f;
        internal const float wizardFootAltDamageCoefficient = 3.5f;

        internal const float darkDiveDamageCoefficient = 3.0f;
        internal const float darkDiveAltDamageCoefficient = 1.5f;
        internal const float darkDiveDamageReducer = 0.1f;
        internal const int darkDiveBlastCountBase = 5;
        internal const float darkDiveBlastInterval = 0.1f;

        internal const float warlockPunchDamageCoefficient = 4.0f;
        internal const float warlockMemeDamage = 3.0f;
        internal const float warlockPunchDamageReducer = 8.0f;
        internal const float warlockPunchDamageReducerScepter = 5.0f;

        internal const float infernoGuillotineCoefficient = 3.5f;
        internal const float infernoGuillotineAirborneCoefficient = 1.0f;
        internal const float infernoGuillotinePullForce = 12f;

        //Maxmimum stack of triforce buffs.
        //Max stack allows the player to stack to maxStack, but anything above maxPowerStack doesn't contribute to damage or armor.
        internal const int maxStack = 120;
        internal const int maxPowerStack = 100;
        internal const int stackAmountToDecay = 2;
        internal const float triforceMaxArmour = 40f;
        internal const float triforceMaxDamage = 35f;
        internal const float maxTimeToDecay = 3f;
        internal const float timeBetweenDecay = 2f;
        //Amount to Decrement as a Utility move
        internal const int utilityStackConsumption = 15;
        internal const float utilityBoostCoefficient = 2.0f; 
        //Amounts to add to buff when killed
        internal const int bossKillStackAmount = 10;
        internal const int eliteKillStackAmount = 5;
        internal const int championKillStackAmount = 15;
        internal const int normalKillStackAmount = 1;

        internal const BodyFlags defaultFlags = BodyFlags.ImmuneToExecutes;
    }
}