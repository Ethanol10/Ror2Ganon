using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GanondorfMod.Modules
{
    internal static class Skins
    {
        internal static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root)
        {
            return CreateSkinDef(skinName, skinIcon, rendererInfos, mainRenderer, root, null);
        }

        internal static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, UnlockableDef unlockableDef)
        {
            SkinDefInfo skinDefInfo = new SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                Icon = skinIcon,
                MeshReplacements = new SkinDef.MeshReplacement[0],
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableDef = unlockableDef
            };

            On.RoR2.SkinDef.Awake += DoNothing;

            SkinDef skinDef = ScriptableObject.CreateInstance<RoR2.SkinDef>();
            skinDef.baseSkins = skinDefInfo.BaseSkins;
            skinDef.icon = skinDefInfo.Icon;
            skinDef.unlockableDef = skinDefInfo.UnlockableDef;
            skinDef.rootObject = skinDefInfo.RootObject;
            skinDef.nameToken = skinDefInfo.NameToken;
            skinDef.name = skinDefInfo.Name;

            SkinDefParams skinDefParams = ScriptableObject.CreateInstance<RoR2.SkinDefParams>();
            skinDefParams.projectileGhostReplacements = new SkinDefParams.ProjectileGhostReplacement[0];
            skinDefParams.minionSkinReplacements = new SkinDefParams.MinionSkinReplacement[0];
            skinDefParams.meshReplacements = new SkinDefParams.MeshReplacement[0];
            skinDefParams.gameObjectActivations = new SkinDefParams.GameObjectActivation[0];
            rendererInfos.CopyTo(skinDefInfo.RendererInfos, 0);
            skinDefParams.rendererInfos = skinDefInfo.RendererInfos;

            skinDef.skinDefParams = skinDefParams;

            On.RoR2.SkinDef.Awake -= DoNothing;

            return skinDef;
        }

        internal static SkinDefParams.MeshReplacement[] getMeshReplacements(CharacterModel.RendererInfo[] defaultRendererInfos, params string[] meshes)
        {

            List<SkinDefParams.MeshReplacement> meshReplacements = new List<SkinDefParams.MeshReplacement>();

            for (int i = 0; i < defaultRendererInfos.Length; i++)
            {
                if (string.IsNullOrEmpty(meshes[i]))
                    continue;

                meshReplacements.Add(
                new SkinDefParams.MeshReplacement
                {
                    renderer = defaultRendererInfos[i].renderer,
                    mesh = Assets.mainAssetBundle.LoadAsset<Mesh>(meshes[i])
                });
            }

            return meshReplacements.ToArray();
        }

        private static void DoNothing(On.RoR2.SkinDef.orig_Awake orig, RoR2.SkinDef self)
        {

        }

        internal struct SkinDefInfo
        {
            internal SkinDef[] BaseSkins;
            internal Sprite Icon;
            internal string NameToken;
            internal UnlockableDef UnlockableDef;
            internal GameObject RootObject;
            internal CharacterModel.RendererInfo[] RendererInfos;
            internal SkinDef.MeshReplacement[] MeshReplacements;
            internal SkinDef.GameObjectActivation[] GameObjectActivations;
            internal SkinDef.ProjectileGhostReplacement[] ProjectileGhostReplacements;
            internal SkinDef.MinionSkinReplacement[] MinionSkinReplacements;
            internal string Name;
        }
    }
}