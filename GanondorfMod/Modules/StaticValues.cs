using System;
using static RoR2.CharacterBody;

namespace GanondorfMod.Modules
{
    internal static class StaticValues
    {
        //Damage coefficients govern how much the base damage should be multiplied
        internal const float punchDamageCoefficient = 2.0f;
        internal const float lightKickDamageCoefficient = 0.9f;
        internal const float heavyKickDamageCoefficient = 2.5f;
        internal const float dashDamageCoefficient = 4.0f;
        internal const float downAirDamageCoefficient = 2.5f;

        internal const float swordSwingDamageCoefficient = 2.2f;
        internal const float swordSwingprocCoefficient = 1.0f;

        internal const float flameChokeDamageCoefficient = 7.0f;
        internal const float flameChokeAltDamageCoefficient = 5.5f;
        internal const float flameChokeDamageReducer = 0.2f;
        internal const int flameChokeBlastCountBase = 2;
        internal const float flameChokeBlastInterval = 0.15f;

        //Reckless Charge
        internal const float recklessChargeDamageCoefficient = 1.5f;
        internal const float recklessChargeProcCoefficient = 0.5f;
        internal const float recklessChargeInitialChargeSpeed = 3.5f;
        internal const float recklessChargeFinalChargeSpeedMultiplier = 2.5f;
        internal const float recklessChargeTimeToFullSpeed = 15f;

        //Sword throw
        internal const float swordThrowMaxDistance = 50f;
        internal const float swordThrowMaxTimeOneWay = 3f;
        internal const float swordBaseDamageCoefficient = 0.5f;
        internal const float swordMaximumDamageMultiplier = 5f;
        internal const float swordThrowProcCoefficient = 0.2f;
        internal const float swordTimeToMaxCharge = 4f;
        internal const float swordThrowBaseDistance = 15f;
        internal const float swordThrowRotationSpeed = 1440f;
        internal const float swordThrowTickFrequency = 0.1f;
        internal const float swordThrowHitRadius = 5f;

        //Wizards foot
        internal const float wizardFootDamageCoefficient = 3.0f;
        internal const float wizardFootAltDamageCoefficient = 7.0f;

        internal const float darkDiveDamageCoefficient = 6.5f;
        internal const float darkDiveAltDamageCoefficient = 4.5f;
        internal const float darkDiveDamageReducer = 0.1f;
        internal const int darkDiveBlastCountBase = 5;
        internal const float darkDiveBlastInterval = 0.1f;

        internal const float warlockPunchDamageCoefficient = 10.0f;
        internal const float warlockMemeDamage = 3.0f;
        internal const float warlockPunchDamageReducer = 10.0f;
        internal const float warlockPunchDamageReducerScepter = 5.0f;

        internal const float infernoGuillotineCoefficient = 11.0f;
        internal const float infernoGuillotineAirborneCoefficient = 1.0f;
        internal const float infernoGuillotinePullForce = 12f;

        //Obliterate
        internal const float obliterateDamageCoefficient = 6.0f;
        internal const float obliterateDORIYACoefficient = 14.0f;
        internal const float obliterateFinalDamageMultiplier = 5.0f;
        internal const float obliterateTimeToMaxCharge = 5f;
        internal const float obliterateMaxDistanceCovered = 75f;
        internal const float obliterateMaxExplosionCount = 10f;
        internal const float obliterateMinDistance = 10f;
        internal const float obliterateRadiusPerExplosion = 5f;
        internal const float obliterateBuffArmourMultiplier = 4f;

        //Maxmimum stack of triforce buffs.
        //Max stack allows the player to stack to maxStack, but anything above maxPowerStack doesn't contribute to damage or armor.
        internal const int maxStack = 120;
        internal const int maxPowerStack = 100;
        internal const int stackAmountToDecay = 2;
        internal const float triforceMaxArmour = 40f;
        internal const float triforceMaxDamage = 45f;
        internal const float maxTimeToDecay = 3f;
        internal const float timeBetweenDecay = 2f;
        //Amount to Decrement as a Utility move
        internal const int utilityStackConsumption = 30;
        internal const float utilityBoostCoefficient = 3.0f; 
        //Amounts to add to buff when killed
        internal const int bossKillStackAmount = 10;
        internal const int eliteKillStackAmount = 5;
        internal const int championKillStackAmount = 15;
        internal const int normalKillStackAmount = 1;

        internal const BodyFlags defaultFlags = BodyFlags.ImmuneToExecutes | BodyFlags.IgnoreFallDamage;
    }
}