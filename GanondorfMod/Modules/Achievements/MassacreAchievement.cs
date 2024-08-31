using GanondorfMod.Modules.Survivors;
using RoR2;
using System;
using UnityEngine;

namespace GanondorfMod.Modules.Achievements
{
    [RegisterAchievement(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT",
        GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_REWARD_ID", null, 0)]
    internal class MassacreAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_REWARD_ID";
        public override string AchievementNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_NAME";
        public override string PrerequisiteUnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_UNLOCKABLE_REWARD_ID";
        public override string UnlockableNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_UNLOCKABLE_NAME";
        public override string AchievementDescToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_DESC";
        public override Sprite Sprite { get; } = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("brawlSkinIcon");

        bool lemFinished = false;
        bool beetleFinished = false;
        bool wispFinished = false;
        bool impFinished = false;
        bool jellyfishFinished = false;
        bool mushroomFinished = false;

        int lemCount = 0;
        int beetleCount = 0;
        int wispCount = 0;
        int impCount = 0;
        int jellyfishCount = 0;
        int mushroomCount = 0;
        int maxAmount = 50;

        public override Func<string> GetHowToUnlock { get; } = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_DESC")
                            }));
        public override Func<string> GetUnlocked { get; } = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASSACRE_ACHIEVEMENT_DESC")
                            }));

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(Modules.Survivors.Ganondorf.instance.fullBodyName);
        }

        public void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (damageReport.attackerBody != null && damageReport.attacker != null && damageReport != null)
            {
                if (damageReport.attackerBody.baseNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_NAME")
                {
                    //Achievement related counters
                    /*
                     Body names:
                     LEMURIAN_BODY_NAME
                     BEETLE_BODY_NAME
                     WISP_BODY_NAME
                     IMP_BODY_NAME
                     JELLYFISH_BODY_NAME
                     MINIMUSHROOM_BODY_NAME
                    */

                    switch (damageReport.victimBody.baseNameToken) {
                        case "LEMURIAN_BODY_NAME":
                            lemCount++;
                            break;
                        case "BEETLE_BODY_NAME":
                            beetleCount++;
                            break;
                        case "WISP_BODY_NAME":
                            wispCount++;
                            break;
                        case "IMP_BODY_NAME":
                            impCount++;
                            break;
                        case "JELLYFISH_BODY_NAME":
                            jellyfishCount++;
                            break;
                        case "MINIMUSHROOM_BODY_NAME":
                            mushroomCount++;
                            break;
                    }
                    checkCap();
                    int total = lemCount + beetleCount + wispCount + impCount + jellyfishCount + mushroomCount;
                    if (total == maxAmount * 5) {
                        if (base.meetsBodyRequirement)
                        {
                            base.Grant();
                        }
                    }
                }
            }
        }

        public void checkCap() {
            if (lemCount >= maxAmount) {
                lemCount = maxAmount;
                if (!lemFinished) {
                    lemFinished = true;
                    Chat.AddMessage(Helpers.DownsideDescription("Lemurians Consumed."));
                }
            }
            if (beetleCount >= maxAmount){
                beetleCount = maxAmount;
                if (!beetleFinished) {
                    beetleFinished = true;
                    Chat.AddMessage(Helpers.DownsideDescription("Beetles Consumed."));
                }
            }
            if (wispCount >= maxAmount){
                wispCount = maxAmount;
                if (!wispFinished) {
                    wispFinished = true;
                    Chat.AddMessage(Helpers.DownsideDescription("Wips Consumed."));
                }
            }
            if (impCount >= maxAmount){
                impCount = maxAmount;
                if (!impFinished) {
                    impFinished = true;
                    Chat.AddMessage(Helpers.DownsideDescription("Imps Consumed."));
                }
            }
            if (jellyfishCount >= maxAmount){
                jellyfishCount = maxAmount;
                if (!jellyfishFinished) {
                    jellyfishFinished = true;
                    Chat.AddMessage(Helpers.DownsideDescription("Jellyfish Consumed."));
                }
            }
            if (mushroomCount >= maxAmount) {
                mushroomCount = maxAmount;
                if (!mushroomFinished) {
                    mushroomFinished = true;
                    Chat.AddMessage(Helpers.DownsideDescription("Mushrums Consumed."));
                }
            }
        }

        public void ClearStatistics(Run run)
        {
            //Reset counters
            lemCount = 0;
            beetleCount = 0;
            wispCount = 0;
            impCount = 0;
            jellyfishCount = 0;
            mushroomCount = 0;

            //Reset bools
            lemFinished = false;
            beetleFinished = false;
            wispFinished = false;
            impFinished = false;
            jellyfishFinished = false;
            mushroomFinished = false;
        }

        public override void OnInstall()
        {
            base.OnInstall();
            On.RoR2.GlobalEventManager.OnCharacterDeath += this.GlobalEventManager_OnCharacterDeath;
            Run.onRunDestroyGlobal += this.ClearStatistics;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            On.RoR2.GlobalEventManager.OnCharacterDeath -= this.GlobalEventManager_OnCharacterDeath;
            Run.onRunDestroyGlobal -= this.ClearStatistics;
        }
    }
}