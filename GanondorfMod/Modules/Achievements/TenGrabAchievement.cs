using GanondorfMod.Modules.Survivors;
using RoR2;
using System;
using UnityEngine;

namespace GanondorfMod.Modules.Achievements
{
    [RegisterAchievement(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT",
              GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_REWARD_ID", null, 0)]
    internal class TenGrabAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_REWARD_ID";
        public override string AchievementNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_NAME";
        public override string PrerequisiteUnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_UNLOCKABLE_REWARD_ID";
        public override string UnlockableNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_UNLOCKABLE_NAME";
        public override string AchievementDescToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_DESC";
        public override Sprite Sprite { get; } = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("darkDiveIconUtility");

        private GanondorfController ganonCon;

        public override Func<string> GetHowToUnlock { get; } = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_DESC")
                            }));
        public override Func<string> GetUnlocked { get; } = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_TENGRAB_ACHIEVEMENT_DESC")
                            }));

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(Modules.Survivors.Ganondorf.instance.fullBodyName);
        }

        public void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);
            if (self.baseNameToken == GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_NAME") {
                if (!ganonCon) {
                    ganonCon = self.gameObject.GetComponent<GanondorfController>();
                }

                if (ganonCon.maxGrabbedVal >= 15) {
                    if (base.meetsBodyRequirement)
                    {
                        base.Grant();
                    }
                }
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();                                                                                                       
            On.RoR2.CharacterBody.RecalculateStats += this.CharacterBody_RecalculateStats;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            On.RoR2.CharacterBody.RecalculateStats -= this.CharacterBody_RecalculateStats;
        }
    }
}