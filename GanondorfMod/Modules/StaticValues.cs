using System;

namespace GanondorfMod.Modules
{
    internal static class StaticValues
    {
        //Damage coefficients govern how much the base damage should be multiplied
        internal const float punchDamageCoefficient = 2f;

        internal const float lightKickDamageCoefficient = 0.5f;

        internal const float heavyKickDamageCoefficient = 3f;

        internal const float dashDamageCoefficient = 4.0f;

        internal const float flameChokeDamageCoefficient = 5.0f;

        internal const float wizardFootDamageCoefficient = 1.8f;

        internal const float warlockPunchDamageCoefficient = 16.5f;

        internal const float warlockMemeDamage = 3.0f;

        internal const float warlockPunchDamageReducer = 10.0f;

        internal const float infernoGuillotineCoefficient = 20f;

        internal const float infernoGuillotineAirborneCoefficient = 1.0f;

        //Maxmimum stack of triforce buffs.
        //Max stack allows the player to stack to maxStack, but anything above maxPowerStack doesn't contribute to damage or armor.
        internal const int maxStack = 120;
        internal const int maxPowerStack = 100;
        internal const int stackAmountToDecay = 2;
        internal const float triforceMaxArmour = 30f;
        internal const float maxTimeToDecay = 3f;
        internal const float timeBetweenDecay = 1.5f;
        //Amounts to add to buff when killed
        internal const int bossKillStackAmount = 10;
        internal const int eliteKillStackAmount = 5;
        internal const int championKillStackAmount = 15;
        internal const int normalKillStackAmount = 1;
    }
}