using BepInEx;
using BepInEx.Bootstrap;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Survivors;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace GanondorfMod
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [R2APISubmoduleDependency(new string[]
    {
        "PrefabAPI",
        "LanguageAPI",
        "SoundAPI",
    })]

    public class GanondorfPlugin : BaseUnityPlugin
    {
        // if you don't change these you're giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.Ethanol10.Ganondorf";
        public const string MODNAME = "Ganondorf";
        public const string MODVERSION = "0.0.1";
        
        //Triforce Buff
        public static TriforceBuffComponent triforceBuff;
        private CharacterBody ganondorfCharacterBody;
        public static GanondorfController ganondorfController;

        //Scepter Vars
        public static bool scepterInstalled = false;
        private static float triforceMaxArmour = 30f;

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string developerPrefix = "ETHA10";

        internal List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static GanondorfPlugin instance;

        private void Awake()
        {
            instance = this;
            GanondorfPlugin.instance = this;

            //make triforcebuff and ganondorfcharacterbody null for now.
            triforceBuff = null;
            ganondorfCharacterBody = null;

            //Check for ancient scepter plugin
            if (Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter"))
            {
                scepterInstalled = true;
            }

            // load assets and read config
            Modules.Assets.Initialize();
            Modules.Config.ReadConfig();
            Modules.States.RegisterStates(); // register states for networking
            Modules.Buffs.RegisterBuffs(); // add and register custom buffs/debuffs
            Modules.Projectiles.RegisterProjectiles(); // add and register custom projectiles
            Modules.Tokens.AddTokens(); // register name tokens
            Modules.ItemDisplays.PopulateDisplays(); // collect item display prefabs for use in our display rules

            // survivor initialization
            new Ganondorf().Initialize();

            // now make a content pack and add it- this part will change with the next update
            new Modules.ContentPacks().Initialize();

            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;

            Hook();
        }

        private void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            // have to set item displays later now because they require direct object references..
            Modules.Survivors.Ganondorf.instance.SetItemDisplays();
        }

        private void Hook()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            // run hooks here, disabling one is as simple as commenting out the line
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            On.RoR2.GenericPickupController.GrantItem += GenericPickupController_GrantItem;
        }

        private void GenericPickupController_GrantItem(On.RoR2.GenericPickupController.orig_GrantItem orig, GenericPickupController self, CharacterBody body, Inventory inventory) {
            orig(self, body, inventory);
            if (body.baseNameToken == developerPrefix + "_GANONDORF_BODY_NAME") {
                if (self.pickupIndex.ToString() == "ItemIndex.ITEM_ANCIENT_SCEPTER") {
                    body.gameObject.GetComponent<TriforceBuffComponent>().SetScepterActive(true);
                }
            }

        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);

            //Update buffCount every frame render.
            if (self.baseNameToken == developerPrefix + "_GANONDORF_BODY_NAME")
            {
                self.SetBuffCount(Modules.Buffs.triforceBuff.buffIndex, triforceBuff.GetBuffCount());
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            // a simple stat hook, adds armor after stats are recalculated
            if (self)
            {
                if (self.HasBuff(Modules.Buffs.armorBuff))
                {
                    self.armor += 300f;
                }
                if (self.HasBuff(Modules.Buffs.triforceBuff)) {
                    if (triforceBuff.GetScepterState()) {
                        self.armor += triforceBuff.GetBuffCount()*(triforceMaxArmour/100);
                    }
                }
            }
        }

        private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self) {
            orig(self);
            if (self.baseNameToken == developerPrefix + "_GANONDORF_BODY_NAME") {
                Util.PlaySound("ganonDying", self.gameObject);
            }
        }

        private void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self) {
            orig(self);
            if (self.gameObject.name.Contains("GanondorfDisplay")) 
            {
                //Load the portal effect, push the portal back a little bit on the x-axis and have ganon walk through it.
                GameObject portalEffect = Resources.Load<GameObject>("prefabs/effects/NullifierSpawnEffect");
                Vector3 pos = self.gameObject.transform.position;
                Quaternion rot = self.gameObject.transform.rotation;
                rot = Quaternion.Euler(0, 90, 0);
                pos.x += 1.5f;
                pos.z -= 1.5f;
                EffectData effectData = new EffectData
                {
                    origin = pos,
                    rotation = rot
                };
                EffectManager.SpawnEffect(portalEffect, effectData, false);
                Util.PlaySound("spawnVoice", self.gameObject);
                Util.PlaySound("Spawning", self.gameObject);
            }
        }
        
    }
}