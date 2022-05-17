using R2API;
using System;

namespace GanondorfMod.Modules
{
    internal static class Tokens
    {
        internal static void AddTokens()
        {
            #region Ganondorf
            string prefix = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_";

            string desc = "Ganondorf, the Great King of Evil has arrived! <color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Lay down a simple punch and send foes flying, or dash through enemies." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Use kicks to swiftly deal with aerial foes." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Use Flame Choke to grab multiple enemies in a row, disabling them in the process." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Wizard's Foot is a great mobility tool, and a way to send enemies flying." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Warlock Punch to release a devastating explosive punch." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Build up your Triforce of Power by attacking or killing foes, in turn increasing your armor, attack and the power of your Utility and Special." + Environment.NewLine + Environment.NewLine;
            //desc = desc + "";

            string outro = "..and so he left, spreading malice in his wake.";
            string outroFailure = "..and so he vanished, sealed for eternity...";

            LanguageAPI.Add(prefix + "NAME", "Ganondorf");
            LanguageAPI.Add(prefix + "DESCRIPTION", desc);
            LanguageAPI.Add(prefix + "SUBTITLE", "The Great King of Evil");
            LanguageAPI.Add(prefix + "LORE", "??");
            LanguageAPI.Add(prefix + "OUTRO_FLAVOR", outro);
            LanguageAPI.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            LanguageAPI.Add(prefix + "DEFAULT_SKIN_NAME", "Classic");
            LanguageAPI.Add(prefix + "MASTERY_SKIN_NAME", "Regal");
            LanguageAPI.Add(prefix + "OLD_SKIN_NAME", "Wise");
            LanguageAPI.Add(prefix + "BRAWL_SKIN_NAME", "Royal");
            LanguageAPI.Add(prefix + "BLAST_SKIN_NAME", "Blast from the past");
            LanguageAPI.Add(prefix + "SKIN1_NAME", "Saturated Classic");
            LanguageAPI.Add(prefix + "SKIN2_NAME", "Purple");
            LanguageAPI.Add(prefix + "SKIN3_NAME", "Green");
            LanguageAPI.Add(prefix + "SKIN5_NAME", "Hulking Malice");
            LanguageAPI.Add(prefix + "SKIN7_NAME", "Brown");
            #endregion

            #region Passive
            LanguageAPI.Add(prefix + "PASSIVE_NAME", "Triforce of Power");
            LanguageAPI.Add(prefix + "PASSIVE_DESCRIPTION",
                $"Every successful hit and kill will build up stacks that increase Ganondorf's armor and base damage. Stacks decay after 2 seconds of no build up." +
                $" All stacks will be consumed to power up a successful hit of your Special, increasing the damage up to" +
                $" <style=cIsDamage>{Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducer * 100f}%</style> at the maximum amount of stacks.");
            #endregion

            #region Primary
            LanguageAPI.Add(prefix + "PRIMARY_PUNCH_NAME", "Melee");
            LanguageAPI.Add(prefix + "PRIMARY_PUNCH_DESCRIPTION", $"On the ground, punch forward for <style=cIsDamage>{100f * StaticValues.punchDamageCoefficient}% damage</style>."
                + Environment.NewLine
                + Helpers.stunningPrefix + $" When sprinting, dash forward, plowing through enemies for <style=cIsDamage>{100f * StaticValues.dashDamageCoefficient}% damage</style>."
                + Environment.NewLine
                + $"When airborne, kick twice for <style=cIsDamage>{100f * StaticValues.lightKickDamageCoefficient}% damage</style>"
                + $" and <style=cIsDamage>{100f * StaticValues.heavyKickDamageCoefficient}% damage</style>."
                + Environment.NewLine
                + $"When airborne and while looking downwards, thrust your feet downwards bouncing a short distance upwards for <style=cIsDamage>{100f * StaticValues.downAirDamageCoefficient}% damage</style>.");

            LanguageAPI.Add(prefix + "PRIMARY_SWORD_NAME", "Cleave");
            LanguageAPI.Add(prefix + "PRIMARY_SWORD_DESCRIPTION", $"Cleave through hordes for <style=cIsDamage>{Modules.StaticValues.swordSwingDamageCoefficient * 100f}%</style> damage.");

            #endregion

            #region Secondary
            LanguageAPI.Add(prefix + "SECONDARY_KICK_NAME", "Wizard's Foot");
            LanguageAPI.Add(prefix + "SECONDARY_KICK_DESCRIPTION", Helpers.heavyPrefix + $" Launch yourself forward pushing light enemies away for <style=cIsDamage>" +
                $"{100f * StaticValues.wizardFootDamageCoefficient}% damage</style>.");
            LanguageAPI.Add(prefix + "SECONDARY_GRAB_NAME", "Flame Choke");
            LanguageAPI.Add(prefix + "SECONDARY_GRAB_DESCRIPTION", $"Dash forward grabbing enemies in a row and choke slamming them dealing "
                + $"<style=cIsDamage>{100f * StaticValues.flameChokeAltDamageCoefficient}% damage</style>.");

            LanguageAPI.Add(prefix + "SECONDARY_SWORD_CHARGE_NAME", "Serrated Whirlwind");
            LanguageAPI.Add(prefix + "SECONDARY_SWORD_CHARGE_DESCRIPTION", $"Hold down to charge your sword for up to {Modules.StaticValues.swordTimeToMaxCharge}s, and" +
                $"release to throw your sword forwards, dealing " +
                $"<style=cIsDamage>{100f * Modules.StaticValues.swordMaximumDamageMultiplier}%</style> at no charge per tick. " +
                $"At full charge, damage is increased by <style=cIsDamage>{Modules.StaticValues.swordMaximumDamageMultiplier}x</style>. " +
                $"Ganondorf can use his melee attacks while his sword is thrown.");
            #endregion

            #region Utility
            LanguageAPI.Add(prefix + "UTILITY_GRAB_NAME", "Flame Choke");
            LanguageAPI.Add(prefix + "UTILITY_GRAB_DESCRIPTION", $"Dash forward grabbing enemies in a row and choke slamming them dealing "
                + $"<style=cIsDamage>{100f * StaticValues.flameChokeDamageCoefficient}% damage</style>. Consumes {StaticValues.utilityStackConsumption}" +
                $" stacks on a successful hit in exchange for <style=cIsDamage>{100f * StaticValues.utilityBoostCoefficient}% more damage</style>.");
            LanguageAPI.Add(prefix + "UTILITY_KICK_NAME", "Wizard's Foot");
            LanguageAPI.Add(prefix + "UTILITY_KICK_DESCRIPTION", Helpers.heavyPrefix + $" Launch yourself forward pushing light enemies away for <style=cIsDamage>" +
                $"{100f * StaticValues.wizardFootAltDamageCoefficient}% damage</style>. Consumes {StaticValues.utilityStackConsumption}" +
                $" stacks on a successful hit in exchange for <style=cIsDamage>{100f * StaticValues.utilityBoostCoefficient}% more damage</style>.");
            #endregion

            #region Utility 2: Dark Dive
            LanguageAPI.Add(prefix + "UTILITY_AERIAL_GRAB_NAME", "Dark Dive");
            LanguageAPI.Add(prefix + "UTILITY_AERIAL_GRAB_DESCRIPTION", $"Leap upwards grabbing enemies in your path, blasting them to smitherines, firstly hitting"
                + $" {StaticValues.darkDiveBlastCountBase} mini-blasts at base for <style=cIsDamage>{100f * StaticValues.darkDiveDamageCoefficient * StaticValues.darkDiveDamageReducer}% damage</style>" +
                $" and then finally unleashing a blast for <style=cIsDamage>{100f * StaticValues.darkDiveDamageCoefficient}% damage</style>." +
                $" The number of mini-blasts scales with attack speed. Holding the button down before the move ends will cause Ganondorf to fastfall." +
                $" Consumes {StaticValues.utilityStackConsumption}" +
                $" stacks on a successful hit in exchange for <style=cIsDamage>{100f * StaticValues.utilityBoostCoefficient}% more damage</style>.");
            LanguageAPI.Add(prefix + "SECONDARY_AERIAL_GRAB_NAME", "Dark Dive");
            LanguageAPI.Add(prefix + "SECONDARY_AERIAL_GRAB_DESCRIPTION", $"Leap upwards grabbing enemies in your path, blasting them to smitherines, firstly hitting"
                + $" {StaticValues.darkDiveBlastCountBase} mini-blasts at base for " +
                $"<style=cIsDamage>{100f * StaticValues.darkDiveAltDamageCoefficient * StaticValues.darkDiveDamageReducer}% damage</style>" +
                $" and then finally unleashing a blast for <style=cIsDamage>{100f * StaticValues.darkDiveAltDamageCoefficient}% damage</style>." +
                $" The number of mini-blasts scales with attack speed. Holding the button down before the move ends will cause Ganondorf to fastfall.");
            #endregion

            #region Special
            LanguageAPI.Add(prefix + "SPECIAL_PUNCH_NAME", "Warlock Punch");
            LanguageAPI.Add(prefix + "SPECIAL_PUNCH_DESCRIPTION", $"Charge up a powerful punch, while gaining a short burst of Super Armor, unleashing"
                + $" <style=cIsDamage>{100f * StaticValues.warlockPunchDamageCoefficient}% damage</style> onto close range foes." +
                $"\nConsume 50 stacks to power the move up by " +
                $"<style=cIsDamage>{(int)100f * (int)Modules.StaticValues.maxPowerStack / (int)Modules.StaticValues.warlockPunchDamageReducer / 3}%</style> damage. " +
                $"Consume 100 stacks to power the move up by " +
                $"<style=cIsDamage>{100f * Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducer}%</style> damage.");

            LanguageAPI.Add(prefix + "INFERNO_KICK_NAME", "Inferno Guillotine");
            LanguageAPI.Add(prefix + "INFERNO_KICK_DESCRIPTION", $"Pull enemies into their demise, slamming your foot down, dealing"
                + $" <style=cIsDamage>{100f * StaticValues.infernoGuillotineCoefficient}% damage</style> in a radius around you." + Helpers.DownsideDescription(" No Super Armor.") +
                $"\nConsume 50 stacks to power the move up by " +
                $"<style=cIsDamage>{(int)100f * (int)Modules.StaticValues.maxPowerStack / (int)Modules.StaticValues.warlockPunchDamageReducer / 3 }%</style> damage. " +
                $"Consume 100 stacks to power the move up by " +
                $"<style=cIsDamage>{100f * Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducer}%</style> damage.");

            LanguageAPI.Add(prefix + "OBLITERATE_SWORD_NAME", "Obliterate");
            LanguageAPI.Add(prefix + "OBLITERATE_SWORD_DESCRIPTION", $"Hold your sword high, charging for up to <style=cIsDamage>{Modules.StaticValues.obliterateTimeToMaxCharge}s</style> to " +
                $"increase damage and range. Release to throw your sword down, sending explosions down in the direction you are facing. " +
                $"At base, explosions deal <style=cIsDamage>{Modules.StaticValues.obliterateDamageCoefficient * 100f}%</style> damage, " +
                $"and can scale up to a maximum of <style=cIsDamage>{Modules.StaticValues.obliterateFinalDamageMultiplier}x</style> per explosion.");
            #endregion

            #region Scepter Upgrade
            LanguageAPI.Add(prefix + "SCEPTERSPECIAL_NAME", "True Warlock Punch");
            LanguageAPI.Add(prefix + "SCEPTERSPECIAL_DESCRIPTION", $"Charge up a powerful punch unleashing"
                + $" <style=cIsDamage>{100f * StaticValues.warlockPunchDamageCoefficient}% damage</style> onto close range foes."
                + $" The Scaling of Warlock Punch is now stronger, maxing out at " +
                $"<style=cIsDamage>{100f * (Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducerScepter)}% damage</style>.");

            LanguageAPI.Add(prefix + "SCEPTER_SPECIAL_KICK_NAME", "True Inferno Guillotine");
            LanguageAPI.Add(prefix + "SCEPTER_SPECIAL_KICK_DESCRIPTION", $"Pull enemies into their demise, slamming your foot down, dealing"
                + $" <style=cIsDamage>{100f * StaticValues.infernoGuillotineCoefficient}% damage</style> " +
                $"in a radius around you." + Helpers.DownsideDescription(" No Super Armor.") +
                $" The Scaling of Inferno Guillotine is now stronger, maxing out at " +
                $"<style=cIsDamage>{100f * (Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducerScepter)}% damage</style>.");

            LanguageAPI.Add(prefix + "SCEPTER_OBLITERATE_SWORD_NAME", "True Obliteration");
            LanguageAPI.Add(prefix + "SCEPTER_OBLITERATE_SWORD_DESCRIPTION", $"Hold your sword high, charging for up to <style=cIsDamage>{Modules.StaticValues.obliterateTimeToMaxCharge}s</style> to" +
                $"increase damage and range. Release to throw your sword down, sending explosions down in the direction you are facing. " +
                $"At base, explosions deal <style=cIsDamage>{Modules.StaticValues.obliterateDamageCoefficient * 100f}%</style> damage, " +
                $"and can scale up to a maximum of <style=cIsDamage>{Modules.StaticValues.obliterateFinalDamageMultiplier}x</style> per explosion." +
                $" The Scaling of Obliterate is now stronger, maxing out at " +
                $"<style=cIsDamage>{100f * (Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducerScepter)}% damage</style>.");
            #endregion

            #region Achievements
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Ganondorf: Mastery");
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "MASTERYUNLOCKABLE_ACHIEVEMENT_DESCRIPTION", "As Ganondorf, beat the game on Monsoon.");
            LanguageAPI.Add(prefix + "MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Ganondorf: Mastery");

            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "EIGHTLUNAR_ACHIEVEMENT_NAME", "Ganondorf: Forbidden Methods");
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "EIGHTLUNAR_ACHIEVEMENT_DESCRIPTION", "As Ganondorf, collect 8 Lunar Items in a single run.");
            LanguageAPI.Add(prefix + "EIGHTLUNAR_UNLOCKABLE_NAME", "Ganondorf: Forbidden Methods");

            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "TENGRAB_ACHIEVEMENT_NAME", "Ganondorf: Hands-on");
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "TENGRAB_ACHIEVEMENT_DESCRIPTION", "As Ganondorf, grab 15 or more enemies in a single grab.");
            LanguageAPI.Add(prefix + "TENGRAB_UNLOCKABLE_NAME", "Ganondorf: Hands-on");

            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "HEAVYDAMAGE_ACHIEVEMENT_NAME", "Ganondorf: Heavy-handed \"Punch\"-ishment");
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "HEAVYDAMAGE_ACHIEVEMENT_DESCRIPTION", "As Ganondorf, perform a move that deals over 7,500 damage in a single hit.");
            LanguageAPI.Add(prefix + "HEAVYDAMAGE_UNLOCKABLE_NAME", "Ganondorf: Heavy-handed \"Punch\"-ishment");

            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "ITEMGATHERER_ACHIEVEMENT_NAME", "Ganondorf: Open for Experimentation");
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "ITEMGATHERER_ACHIEVEMENT_DESCRIPTION", "As Ganondorf, hold onto one scrap from each item tier.");
            LanguageAPI.Add(prefix + "ITEMGATHERER_UNLOCKABLE_NAME", "Ganondorf: Open for Experimentation");

            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "MASSACRE_ACHIEVEMENT_NAME", "Ganondorf: Malice Bringer");
            LanguageAPI.Add("ACHIEVEMENT_" + prefix + "MASSACRE_ACHIEVEMENT_DESCRIPTION", "As Ganondorf: Kill 50 Lemurians, Wisps, Jellyfishes, Beetles, Mini Mushrums and Imps in a single run.");
            LanguageAPI.Add(prefix + "MASSACRE_UNLOCKABLE_NAME", "Ganondorf: Malice Bringer");
            #endregion

            #endregion
        }
    }
}