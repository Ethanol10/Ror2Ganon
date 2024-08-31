using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using EntityStates;
using AncientScepter;

namespace GanondorfMod.Modules.Survivors
{
    internal class Ganondorf : SurvivorBase
    {
        internal override string bodyName { get; set; } = "Ganondorf";

        internal override GameObject bodyPrefab { get; set; }
        internal override GameObject displayPrefab { get; set; }

        internal override float sortPosition { get; set; } = 100f;

        internal override ConfigEntry<bool> characterEnabled { get; set; }

        internal override BodyInfo bodyInfo { get; set; } = new BodyInfo
        {
            armor = 20f,
            armorGrowth = 0.1f,
            bodyName = "GanondorfBody",
            bodyNameToken = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_NAME",
            bodyColor = Color.red,
            characterPortrait = Modules.Assets.LoadCharacterIcon("Ganondorf"),
            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            damage = 17f,
            damageGrowth = 2.5f,
            healthGrowth = 60f,
            healthRegen = 1.5f,
            jumpCount = 2,
            maxHealth = 250.0f,
            subtitleNameToken = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SUBTITLE",
            jumpPower = 20.0f,
        };

        internal static Material ganondorfMat = Modules.Assets.CreateMaterial("ganontex", 0.0f, Color.white, 1.0f);
        internal static Material swordMat = Modules.Assets.CreateMaterial("SmashSwordMat", 0.0f, Color.white, 2.0f);
        internal override int mainRendererIndex { get; set; } = 1;

