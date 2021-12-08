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
            desc = desc + "< ! > Build up your Triforce of Power by attacking or killing foes, in turn increasing your armor and the power of your Warlock Punch." + Environment.NewLine + Environment.NewLine;
            //desc = desc + "";

            string outro = "..and so he left, spreading malice in his wake.";
            string outroFailure = "..and so he vanished, in .";

            LanguageAPI.Add(prefix + "NAME", "Ganondorf");
            LanguageAPI.Add(prefix + "DESCRIPTION", desc);
            LanguageAPI.Add(prefix + "SUBTITLE", "The Great King of Evil");
            LanguageAPI.Add(prefix + "LORE", "??");
            LanguageAPI.Add(prefix + "OUTRO_FLAVOR", outro);
            LanguageAPI.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            LanguageAPI.Add(prefix + "DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add(prefix + "MASTERY_SKIN_NAME", "Regal");
            LanguageAPI.Add(prefix + "SKIN2_NAME", "Wise");
            #endregion

            #region Passive
            LanguageAPI.Add(prefix + "PASSIVE_NAME", "Triforce of Power");
            LanguageAPI.Add(prefix + "PASSIVE_DESCRIPTION", 
                $"Every successful hit and kill will build up stacks that increase Ganondorf's armor. Stacks decay after 2 seconds of no build up." +
                $" All stacks will be consumed to power up a successful hit of Warlock punch, increasing the damage up to" +
                $" <style=cIsDamage>{Modules.StaticValues.maxPowerStack / Modules.StaticValues.warlockPunchDamageReducer * 100f}%</style> at the maximum amount of stacks.");
            #endregion

            #region Primary
            LanguageAPI.Add(prefix + "PRIMARY_PUNCH_NAME", "Punch");
            LanguageAPI.Add(prefix + "PRIMARY_PUNCH_DESCRIPTION", $"On the ground, punch forward for <style=cIsDamage>{100f * StaticValues.punchDamageCoefficient}% damage</style>."
                + Environment.NewLine 
                + Helpers.stunningPrefix + $" When sprinting, dash forward, plowing through enemies for <style=cIsDamage>{100f * StaticValues.dashDamageCoefficient}% damage</style>"
                + Environment.NewLine
                + $"When airborne, kick twice for <style=cIsDamage>{100f * StaticValues.lightKickDamageCoefficient}% damage</style>" 
                + $" and <style=cIsDamage>{100f * StaticValues.heavyKickDamageCoefficient}% damage</style>.");
            #endregion

            #region Secondary
            LanguageAPI.Add(prefix + "SECONDARY_KICK_NAME", "Wizard's Foot");
            LanguageAPI.Add(prefix + "SECONDARY_KICK_DESCRIPTION", Helpers.heavyPrefix + $" Launch yourself forward pushing light enemies away for <style=cIsDamage>" +
                $"{100f * StaticValues.wizardFootDamageCoefficient}% damage</style>.");
            #endregion

            #region Utility
            LanguageAPI.Add(prefix + "UTILITY_GRAB_NAME", "Flame Choke");
            LanguageAPI.Add(prefix + "UTILITY_GRAB_DESCRIPTION", $"Dash forward grabbing enemies in a row and choke slamming them dealing " 
                + $"<style=cIsDamage>{100f * StaticValues.flameChokeDamageCoefficient}% damage</style>.");
            #endregion

            #region Special
            LanguageAPI.Add(prefix + "SPECIAL_PUNCH_NAME", "Warlock Punch");
            LanguageAPI.Add(prefix + "SPECIAL_PUNCH_DESCRIPTION", $"Charge up a powerful punch unleashing" 
                + $" <style=cIsDamage>{100f * StaticValues.warlockPunchDamageCoefficient}% damage</style> onto close range foes." 
                + $" 5% of the time, Damage is increased to <style=cIsDamage>{100f * StaticValues.warlockPunchDamageCoefficient * StaticValues.warlockMemeDamage}% damage</style>");
            #endregion

            #region Scepter Upgrade
            LanguageAPI.Add(prefix + "SCEPTERSPECIAL_NAME", "True Warlock Punch");
            LanguageAPI.Add(prefix + "SCEPTERSPECIAL_DESCRIPTION", $"Charge up a powerful punch unleashing" 
                + $" <style=cIsDamage>{100f * StaticValues.warlockPunchDamageCoefficient}% damage</style> onto close range foes." 
                + $" 5% of the time, Damage is increased to <style=cIsDamage>{100f * StaticValues.warlockPunchDamageCoefficient * StaticValues.warlockMemeDamage}% damage</style>."
                + $" The Power increase from the Triforce of Power buff is increased to 100% per stack of buff.");
            #endregion

            #region Achievements
            LanguageAPI.Add(prefix + "MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Ganondorf: Mastery");
            LanguageAPI.Add(prefix + "MASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Ganondorf, beat the game or obliterate on Monsoon.");
            LanguageAPI.Add(prefix + "MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Ganondorf: Mastery");

            LanguageAPI.Add(prefix + "EIGHTLUNAR_ACHIEVEMENT_NAME", "Ganondorf: Forbidden Methods");
            LanguageAPI.Add(prefix + "EIGHTLUNAR_ACHIEVEMENT_DESC", "As Ganondorf, collect 8 Lunar Items in a single run.");
            LanguageAPI.Add(prefix + "EIGHTLUNAR_UNLOCKABLE_NAME", "Ganondorf: Forbidden Methods");
            #endregion
            #endregion
        }
    }
}