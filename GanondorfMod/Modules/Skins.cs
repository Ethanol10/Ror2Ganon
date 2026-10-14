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
                GameObjectActivations = Array.Empty<SkinDefParams.GameObjectActivation>(),
                Icon = skinIcon,
                MeshReplacements = Array.Empty<SkinDefParams.MeshReplacement>(),
                MinionSkinReplacements = Array.Empty<SkinDefParams.MinionSkinReplacement>(),
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = Array.Empty<SkinDefParams.ProjectileGhostReplacement>(),
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableDef = unlockableDef
            };

            SkinDef skinDef = ScriptableObject.CreateInstance<RoR2.SkinDef>();
            skinDef.baseSkins = skinDefInfo.BaseSkins;
            skinDef.icon = skinDefInfo.Icon;
            skinDef.unlockableDef = skinDefInfo.UnlockableDef;
            skinDef.rootObject = skinDefInfo.RootObject;
            skinDef.nameToken = skinDefInfo.NameToken;
            skinDef.name = skinDefInfo.Name;

            SkinDefParams skinDefParams = ScriptableObject.CreateInstance<SkinDefParams>();
            skinDefParams.name = skinName + "Params";
            skinDefParams.rendererInfos = skinDefInfo.RendererInfos;
            skinDefParams.gameObjectActivations = skinDefInfo.GameObjectActivations;
            skinDefParams.meshReplacements = skinDefInfo.MeshReplacements;
            skinDefParams.projectileGhostReplacements = skinDefInfo.ProjectileGhostReplacements;
            skinDefParams.minionSkinReplacements = skinDefInfo.MinionSkinReplacements;
            skinDefParams.lightReplacements = Array.Empty<CharacterModel.LightInfo>();
            skinDef.skinDefParams = skinDefParams;

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

        internal struct SkinDefInfo
        {
            internal SkinDef[] BaseSkins;
            internal Sprite Icon;
            internal string NameToken;
            internal UnlockableDef UnlockableDef;
            internal GameObject RootObject;
            internal CharacterModel.RendererInfo[] RendererInfos;
            internal SkinDefParams.MeshReplacement[] MeshReplacements;
            internal SkinDefParams.GameObjectActivation[] GameObjectActivations;
            internal SkinDefParams.ProjectileGhostReplacement[] ProjectileGhostReplacements;
            internal SkinDefParams.MinionSkinReplacement[] MinionSkinReplacements;
            internal string Name;
        }
    }
}
