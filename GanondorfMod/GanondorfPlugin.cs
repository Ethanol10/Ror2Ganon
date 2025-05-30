﻿using BepInEx;
using BepInEx.Bootstrap;
using GanondorfMod.Modules;
using GanondorfMod.Modules.Survivors;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using EmotesAPI;
using R2API.Networking;
using GanondorfMod.Modules.Networking;

#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace GanondorfMod
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.sound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.networking", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.ClassicItems", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]

    public class GanondorfPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Ethanol10.Ganondorf";
        public const string MODNAME = "Ganondorf";
        public const string MODVERSION = "3.2.2";
        
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
                GanondorfPlugin.scepterInstalled = true;
            }
            Modules.Config.ReadConfig();
            Modules.Config.OnChangeHooks();
            // load assets and read config
            Modules.Assets.Initialize();
            if (Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                Modules.Config.SetupRiskOfOptions();
            }
            
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
            SetupNetworkMessages();
        }

        private void LateSetup()
        {
            // have to set item displays later now because they require direct object references..
            Modules.Survivors.Ganondorf.instance.SetItemDisplays();
        }

        private void SetupNetworkMessages()
        {
            NetworkingAPI.RegisterMessageType<FullyChargedSwordNetworkRequest>();
            NetworkingAPI.RegisterMessageType<ChargingSwordNetworkRequest>();
            NetworkingAPI.RegisterMessageType<SwordBeamRegenerateStocksNetworkRequest>();
            NetworkingAPI.RegisterMessageType<PlaySoundNetworkRequest>();
            NetworkingAPI.RegisterMessageType<StopSoundEventNetworkRequest>();
        }

        private void Hook()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            On.RoR2.CharacterModel.Start += CharacterModel_Start;

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;

            if (Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI")) 
            {
                On.RoR2.SurvivorCatalog.Init += SurvivorCatalog_Init;
            }
        }

        private void SurvivorCatalog_Init(On.RoR2.SurvivorCatalog.orig_Init orig) 
        {
            orig();
            foreach (var item in SurvivorCatalog.allSurvivorDefs)
            {
                if (item.bodyPrefab.name == "GanondorfBody")
                {
                    CustomEmotesAPI.ImportArmature(item.bodyPrefab, Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("humanoidGanondorf"));
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo info) 
        {
            orig(self, info);
            if (info.attacker) 
            {
                if (self) 
                {
                    if (self.body) 
                    {
                        if (self.body.HasBuff(Modules.Buffs.damageAbsorberBuff)) 
                        {
                            self.body.AddBuff(Modules.Buffs.damageAbsorberBuff);
                        }
                    }
                }
            }
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
                if (self.HasBuff(Modules.Buffs.damageAbsorberBuff)) 
                {
                    self.armor += self.GetBuffCount(Modules.Buffs.damageAbsorberBuff) * Modules.StaticValues.obliterateBuffArmourMultiplier;
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
                if (AkSoundEngine.IsInitialized())
                {
                    AkSoundEngine.SetRTPCValue("Volume_GanonVoice", Modules.Config.voiceVolume.Value);
                    AkSoundEngine.SetRTPCValue("Volume_GanonSFX", Modules.Config.sfxVolume.Value);
                }

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

        private void CharacterModel_Start(On.RoR2.CharacterModel.orig_Start orig, CharacterModel self)
        {
            orig(self);
            if (self.gameObject.name.Contains("GanondorfDisplay"))
            {
                GanondorfDisplaySwordController displayHandler = self.gameObject.GetComponent<GanondorfDisplaySwordController>();
                if (!displayHandler) 
                {
                    ChildLocator childLoc = self.gameObject.GetComponent<ChildLocator>();

                    if (childLoc) 
                    {
                        Transform swordMesh = childLoc.FindChild("SwordMeshContainer");
                        Transform HandLTrans = childLoc.FindChild("SwordHandLLoc");
                        Transform bustTrans = childLoc.FindChild("SwordBustLoc");

                        displayHandler = self.gameObject.AddComponent<GanondorfDisplaySwordController>();
                        displayHandler.handLoc = HandLTrans;
                        displayHandler.meshLoc = swordMesh;
                        displayHandler.bustLoc = bustTrans;
                        displayHandler.targetTrans = bustTrans;
                    }
                }
            }
        }

        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);

            if (self)
            {
                if (self.body)
                {
                    GanondorfController ganoncon = self.body.GetComponent<GanondorfController>();
                    if (ganoncon) 
                    {
                        this.LiterallyGarbageOverlayFunction(Modules.Assets.chargingMat,
                                                            ganoncon.chargingSword,
                                                            self);
                        this.LiterallyGarbageOverlayFunction(Modules.Assets.fullyChargedMat,
                                                            ganoncon.swordFullyCharged,
                                                            self);
                    }
                }
            }
        }

        private void LiterallyGarbageOverlayFunction(Material overlayMaterial, bool condition, CharacterModel model)
        {
            if (model.activeOverlayCount >= CharacterModel.maxOverlays)
            {
                return;
            }
            if (condition)
            {
                Material[] array = model.currentOverlays;
                int num = model.activeOverlayCount;
                model.activeOverlayCount = num + 1;
                array[num] = overlayMaterial;
            }
        }
    }
}