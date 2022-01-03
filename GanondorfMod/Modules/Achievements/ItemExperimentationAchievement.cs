using RoR2;
using System;
using UnityEngine;

namespace GanondorfMod.Modules.Achievements
{
    internal class ItemExperiementationAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_REWARD_ID";
        public override string AchievementNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_NAME";
        public override string PrerequisiteUnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_UNLOCKABLE_REWARD_ID";
        public override string UnlockableNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_UNLOCKABLE_NAME";
        public override string AchievementDescToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_DESC";
        public override Sprite Sprite { get; } = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("achievementMultiSkinIcon");

        public override Func<string> GetHowToUnlock { get; } = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_DESC")
                            }));
        public override Func<string> GetUnlocked { get; } = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_ITEMGATHERER_ACHIEVEMENT_DESC")
                            }));

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(Modules.Survivors.Ganondorf.instance.fullBodyName);
        }

        private void CheckItem(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);

            if (self && self.teamIndex == TeamIndex.Player && self.inventory) {
                if (InventoryCheck(self.inventory)) {
                    base.Grant();
                }
            }
        }

        private bool InventoryCheck(Inventory inventory) {

            bool hasWhite = (inventory.GetItemCount(RoR2Content.Items.ScrapWhite) > 0 ? true : false);
            bool hasGreen = (inventory.GetItemCount(RoR2Content.Items.ScrapGreen) > 0 ? true : false);
            bool hasRed = (inventory.GetItemCount(RoR2Content.Items.ScrapRed) > 0 ? true : false);
            bool hasYellow = (inventory.GetItemCount(RoR2Content.Items.ScrapYellow) > 0 ? true : false);

            return hasWhite && hasGreen && hasRed && hasYellow;
        }

        public override void OnInstall()
        {
            base.OnInstall();

            On.RoR2.CharacterMaster.OnInventoryChanged += this.CheckItem;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();

            On.RoR2.CharacterMaster.OnInventoryChanged -= this.CheckItem;
        }
    }
}