using BepInEx;
using BepInEx.Bootstrap;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Survivors;
using GanondorfMod.SkillStates;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete
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
        public const string MODUID = "com.Ethanol10.Ganondorf";
        public const string MODNAME = "Ganondorf";
        public const string MODVERSION = "2.1.3";
        
        //Triforce Buff
        public static TriforceBuffComponent triforceBuff;
        public static GanondorfController ganondorfController;

        //Scepter Vars
        public static bool scepterInstalled = false;

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

            RoR2Application.onLoad += LateSetup;

            Hook();
        }

        private void LateSetup()
        {
            // have to set item displays later now because they require direct object references..
            Modules.Survivors.Ganondorf.instance.SetItemDisplays();
        }

        private void Hook()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker != null && damageInfo != null) {
                if (damageInfo.attacker.name.Contains("GanondorfBody")){
                    GanondorfController ganonCon = damageInfo.attacker.GetComponent<GanondorfController>();

                    if (ganonCon) {
                        ganonCon.SetMaxDamage(damageInfo.damage);
                    }
                }
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) {
            orig(self, damageReport);
            if (damageReport.attackerBody != null && damageReport.attacker != null && damageReport != null) {
                if (damageReport.attackerBody.baseNameToken == developerPrefix + "_GANONDORF_BODY_NAME")
                {
                    //Add to Counters to check how much buff should be applied after killing an enemy
                    int amountToAdd = 0;
                    if (damageReport.victimIsBoss)
                    {
                        amountToAdd += Modules.StaticValues.bossKillStackAmount;
                    }
                    if (damageReport.victimIsChampion)
                    {
                        amountToAdd += Modules.StaticValues.championKillStackAmount;
                    }
                    if (damageReport.victimIsElite)
                    {
                        amountToAdd += Modules.StaticValues.eliteKillStackAmount;
                    }

                    if (!damageReport.victimIsBoss && !damageReport.victimIsElite && !damageReport.victimIsChampion)
                    {
                        amountToAdd += Modules.StaticValues.normalKillStackAmount;
                    }

                    damageReport.attacker.GetComponent<TriforceBuffComponent>().AddToBuffCount(amountToAdd);
                }
            }
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);

            //Update buffCount every frame render.
            if (self.baseNameToken == developerPrefix + "_GANONDORF_BODY_NAME")
            {
                self.SetBuffCount(Modules.Buffs.triforceBuff.buffIndex, self.GetComponent<TriforceBuffComponent>().GetTrueBuffCount());
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

                //Add armour if buff is available.
                if (self.HasBuff(Modules.Buffs.triforceBuff)) {
                    TriforceBuffComponent triforceBuffComponent = self.GetComponent<TriforceBuffComponent>();
                    self.armor += triforceBuffComponent.GetBuffCount()*(Modules.StaticValues.triforceMaxArmour/ Modules.StaticValues.maxPowerStack);
                    self.damage += triforceBuffComponent.GetBuffCount() * (Modules.StaticValues.triforceMaxDamage / Modules.StaticValues.maxPowerStack);
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
                GameObject portalEffect = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/NullifierSpawnEffect");
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