        internal override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = ganondorfMat
                },
                new CustomRendererInfo{ 
                    childName = "SwordMesh",
                    material = swordMat,
                    ignoreOverlays = true
                }
        };

        internal override Type characterMainState { get; set; } = typeof(EntityStates.GenericCharacterMain);

        // item display stuffs
        internal override ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal override List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        internal static SkillDef warlockPunch;
        internal static SkillDef warlockPunchScepter;
        internal static SkillDef infernoGuillotine;
        internal static SkillDef punchPrimary;
        internal static SkillDef wizardsFoot;
        internal static SkillDef flameChoke;
        internal static SkillDef swordThrow;
        internal static SkillDef obliterate;

        //Unlockables.
        internal override UnlockableDef characterUnlockableDef { get; set; }
        private static UnlockableDef masterySkinUnlockableDef;
        private static UnlockableDef secondarySkinUnlockableDef;
        private static UnlockableDef tenGrabUnlockableDef;
        private static UnlockableDef heavyDamageUnlockableDef;
        private static UnlockableDef collectScrapUnlockableDef;
        private static UnlockableDef massacreUnlockableDef;

        internal override void InitializeCharacter()
        {
            base.InitializeCharacter();

            //Attach the TriforceBuffComponent
            bodyPrefab.AddComponent<GanondorfController>();
            GanondorfPlugin.triforceBuff = bodyPrefab.AddComponent<TriforceBuffComponent>();
            EntityStateMachine ganonEntityStateMachine = bodyPrefab.GetComponent<EntityStateMachine>();
            ganonEntityStateMachine.initialStateType = new SerializableEntityStateType(typeof(SkillStates.SpawnState));

            //Initialise Scepter if available
            CreateScepterSkills();
        }

        internal override void InitializeUnlockables()
        {
            masterySkinUnlockableDef = Modules.Unlockables.AddUnlockable<Achievements.MasteryAchievement>(true);
            secondarySkinUnlockableDef = Modules.Unlockables.AddUnlockable<Achievements.EightLunarItemsAchievement>(true);
            tenGrabUnlockableDef = Modules.Unlockables.AddUnlockable<Achievements.TenGrabAchievement>(true);
            heavyDamageUnlockableDef = Modules.Unlockables.AddUnlockable<Achievements.HeavyDamageAchievement>(true);
            collectScrapUnlockableDef = Modules.Unlockables.AddUnlockable<Achievements.ItemExperiementationAchievement>(true);
            massacreUnlockableDef = Modules.Unlockables.AddUnlockable<Achievements.MassacreAchievement>(true);
        }

        internal override void InitializeDoppelganger()
        {
            base.InitializeDoppelganger();
        }

        internal override void InitializeHitboxes()
        {
            ChildLocator childLocator = bodyPrefab.GetComponentInChildren<ChildLocator>();
            GameObject model = childLocator.gameObject;

            Transform hitboxTransform = childLocator.FindChild("MeleeHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform, "melee");

            Transform hitboxTransform2 = childLocator.FindChild("KickHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform2, "kick");

            Transform hitboxTransform3 = childLocator.FindChild("WarlockPunchHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform3, "warlock");

            Transform hitboxTransform4 = childLocator.FindChild("SwordHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform4, "sword");

            Transform hitboxTransform5 = childLocator.FindChild("DashAttackHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform5, "dash");

            Transform hitboxTransform6 = childLocator.FindChild("LightKickHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform6, "lightkick");

            Transform hitboxTransform7 = childLocator.FindChild("DownAirHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform7, "downair");

            Transform hitboxTransform8 = childLocator.FindChild("InfernoKickHitbox");
            Modules.Prefabs.SetupHitbox(model, hitboxTransform8, "inferno");
        }

        internal override void InitializeSkills()
        {
            Modules.Skills.CreateSkillFamilies(bodyPrefab);

            string prefix = GanondorfPlugin.developerPrefix;

            #region Passive
            SkillLocator skillLoc = bodyPrefab.GetComponent<SkillLocator>();
            skillLoc.passiveSkill.enabled = true;
            skillLoc.passiveSkill.skillNameToken = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_PASSIVE_NAME";
            skillLoc.passiveSkill.skillDescriptionToken = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_PASSIVE_DESCRIPTION";
            skillLoc.passiveSkill.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("TriforceIcon");
            #endregion 

            #region Primary
            punchPrimary = Modules.Skills.CreateSkillDef(new SkillDefInfo {
                skillName = prefix + "_GANONDORF_BODY_PRIMARY_PUNCH_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_PRIMARY_PUNCH_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_PRIMARY_PUNCH_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("punchIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Punch)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false, // Although false, it is cancelling sprint in the OnEnter() method!
                rechargeStock = 1,
                requiredStock = 0,
                stockToConsume = 0,
                keywordTokens = new string[] { "KEYWORD_STUNNING" }
            });
            Modules.Skills.AddPrimarySkill(bodyPrefab, punchPrimary);

            SkillDef swordSwingSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_PRIMARY_SWORD_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_PRIMARY_SWORD_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_PRIMARY_SWORD_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("CleaveIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.SwordSlashCombo)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = false, // Although false, it is cancelling sprint in the OnEnter() method!
                rechargeStock = 1,
                requiredStock = 0,
                stockToConsume = 0,
                //keywordTokens = new string[] { "KEYWORD_STUNNING" }
            });
            Modules.Skills.AddPrimarySkill(bodyPrefab, swordSwingSkillDef);
            #endregion

            #region Secondary/Utility Section
            wizardsFoot = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SECONDARY_KICK_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SECONDARY_KICK_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SECONDARY_KICK_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("wizardFootIconSecondary"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.WizardFoot)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 2,
                baseRechargeInterval = Modules.Config.wizardsFootCooldown.Value,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = true,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_HEAVY" }
            });
            
            flameChoke = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_UTILITY_GRAB_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_UTILITY_GRAB_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_UTILITY_GRAB_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("flameChokeIconUtility"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.FlameChoke)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = Modules.Config.flameChokeCooldown.Value,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            SkillDef darkDive = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_UTILITY_AERIAL_GRAB_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_UTILITY_AERIAL_GRAB_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_UTILITY_AERIAL_GRAB_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("darkDiveIconUtility"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.DarkDive)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 8.0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            Modules.Skills.AddSecondarySkills(bodyPrefab, wizardsFoot);
            Modules.Skills.AddUtilitySkills(bodyPrefab, flameChoke);

            //THIS ONE IS WEAKER AND INTENDED FOR SECONDARY SLOTS.
            SkillDef flameChokeAlt = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SECONDARY_GRAB_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SECONDARY_GRAB_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SECONDARY_GRAB_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("flameChokeIconSecondary"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.FlameChoke)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 2,
                baseRechargeInterval = Modules.Config.secondaryFlameChokeCooldown.Value,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            //THIS ONE IS STRONGER AND IS INTENDED FOR UTILITY SLOTS
            SkillDef wizardFootAlt = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_UTILITY_KICK_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_UTILITY_KICK_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_UTILITY_KICK_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("wizardFootIconUtility"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.WizardFoot)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = Modules.Config.utilWizardsFootCooldown.Value,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = true,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_HEAVY" }
            });

            SkillDef darkDiveAlt = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SECONDARY_AERIAL_GRAB_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SECONDARY_AERIAL_GRAB_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SECONDARY_AERIAL_GRAB_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("darkDiveIconSecondary"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.DarkDive)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 2,
                baseRechargeInterval = 5.0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            SkillDef recklessCharge = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("darkDiveIconSecondary"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Ganondorf.RecklessCharge)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 8.0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = true,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            SkillDef swordCharge = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("serratedWhirlwindIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Ganondorf.SwordCharge)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 8.0f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = false,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            swordThrow = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SECONDARY_SWORD_CHARGE_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("darkDiveIconSecondary"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Ganondorf.SwordThrow)),
                activationStateMachineName = "Slide",
                baseMaxStock = 1,
                baseRechargeInterval = 8.0f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = false,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            Modules.Skills.AddSecondarySkill(bodyPrefab, flameChokeAlt, null);
            Modules.Skills.AddUtilitySkill(bodyPrefab, wizardFootAlt, null);
            Modules.Skills.AddUtilitySkill(bodyPrefab, darkDive, tenGrabUnlockableDef);
            Modules.Skills.AddSecondarySkill(bodyPrefab, darkDiveAlt, tenGrabUnlockableDef);
            Modules.Skills.AddSecondarySkill(bodyPrefab, swordCharge, null);
            //Modules.Skills.AddSecondarySkill(bodyPrefab, recklessCharge, null);

            #endregion

            #region Special
            warlockPunch = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_SPECIAL_PUNCH_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_SPECIAL_PUNCH_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_SPECIAL_PUNCH_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("warlockPunchIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.WarlockPunch)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Modules.Skills.AddSpecialSkill(bodyPrefab, warlockPunch, null);

            //Inferno Guillotine
            infernoGuillotine = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_INFERNO_KICK_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_INFERNO_KICK_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_INFERNO_KICK_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("InfernoGuillotineIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.InfernoGuillotine)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Modules.Skills.AddSpecialSkill(bodyPrefab, infernoGuillotine, heavyDamageUnlockableDef);

            //obliterate
            obliterate = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GANONDORF_BODY_OBLITERATE_SWORD_NAME",
                skillNameToken = prefix + "_GANONDORF_BODY_OBLITERATE_SWORD_NAME",
                skillDescriptionToken = prefix + "_GANONDORF_BODY_OBLITERATE_SWORD_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("ObliterateIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.ObliterateBeginCharging)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = false,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            Modules.Skills.AddSpecialSkill(bodyPrefab, obliterate, null);


            #endregion
        }

        #region ScepterSkills
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void CreateScepterSkills()
        {
            string prefix = GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_";
            warlockPunchScepter = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "SCEPTERSPECIAL_NAME",
                skillNameToken = prefix + "SCEPTERSPECIAL_NAME",
                skillDescriptionToken = prefix + "SCEPTERSPECIAL_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("warlockPunchBoosted"),
                activationState = new SerializableEntityStateType(typeof(SkillStates.WarlockPunchScepter)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
            });

            SkillDef InfernoGuillotineScepter = Skills.CreateSkillDef(new SkillDefInfo {
                skillName = prefix + "SCEPTER_SPECIAL_KICK_NAME",
                skillNameToken = prefix + "SCEPTER_SPECIAL_KICK_NAME",
                skillDescriptionToken = prefix + "SCEPTER_SPECIAL_KICK_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("InfernoGuillotineIconBoosted"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.InfernoGuillotineScepter)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });

            SkillDef obliterateScepter = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "SCEPTER_OBLITERATE_SWORD_NAME",
                skillNameToken = prefix + "SCEPTER_OBLITERATE_SWORD_NAME",
                skillDescriptionToken = prefix + "SCEPTER_OBLITERATE_SWORD_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("obliterateBoostedIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.ScepterObliterateBeginCharging)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = false,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });


            if (GanondorfPlugin.scepterInstalled)
            {
                RegisterAncientScepterStandalone(warlockPunchScepter, InfernoGuillotineScepter, obliterateScepter);
            }       
        }

        private static void RegisterAncientScepterStandalone(SkillDef skill1, SkillDef skill2, SkillDef skill3)
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(skill1, instance.fullBodyName, SkillSlot.Special, 0);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(skill2, instance.fullBodyName, SkillSlot.Special, 1);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(skill3, instance.fullBodyName, SkillSlot.Special, 2);
        }

        #endregion

        internal override void InitializeSkins()
        {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_DEFAULT_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("skin0"),
                defaultRenderers,
                mainRenderer,
                model);

            defaultSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = defaultRenderers[0].renderer
                }, 
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = defaultRenderers[instance.mainRendererIndex].renderer
                }
            };

            skins.Add(defaultSkin);
            #endregion

            if (Config.saturatedClassicEnabled.Value) 
            {
                #region Skin01
                Material skin01Mat = Modules.Assets.CreateMaterial("ganonTex01", 0f, Color.white, 1.0f);
                Material swordMat = Modules.Assets.CreateMaterial("SmashSwordMat", 0f, Color.white, 2.0f);
                CharacterModel.RendererInfo[] skin01RendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                skin01Mat,
                swordMat,
                });

                SkinDef skin01 = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SKIN1_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("skin1"),
                    skin01RendererInfos,
                    mainRenderer,
                    model,
                    collectScrapUnlockableDef);

                skin01.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = skin01RendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = skin01RendererInfos[instance.mainRendererIndex].renderer
                }
                };

                skins.Add(skin01);
                #endregion
            }

            if (Config.purpleEnabled.Value) 
            {
                #region Skin02
                Material skin02Mat = Modules.Assets.CreateMaterial("ganonTex02", 0f, Color.white, 1.0f);
                CharacterModel.RendererInfo[] skin02RendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                skin02Mat,
                swordMat
                });

                SkinDef skin02 = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SKIN2_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("skin2"),
                    skin02RendererInfos,
                    mainRenderer,
                    model,
                    collectScrapUnlockableDef);

                skin02.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = skin02RendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = skin02RendererInfos[instance.mainRendererIndex].renderer
                }
                };

                skins.Add(skin02);
                #endregion
            }

            if (Config.greenEnabled.Value) 
            {
                #region Skin03
                Material skin03Mat = Modules.Assets.CreateMaterial("ganonTex03", 0f, Color.white, 1.0f);
                CharacterModel.RendererInfo[] skin03RendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                skin03Mat,
                swordMat,
                });

                SkinDef skin03 = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SKIN3_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("skin3"),
                    skin03RendererInfos,
                    mainRenderer,
                    model,
                    collectScrapUnlockableDef);

                skin03.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = skin03RendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = skin03RendererInfos[instance.mainRendererIndex].renderer
                }
                };

                skins.Add(skin03);
                #endregion
            }

            if (Config.hulkingMaliceEnabled.Value) 
            {
                #region Skin05
                Material skin05Mat = Modules.Assets.CreateMaterial("ganonTex05", 0f, Color.white, 1.0f);
                CharacterModel.RendererInfo[] skin05RendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                skin05Mat,
                swordMat
                });

                SkinDef skin05 = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SKIN5_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("skin5"),
                    skin05RendererInfos,
                    mainRenderer,
                    model,
                    collectScrapUnlockableDef);

                skin05.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = skin05RendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = skin05RendererInfos[instance.mainRendererIndex].renderer
                }
                };

                skins.Add(skin05);
                #endregion
            }

            if (Config.brownEnabled.Value)
            {
                #region Skin07
                Material skin07Mat = Modules.Assets.CreateMaterial("ganonTex07", 0f, Color.white, 1.0f);
                CharacterModel.RendererInfo[] skin07RendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
                {
                skin07Mat,
                swordMat
                });

                SkinDef skin07 = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_SKIN7_NAME",
                    Assets.mainAssetBundle.LoadAsset<Sprite>("skin7"),
                    skin07RendererInfos,
                    mainRenderer,
                    model,
                    collectScrapUnlockableDef);

                skin07.meshReplacements = new SkinDef.MeshReplacement[]
                {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = skin07RendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = skin07RendererInfos[instance.mainRendererIndex].renderer
                }
                };

                skins.Add(skin07);
                #endregion
            }

            #region oldManSkin6
            Material oldManSkinMat = Modules.Assets.CreateMaterial("ganonTex06", 0f, Color.white, 1.0f);
            CharacterModel.RendererInfo[] oldManSkinRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                oldManSkinMat,
                swordMat
            });

            SkinDef oldManSkin = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_OLD_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("skin6"),
                oldManSkinRendererInfos,
                mainRenderer,
                model,
                secondarySkinUnlockableDef);

            oldManSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = oldManSkinRendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = oldManSkinRendererInfos[instance.mainRendererIndex].renderer
                }
            };

            skins.Add(oldManSkin);
            #endregion

            #region MasterySkin
            Material masteryMat = Modules.Assets.CreateMaterial("ganonTex04", 10f, Color.white, 1.0f);
            Material masterySwordMat = Modules.Assets.CreateMaterial("MasterySwordMat", 10f, Color.white, 1.0f);
            CharacterModel.RendererInfo[] masteryRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                masteryMat,
                masterySwordMat
            });

            SkinDef masterySkin = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_MASTERY_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("skinMastery"),
                masteryRendererInfos,
                mainRenderer,
                model,
                masterySkinUnlockableDef);

            masterySkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonMesh"),
                    renderer = masteryRendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = masteryRendererInfos[instance.mainRendererIndex].renderer
                }
            };

            skins.Add(masterySkin);
            #endregion

            #region BrawlGanon
            Material brawlSkinMat = Modules.Assets.CreateMaterial("ganonTexBrawl", 0f, Color.white, 1.0f);
            Material brawlSwordMat = Modules.Assets.CreateMaterial("BrawlSwordMat", 2f, Color.white, 1.0f);
            CharacterModel.RendererInfo[] brawlSkinRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                brawlSkinMat,
                brawlSwordMat
            });

            SkinDef brawlSkin = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_BRAWL_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("brawlSkinIcon"),
                brawlSkinRendererInfos,
                mainRenderer,
                model,
                massacreUnlockableDef);

            brawlSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("ganonBrawlMesh"),
                    renderer = brawlSkinRendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("BrawlSword"),
                    renderer = brawlSkinRendererInfos[instance.mainRendererIndex].renderer
                }
            };

            skins.Add(brawlSkin);
            #endregion

            #region OOTGanon
            Material ootSkinMat = Modules.Assets.CreateMaterial("ootGanonTex", 0f, Color.white, 1.0f);
            CharacterModel.RendererInfo[] ootSkinRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                ootSkinMat,
                swordMat
            });

            SkinDef ootSkin = Modules.Skins.CreateSkinDef(GanondorfPlugin.developerPrefix + "_GANONDORF_BODY_BLAST_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("brawlSkinIcon"),
                ootSkinRendererInfos,
                mainRenderer,
                model);

            ootSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("OOTGanonMesh"),
                    renderer = ootSkinRendererInfos[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("SmashSword"),
                    renderer = ootSkinRendererInfos[instance.mainRendererIndex].renderer
                }
            };

            skins.Add(ootSkin);
            #endregion

            skinController.skins = skins.ToArray();
        }

        internal override void SetItemDisplays()
        {
            itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            // add item displays here
            //  HIGHLY recommend using KingEnderBrine's ItemDisplayPlacementHelper mod for this
            #region Item Displays
            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Jetpack"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
ruleType = ItemDisplayRuleType.ParentedPrefab,
followerPrefab = ItemDisplays.LoadDisplay("DisplayBugWings"),
childName = "dash",
localPos = new Vector3(0.0009F, 0.2767F, -0.1F),
localAngles = new Vector3(21.4993F, 358.6616F, 358.3334F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/GoldGat"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGoldGat"),
                            childName = "Bust",
                            localPos = new Vector3(-0.04873F, -0.02999F, -0.05053F),
                            localAngles = new Vector3(349.7842F, 169.3696F, 226.1825F),
                            localScale = new Vector3(0.02F, 0.02F, 0.02F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/BFG"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBFG"),
                            childName = "Bust",
                            localPos = new Vector3(-0.02835F, 0.00009F, -0.04215F),
                            localAngles = new Vector3(291.7483F, 293.0923F, 148.9189F),
                            localScale = new Vector3(0.02F, 0.02F, 0.02F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/CritGlasses"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGlasses"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.00289F, 0.02069F, 0.00026F),
                            localAngles = new Vector3(293.6736F, 80.62831F, 6.56808F),
                            localScale = new Vector3(0.025F, 0.025F, 0.025F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Syringe"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySyringeCluster"),
childName = "dash",
localPos = new Vector3(-0.0534F, 0.0352F, 0F),
localAngles = new Vector3(0F, 0F, 83.2547F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Behemoth"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBehemoth"),
                            childName = "Bust",
                            localPos = new Vector3(-0.04622F, 0.00816F, 0.03489F),
                            localAngles = new Vector3(359.8524F, 268.4894F, 344.9485F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Missile"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMissileLauncher"),
childName = "dash",
localPos = new Vector3(-0.3075F, 0.5204F, -0.049F),
localAngles = new Vector3(0F, 0F, 51.9225F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Dagger"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDagger"),
                            childName = "ShoulderR",
                            localPos = new Vector3(0.02234F, 0.00561F, -0.03701F),
                            localAngles = new Vector3(71.15778F, 65.10897F, 219.2698F),
                            localScale = new Vector3(0.15F, 0.15F, 0.15F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Hoof"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHoof"),
childName = "dash",
localPos = new Vector3(-0.0071F, 0.3039F, -0.051F),
localAngles = new Vector3(76.5928F, 0F, 0F),
localScale = new Vector3(0.0846F, 0.0846F, 0.0758F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ChainLightning"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayUkulele"),
                            childName = "Bust",
                            localPos = new Vector3(0.01467F, -0.04839F, -0.03012F),
                            localAngles = new Vector3(81.72643F, 264.4486F, 309.3553F),
                            localScale = new Vector3(0.15F, 0.15F, 0.15F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/GhostOnKill"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMask"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.00601F, 0.01382F, 0.00031F),
                            localAngles = new Vector3(289.1977F, 84.07439F, 11.49006F),
                            localScale = new Vector3(0.06F, 0.06F, 0.06F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Mushroom"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMushroom"),
childName = "dash",
localPos = new Vector3(-0.0139F, 0.1068F, 0F),
localAngles = new Vector3(0F, 0F, 89.4525F),
localScale = new Vector3(0.0501F, 0.0501F, 0.0501F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/AttackSpeedOnCrit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWolfPelt"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.01F, 0F, 0F),
                            localAngles = new Vector3(270F, 90F, 0F),
                            localScale = new Vector3(0.08F, 0.08F, 0.08F),
                            limbMask = LimbFlags.Head
                        }

                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BleedOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTriTip"),
                            childName = "Waist",
                            localPos = new Vector3(0.00105F, 0.00679F, -0.03055F),
                            localAngles = new Vector3(31.38007F, 107.3081F, 219.8861F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/WardOnLevel"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarbanner"),
childName = "dash",
localPos = new Vector3(0.0168F, 0.0817F, -0.0955F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(0.3162F, 0.3162F, 0.3162F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/HealOnCrit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayScythe"),
childName = "dash",
localPos = new Vector3(0.0278F, 0.2322F, -0.0877F),
localAngles = new Vector3(323.9545F, 90F, 270F),
localScale = new Vector3(0.1884F, 0.1884F, 0.1884F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/HealWhileSafe"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySnail"),
childName = "dash",
localPos = new Vector3(0F, 0.3267F, 0.046F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(0.0289F, 0.0289F, 0.0289F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Clover"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayClover"),
                            childName = "Bust",
                            localPos = new Vector3(0.02576F, 0.02835F, 0.00453F),
                            localAngles = new Vector3(351.5688F, 176.6488F, 335.4691F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BarrierOnOverHeal"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAegis"),
                            childName = "ArmR",
                            localPos = new Vector3(-0.02336F, 0.00885F, 0.00531F),
                            localAngles = new Vector3(10.61494F, 268.512F, 118.7871F),
                            localScale = new Vector3(0.025F, 0.025F, 0.025F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/GoldOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBoneCrown"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.01446F, 0.00488F, -0.00033F),
                            localAngles = new Vector3(5.02862F, 1.15325F, 73.43647F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/WarCryOnMultiKill"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPauldron"),
childName = "dash",
localPos = new Vector3(0.0435F, 0.0905F, 0.0118F),
localAngles = new Vector3(82.0839F, 160.9228F, 100.7722F),
localScale = new Vector3(0.7094F, 0.7094F, 0.7094F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SprintArmor"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBuckler"),
childName = "dash",
localPos = new Vector3(-0.005F, 0.285F, 0.0074F),
localAngles = new Vector3(358.4802F, 192.347F, 88.4811F),
localScale = new Vector3(0.3351F, 0.3351F, 0.3351F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/IceRing"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayIceRing"),
                            childName = "HandL",
                            localPos = new Vector3(0.01209F, 0.00007F, -0.00244F),
                            localAngles = new Vector3(0.05293F, 280.9355F, 79.01855F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayIceRing"),
                            childName = "HandR",
                            localPos = new Vector3(0.0128F, -0.00035F, -0.00212F),
                            localAngles = new Vector3(0.05293F, 280.9355F, 79.01855F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        },
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FireRing"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireRing"),
                            childName = "HandL",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(0.05293F, 280.9355F, 79.01855F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireRing"),
                            childName = "HandR",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(0.05293F, 280.9355F, 79.01855F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/UtilitySkillMagazine"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "UpperArmL",
                            localPos = new Vector3(0, 0, -0.002f),
                            localAngles = new Vector3(-90, 0, 0),
                            localScale = new Vector3(0.01f, 0.01f, 0.01f),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "UpperArmR",
                            localPos = new Vector3(0, 0, -0.002f),
                            localAngles = new Vector3(-90, 0, 0),
                            localScale = new Vector3(0.01f, 0.01f, 0.01f),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/JumpBoost"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWaxBird"),
                            childName = "GanonHead",
                            localPos = new Vector3(0.03941F, -0.00122F, 0.00208F),
                            localAngles = new Vector3(279.6632F, 328.884F, 115.9476F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ArmorReductionOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarhammer"),
                            childName = "Bust",
                            localPos = new Vector3(0.0521F, -0.05021F, 0.01613F),
                            localAngles = new Vector3(17.44783F, 67.45691F, 182.4427F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/NearbyDamageBonus"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDiamond"),
childName = "dash",
localPos = new Vector3(-0.002F, 0.1828F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1236F, 0.1236F, 0.1236F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ArmorPlate"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRepulsionArmorPlate"),
                            childName = "ArmL",
                            localPos = new Vector3(-0.01479F, 0.00016F, -0.00716F),
                            localAngles = new Vector3(340.9198F, 93.25529F, 100.7159F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/CommandMissile"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMissileRack"),
childName = "dash",
localPos = new Vector3(0F, 0.2985F, -0.0663F),
localAngles = new Vector3(90F, 180F, 0F),
localScale = new Vector3(0.3362F, 0.3362F, 0.3362F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Feather"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFeather"),
                            childName = "GanonHead",
                            localPos = new Vector3(0.02226F, -0.03436F, -0.00665F),
                            localAngles = new Vector3(324.1017F, 260.9333F, 188.7581F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Crowbar"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayCrowbar"),
                            childName = "Bust",
                            localPos = new Vector3(-0.026F, -0.03911F, -0.01334F),
                            localAngles = new Vector3(17.57104F, 38.50034F, 101.3128F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FallBoots"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
                            childName = "KneeL",
                            localPos = new Vector3(0.00476F, 0.00222F, -0.0064F),
                            localAngles = new Vector3(299.7572F, 344.0165F, 129.8692F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
                            childName = "KneeR",
                            localPos = new Vector3(0.00507F, 0.01096F, -0.00294F),
                            localAngles = new Vector3(293.5385F, 214.9568F, 238.9444F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExecuteLowHealthElite"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGuillotine"),
                            childName = "Bust",
                            localPos = new Vector3(-0.00264F, 0.02091F, 0.03199F),
                            localAngles = new Vector3(6.47929F, 256.2647F, 0.89259F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/EquipmentMagazine"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBattery"),
                            childName = "Bust",
                            localPos = new Vector3(0.02456F, -0.01846F, 0.05254F),
                            localAngles = new Vector3(352.2217F, 265.369F, 69.80441F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/NovaOnHeal"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.0112F, 0.00454F, 0.01047F),
                            localAngles = new Vector3(333.6707F, 268.5498F, 54.30248F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.01301F, 0.00564F, -0.00893F),
                            localAngles = new Vector3(341.8323F, 264.8607F, 314.3775F),
                            localScale = new Vector3(-0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Infusion"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayInfusion"),
childName = "dash",
localPos = new Vector3(-0.0703F, 0.0238F, -0.0366F),
localAngles = new Vector3(0F, 45F, 0F),
localScale = new Vector3(0.5253F, 0.5253F, 0.5253F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Medkit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMedkit"),
childName = "dash",
localPos = new Vector3(0.0039F, -0.0125F, -0.0546F),
localAngles = new Vector3(290F, 180F, 0F),
localScale = new Vector3(0.4907F, 0.4907F, 0.4907F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Bandolier"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBandolier"),
                            childName = "KneeL",
                            localPos = new Vector3(0.00956F, 0.00935F, -0.00253F),
                            localAngles = new Vector3(340.8699F, 136.966F, 246.8185F),
                            localScale = new Vector3(0.04F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BounceNearby"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHook"),
                            childName = "ShoulderR",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(316.4043F, 155.1508F, 346.6775F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/IgniteOnKill"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGasoline"),
childName = "dash",
localPos = new Vector3(0.0494F, 0.0954F, 0.0015F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(0.3165F, 0.3165F, 0.3165F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/StunChanceOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStunGrenade"),
childName = "dash",
localPos = new Vector3(0.001F, 0.3609F, 0.0523F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(0.5672F, 0.5672F, 0.5672F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Firework"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFirework"),
childName = "dash",
localPos = new Vector3(0.0086F, 0.0069F, 0.0565F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1194F, 0.1194F, 0.1194F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/LunarDagger"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLunarDagger"),
childName = "dash",
localPos = new Vector3(-0.0015F, 0.2234F, -0.0655F),
localAngles = new Vector3(277.637F, 358.2474F, 1.4903F),
localScale = new Vector3(0.3385F, 0.3385F, 0.3385F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Knurl"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayKnurl"),
childName = "dash",
localPos = new Vector3(-0.0186F, 0.0405F, -0.0049F),
localAngles = new Vector3(78.8707F, 36.6722F, 105.8275F),
localScale = new Vector3(0.0848F, 0.0848F, 0.0848F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BeetleGland"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBeetleGland"),
                            childName = "Bust",
                            localPos = new Vector3(0.00011F, 0.01936F, -0.02337F),
                            localAngles = new Vector3(359.9584F, 0.1329F, 39.8304F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SprintBonus"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySoda"),
childName = "dash",
localPos = new Vector3(-0.075F, 0.095F, 0F),
localAngles = new Vector3(270F, 251.0168F, 0F),
localScale = new Vector3(0.1655F, 0.1655F, 0.1655F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SecondarySkillMagazine"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDoubleMag"),
childName = "dash",
localPos = new Vector3(-0.0018F, 0.0002F, 0.097F),
localAngles = new Vector3(84.2709F, 200.5981F, 25.0139F),
localScale = new Vector3(0.0441F, 0.0441F, 0.0441F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/StickyBomb"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStickyBomb"),
childName = "dash",
localPos = new Vector3(0.0594F, 0.0811F, 0.0487F),
localAngles = new Vector3(8.4958F, 176.5473F, 162.7601F),
localScale = new Vector3(0.0736F, 0.0736F, 0.0736F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/TreasureCache"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayKey"),
childName = "dash",
localPos = new Vector3(0.0589F, 0.1056F, -0.0174F),
localAngles = new Vector3(0.2454F, 195.0205F, 89.0854F),
localScale = new Vector3(0.4092F, 0.4092F, 0.4092F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BossDamageBonus"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAPRound"),
                            childName = "ArmL",
                            localPos = new Vector3(-0.02132F, -0.00258F, 0.01275F),
                            localAngles = new Vector3(357.2343F, 57.3191F, 114.5629F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SlowOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBauble"),
childName = "dash",
localPos = new Vector3(-0.0074F, 0.076F, -0.0864F),
localAngles = new Vector3(0F, 23.7651F, 0F),
localScale = new Vector3(0.0687F, 0.0687F, 0.0687F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExtraLife"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHippo"),
                            childName = "Bust",
                            localPos = new Vector3(-0.05378F, -0.00438F, -0.04211F),
                            localAngles = new Vector3(313.4407F, 311.4665F, 129.5845F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/KillEliteFrenzy"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrainstalk"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.01777F, -0.00055F, 0.00121F),
                            localAngles = new Vector3(23.79291F, 337.9178F, 73.58833F),
                            localScale = new Vector3(0.025F, 0.025F, 0.025F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/RepeatHeal"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayCorpseFlower"),
childName = "dash",
localPos = new Vector3(-0.0393F, 0.1484F, 0F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.1511F, 0.1511F, 0.1511F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/AutoCastEquipment"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFossil"),
                            childName = "Bust",
                            localPos = new Vector3(-0.00821F, 0.02639F, -0.00027F),
                            localAngles = new Vector3(347.1046F, 29.72679F, 269.3189F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/IncreaseHealing"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.0166F, 0.00537F, -0.00799F),
                            localAngles = new Vector3(0.42134F, 175.9255F, 278.2194F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.0171F, 0.00706F, 0.0091F),
                            localAngles = new Vector3(14.67922F, 12.3438F, 72.866F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/TitanGoldDuringTP"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGoldHeart"),
childName = "dash",
localPos = new Vector3(-0.0571F, 0.3027F, 0.0755F),
localAngles = new Vector3(335.0033F, 343.2951F, 0F),
localScale = new Vector3(0.1191F, 0.1191F, 0.1191F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SprintWisp"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrokenMask"),
childName = "dash",
localPos = new Vector3(-0.0283F, 0.0452F, -0.0271F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(0.1385F, 0.1385F, 0.1385F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BarrierOnKill"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrooch"),
                            childName = "Bust",
                            localPos = new Vector3(-0.02705F, 0.01924F, 0.02586F),
                            localAngles = new Vector3(330.0003F, 100.5729F, 8.88253F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/TPHealingNova"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGlowFlower"),
childName = "dash",
localPos = new Vector3(0.0399F, 0.1684F, 0.0121F),
localAngles = new Vector3(0F, 73.1449F, 0F),
localScale = new Vector3(0.2731F, 0.2731F, 0.0273F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/LunarUtilityReplacement"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdFoot"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.02147F, -0.0172F, 0.00227F),
                            localAngles = new Vector3(336.0344F, 354.2175F, 73.6545F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Thorns"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRazorwireLeft"),
childName = "dash",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.4814F, 0.4814F, 0.4814F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/LunarPrimaryReplacement"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdEye"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.00367F, 0.01786F, 0.00503F),
                            localAngles = new Vector3(342.6146F, 188.5165F, 198.2434F),
                            localScale = new Vector3(0.015F, 0.015F, 0.015F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/NovaOnLowHealth"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayJellyGuts"),
                            childName = "ArmL",
                            localPos = new Vector3(-0.03028F, -0.00284F, 0.00647F),
                            localAngles = new Vector3(14.40792F, 136.4354F, 98.32927F),
                            localScale = new Vector3(0.02F, 0.02F, 0.02F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/LunarTrinket"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBeads"),
childName = "dash",
localPos = new Vector3(0F, 0.3249F, 0.0381F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Plant"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayInterstellarDeskPlant"),
childName = "dash",
localPos = new Vector3(-0.0663F, 0.2266F, 0F),
localAngles = new Vector3(4.9717F, 270F, 54.4915F),
localScale = new Vector3(0.0429F, 0.0429F, 0.0429F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Bear"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBear"),
                            childName = "Waist",
                            localPos = new Vector3(-0.00008F, 0.01773F, -0.01305F),
                            localAngles = new Vector3(287.223F, 173.6088F, 279.7755F),
                            localScale = new Vector3(0.025F, 0.025F, 0.025F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/DeathMark"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDeathMark"),
                            childName = "HandL",
                            localPos = new Vector3(-0.01407F, -0.00217F, 0.00295F),
                            localAngles = new Vector3(340.4736F, 107.3662F, 30.97315F),
                            localScale = new Vector3(0.003F, 0.003F, 0.003F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ExplodeOnDeath"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWilloWisp"),
                            childName = "Waist",
                            localPos = new Vector3(0.01097F, -0.00667F, -0.03911F),
                            localAngles = new Vector3(70.86782F, 357.6516F, 83.30996F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Seed"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySeed"),
                            childName = "Waist",
                            localPos = new Vector3(0.00403F, 0.00995F, 0.01817F),
                            localAngles = new Vector3(313.235F, 199.9487F, 265.1344F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SprintOutOfCombat"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWhip"),
childName = "dash",
localPos = new Vector3(0.1001F, -0.0132F, 0F),
localAngles = new Vector3(0F, 0F, 20.1526F),
localScale = new Vector3(0.2845F, 0.2845F, 0.2845F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Phasing"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStealthkit"),
childName = "dash",
localPos = new Vector3(-0.0063F, 0.2032F, -0.0507F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(0.1454F, 0.2399F, 0.16F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/PersonalShield"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldGenerator"),
childName = "dash",
localPos = new Vector3(0F, 0.2649F, 0.0689F),
localAngles = new Vector3(304.1204F, 90F, 270F),
localScale = new Vector3(0.1057F, 0.1057F, 0.1057F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ShockNearby"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeslaCoil"),
childName = "dash",
localPos = new Vector3(0.0008F, 0.3747F, -0.0423F),
localAngles = new Vector3(297.6866F, 1.3864F, 358.5596F),
localScale = new Vector3(0.3229F, 0.3229F, 0.3229F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ShieldOnly"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.02159F, 0.01004F, 0.00755F),
                            localAngles = new Vector3(51.64918F, 209.0314F, 270.7932F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.02274F, 0.00921F, -0.00663F),
                            localAngles = new Vector3(339.7332F, 126.4673F, 272.4875F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/AlienHead"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAlienHead"),
                            childName = "Waist",
                            localPos = new Vector3(-0.00239F, 0.00947F, 0.02009F),
                            localAngles = new Vector3(26.04111F, 109.755F, 264.8393F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/HeadHunter"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySkullCrown"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.02037F, 0.00811F, 0.00006F),
                            localAngles = new Vector3(66.69865F, 259.349F, 341.9424F),
                            localScale = new Vector3(0.04F, 0.015F, 0.015F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/EnergizedOnEquipmentUse"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarHorn"),
                            childName = "Waist",
                            localPos = new Vector3(0.00025F, 0.01996F, 0.02532F),
                            localAngles = new Vector3(0F, 0F, 69.9659F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FlatHealth"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySteakCurved"),
childName = "dash",
localPos = new Vector3(-0.02355F, 0.02687F, -0.01517F),
localAngles = new Vector3(304.7966F, 254.0776F, 190.1174F),
localScale = new Vector3(0.02F, 0.02F, 0.02F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Tooth"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayToothMeshLarge"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.00446F, 0.00651F, 0.00032F),
                            localAngles = new Vector3(344.9017F, 0F, 0F),
                            localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Pearl"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPearl"),
childName = "dash",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/ShinyPearl"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShinyPearl"),
childName = "dash",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BonusGoldPackOnKill"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTome"),
                            childName = "Waist",
                            localPos = new Vector3(0.00758F, 0.01043F, -0.03033F),
                            localAngles = new Vector3(310.8412F, 170.0328F, 272.9804F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Squid"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySquidTurret"),
                            childName = "Waist",
                            localPos = new Vector3(0.00084F, 0.01135F, -0.02151F),
                            localAngles = new Vector3(2.66104F, 95.12044F, 293.5565F),
                            localScale = new Vector3(0.005F, 0.005F, 0.005F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Icicle"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFrostRelic"),
childName = "dash",
localPos = new Vector3(-0.658F, -1.0806F, 0.015F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Talisman"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTalisman"),
childName = "dash",
localPos = new Vector3(0.8357F, -0.7042F, -0.2979F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/LaserTurbine"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLaserTurbine"),
childName = "dash",
localPos = new Vector3(0F, 0.0622F, -0.0822F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2159F, 0.2159F, 0.2159F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FocusConvergence"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFocusedConvergence"),
childName = "dash",
localPos = new Vector3(-0.0554F, -1.6605F, -0.3314F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/FireballsOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireballsOnHit"),
                            childName = "ShoulderR",
                            localPos = new Vector3(-0.00304F, -0.0117F, 0.02888F),
                            localAngles = new Vector3(20.38911F, 352.7317F, 251.9768F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/SiphonOnLowHealth"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySiphonOnLowHealth"),
childName = "dash",
localPos = new Vector3(0.0542F, 0.0206F, -0.0019F),
localAngles = new Vector3(0F, 303.4368F, 0F),
localScale = new Vector3(0.0385F, 0.0385F, 0.0385F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/BleedOnHitAndExplode"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBleedOnHitAndExplode"),
                            childName = "ShoulderL",
                            localPos = new Vector3(-0.00234F, 0.00916F, -0.00586F),
                            localAngles = new Vector3(14.6919F, 164.4924F, 357.1207F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/MonstersOnShrineUse"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMonstersOnShrineUse"),
childName = "dash",
localPos = new Vector3(0.0022F, 0.084F, 0.066F),
localAngles = new Vector3(352.4521F, 260.6884F, 341.5106F),
localScale = new Vector3(0.0246F, 0.0246F, 0.0246F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<ItemDef>("ItemDefs/RandomDamageZone"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRandomDamageZone"),
childName = "dash",
localPos = new Vector3(0.0709F, 0.4398F, 0.0587F),
localAngles = new Vector3(349.218F, 235.9453F, 0F),
localScale = new Vector3(0.0465F, 0.0465F, 0.0465F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Fruit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFruit"),
childName = "dash",
localPos = new Vector3(-0.0513F, 0.2348F, -0.1839F),
localAngles = new Vector3(354.7403F, 305.3714F, 336.9526F),
localScale = new Vector3(0.2118F, 0.2118F, 0.2118F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixRed"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
                            childName = "Bust",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(305.1317F, 28.21677F, 40.99422F),
                            localScale = new Vector3(0.025F, 0.025F, 0.025F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
                            childName = "Bust",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(304.6882F, 153.5537F, 316.4307F),
                            localScale = new Vector3(-0.025F, 0.025F, 0.025F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixBlue"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
                            childName = "ShoulderL",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(340.7975F, 165.5608F, 247.0858F),
                            localScale = new Vector3(0.05F, 0.05F, 0.05F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
                            childName = "ShoulderL",
                            localPos = new Vector3(0.01395F, 0.00313F, -0.00514F),
                            localAngles = new Vector3(327.9544F, 162.6673F, 261.1701F),
                            localScale = new Vector3(0.03F, 0.03F, 0.03F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixWhite"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteIceCrown"),
                            childName = "GanonHead",
                            localPos = new Vector3(-0.02396F, 0.00015F, 0.001F),
                            localAngles = new Vector3(342.6126F, 272.7209F, 189.5704F),
                            localScale = new Vector3(0.0025F, 0.0025F, 0.0025F),
                            limbMask = LimbFlags.Head
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixPoison"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteUrchinCrown"),
                            childName = "Bust",
                            localPos = new Vector3(0.01162F, 0.00184F, 0.00096F),
                            localAngles = new Vector3(67.14866F, 119.7638F, 130.2586F),
                            localScale = new Vector3(0.015F, 0.015F, 0.015F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/AffixHaunted"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteStealthCrown"),
                            childName = "ArmR",
                            localPos = new Vector3(0F, 0F, 0F),
                            localAngles = new Vector3(7.23032F, 117.0429F, 109.7033F),
                            localScale = new Vector3(0.005F, 0.005F, 0.005F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/CritOnUse"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayNeuralImplant"),
childName = "dash",
localPos = new Vector3(0F, 0.1861F, 0.2328F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2326F, 0.2326F, 0.2326F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/DroneBackup"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRadio"),
childName = "dash",
localPos = new Vector3(0.0604F, 0.1269F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.2641F, 0.2641F, 0.2641F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Lightning"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLightningArmRight"),
childName = "dash",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.3413F, 0.3413F, 0.3413F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/BurnNearby"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPotion"),
childName = "dash",
localPos = new Vector3(0.078F, 0.065F, 0F),
localAngles = new Vector3(359.1402F, 0.1068F, 331.8908F),
localScale = new Vector3(0.0307F, 0.0307F, 0.0307F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/CrippleWard"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEffigy"),
childName = "dash",
localPos = new Vector3(0.0768F, -0.0002F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(0.2812F, 0.2812F, 0.2812F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/QuestVolatileBattery"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBatteryArray"),
childName = "dash",
localPos = new Vector3(0F, 0.2584F, -0.0987F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2188F, 0.2188F, 0.2188F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/GainArmor"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayElephantFigure"),
childName = "dash",
localPos = new Vector3(0F, 0.3011F, 0.0764F),
localAngles = new Vector3(77.5634F, 0F, 0F),
localScale = new Vector3(0.6279F, 0.6279F, 0.6279F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Recycle"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRecycler"),
childName = "dash",
localPos = new Vector3(0F, 0.1976F, -0.0993F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.0802F, 0.0802F, 0.0802F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/FireBallDash"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEgg"),
childName = "dash",
localPos = new Vector3(0.0727F, 0.0252F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.1891F, 0.1891F, 0.1891F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Cleanse"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWaterPack"),
childName = "dash",
localPos = new Vector3(0F, 0.1996F, -0.0489F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.0821F, 0.0821F, 0.0821F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Tonic"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTonic"),
childName = "dash",
localPos = new Vector3(0.066F, 0.058F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.1252F, 0.1252F, 0.1252F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Gateway"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayVase"),
childName = "dash",
localPos = new Vector3(0.0807F, 0.0877F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.0982F, 0.0982F, 0.0982F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Meteor"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMeteor"),
childName = "dash",
localPos = new Vector3(0F, -1.7606F, -0.9431F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Saw"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySawmerang"),
childName = "dash",
localPos = new Vector3(0F, -1.7606F, -0.9431F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Blackhole"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravCube"),
childName = "dash",
localPos = new Vector3(0F, -1.7606F, -0.9431F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Scanner"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayScanner"),
childName = "dash",
localPos = new Vector3(0.0857F, 0.0472F, 0.0195F),
localAngles = new Vector3(270F, 154.175F, 0F),
localScale = new Vector3(0.0861F, 0.0861F, 0.0861F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/DeathProjectile"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDeathProjectile"),
childName = "dash",
localPos = new Vector3(0F, 0.028F, -0.0977F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.0596F, 0.0596F, 0.0596F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/LifestealOnHit"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLifestealOnHit"),
                            childName = "KneeL",
                            localPos = new Vector3(0.01619F, 0.01329F, 0.02472F),
                            localAngles = new Vector3(6.85632F, 223.9352F, 188.2641F),
                            localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/TeamWarCry"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeamWarCry"),
childName = "dash",
localPos = new Vector3(0F, 0F, 0.1866F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1233F, 0.1233F, 0.1233F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });

            itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = RoR2.LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/TeamWarCry"),
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = new ItemDisplayRule[]
                    {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeamWarCry"),
childName = "dash",
localPos = new Vector3(0F, 0F, 0.1866F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1233F, 0.1233F, 0.1233F),
                            limbMask = LimbFlags.None
                        }
                    }
                }
            });
            #endregion

            itemDisplayRuleSet.keyAssetRuleGroups = itemDisplayRules.ToArray();
            itemDisplayRuleSet.GenerateRuntimeValues();
        }

        private static CharacterModel.RendererInfo[] SkinRendererInfos(CharacterModel.RendererInfo[] defaultRenderers, Material[] materials)
        {
            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            
            defaultRenderers.CopyTo(newRendererInfos, 0);

            newRendererInfos[0].defaultMaterial = materials[0];
            newRendererInfos[instance.mainRendererIndex].defaultMaterial = materials[instance.mainRendererIndex];

            return newRendererInfos;
        }
    }
}