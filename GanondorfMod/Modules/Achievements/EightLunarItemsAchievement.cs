﻿using RoR2;
using System;
using UnityEngine;

namespace GanondorfMod.Modules.Achievements
{
    internal class EightLunarItemsAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_REWARD_ID";
        public override string AchievementNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_NAME";
        public override string PrerequisiteUnlockableIdentifier { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_UNLOCKABLE_REWARD_ID";
        public override string UnlockableNameToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_UNLOCKABLE_NAME";
        public override string AchievementDescToken { get; } = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_DESC";
        public override Sprite Sprite { get; } = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("skin1");

        public override Func<string> GetHowToUnlock { get; } = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_DESC")
                            }));
        public override Func<string> GetUnlocked { get; } = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_NAME"),
                                Language.GetString(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_EIGHTLUNAR_ACHIEVEMENT_DESC")
                            }));

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(Modules.Survivors.Ganondorf.instance.fullBodyName);
        }

        private void CheckItem(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);

            if (self && self.teamIndex == TeamIndex.Player && self.inventory) {
                int count = InventoryCheck(self.inventory);
                if (count >= 8) {
                    base.Grant();
                }
            }
        }

        private int InventoryCheck(Inventory inventory) {
            int count = 0;

            count += inventory.GetItemCount(RoR2Content.Items.LunarBadLuck);
            count += inventory.GetItemCount(RoR2Content.Items.LunarTrinket);
            count += inventory.GetItemCount(RoR2Content.Items.GoldOnHit);
            count += inventory.GetItemCount(RoR2Content.Items.RepeatHeal);
            count += inventory.GetItemCount(RoR2Content.Items.MonstersOnShrineUse);
            count += inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement);
            count += inventory.GetItemCount(RoR2Content.Items.FocusConvergence);
            count += inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment);
            count += inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement);
            count += inventory.GetItemCount(RoR2Content.Items.RandomDamageZone);
            count += inventory.GetItemCount(RoR2Content.Items.LunarDagger);
            count += inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement);
            count += inventory.GetItemCount(RoR2Content.Items.ShieldOnly);
            count += inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement);

            if (inventory.GetEquipmentIndex() == RoR2Content.Equipment.LunarPotion.equipmentIndex
                || inventory.GetEquipmentIndex() == RoR2Content.Equipment.Meteor.equipmentIndex 
                || inventory.GetEquipmentIndex() == RoR2Content.Equipment.CrippleWard.equipmentIndex 
                || inventory.GetEquipmentIndex() == RoR2Content.Equipment.Tonic.equipmentIndex
                || inventory.GetEquipmentIndex() == RoR2Content.Equipment.BurnNearby.equipmentIndex) {
                count++;
            }

            return count;
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