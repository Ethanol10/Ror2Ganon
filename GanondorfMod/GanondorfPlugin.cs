using BepInEx;
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

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string developerPrefix = "ETHA10";

        internal List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static GanondorfPlugin instance;

        private void Awake()
        {
            instance = this;

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