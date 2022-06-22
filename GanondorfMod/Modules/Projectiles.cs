using GanondorfMod.Modules.Networking;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.Modules
{
    internal static class Projectiles
    {
        internal static GameObject bombPrefab;
        internal static GameObject swordbeamProjectile;

        internal static void RegisterProjectiles()
        {
            // only separating into separate methods for my sanity
            CreateSwordBeam();

            AddProjectile(swordbeamProjectile);
        }

        internal static void AddProjectile(GameObject projectileToAdd)
        {
            Modules.Prefabs.projectilePrefabs.Add(projectileToAdd);
        }

        private static void CreateSwordBeam()
        {
            swordbeamProjectile = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("SwordBeam");
            // Ensure that the child is set in the right position in Unity!!!!
            Modules.Prefabs.SetupHitbox(swordbeamProjectile, swordbeamProjectile.transform.GetChild(0), "swordbeam");
            swordbeamProjectile.AddComponent<NetworkIdentity>();
            ProjectileController swordProjectileController = swordbeamProjectile.AddComponent<ProjectileController>();

            ProjectileDamage swordbeamProjectileDamage = swordbeamProjectile.AddComponent<ProjectileDamage>();
            InitializeSwordBeamDamage(swordbeamProjectileDamage);

            ProjectileSimple swordbeamTrajectory = swordbeamProjectile.AddComponent<ProjectileSimple>();
            InitializeSwordBeamTrajectory(swordbeamTrajectory);

            ProjectileOverlapAttack swordbeamOverlapAttack = swordbeamProjectile.AddComponent<ProjectileOverlapAttack>();
            InitializeSwordBeamOverlapAttack(swordbeamOverlapAttack);
            swordbeamProjectile.AddComponent<SwordbeamOnHit>();

            //ProjectileImpactExplosion waterbladeProjectileImpactExplosion = waterbladeProjectile.AddComponent<ProjectileImpactExplosion>();
            //Modules.Projectiles.InitializeImpactExplosion(waterbladeProjectileImpactExplosion);

            //Waterblade Damage
            swordProjectileController.procCoefficient = 1.0f;
            swordProjectileController.canImpactOnTrigger = true;

            PrefabAPI.RegisterNetworkPrefab(swordbeamProjectile);
        }

        internal static void InitializeSwordBeamOverlapAttack(ProjectileOverlapAttack overlap)
        {
            overlap.overlapProcCoefficient = 1.0f;
            overlap.damageCoefficient = 1.0f;
            //overlap.impactEffect = Modules.Assets.waterbladeimpactEffect;
        }

        internal static void InitializeSwordBeamTrajectory(ProjectileSimple simple)
        {
            simple.lifetime = Modules.StaticValues.swordBeamLifetime;
            simple.desiredForwardSpeed = Modules.StaticValues.swordBeamProjectileSpeed;

        }

        internal static void InitializeSwordBeamDamage(ProjectileDamage damageComponent)
        {
            damageComponent.damage = Modules.StaticValues.swordBeamDamageCoefficientBase;
            damageComponent.crit = false;
            damageComponent.force = Modules.StaticValues.swordBeamForce;
            damageComponent.damageType = DamageType.Generic;
        }

        private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
        {
            projectileImpactExplosion.blastDamageCoefficient = 1f;
            projectileImpactExplosion.blastProcCoefficient = 1f;
            projectileImpactExplosion.blastRadius = 1f;
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.childrenDamageCoefficient = 0f;
            projectileImpactExplosion.childrenProjectilePrefab = null;
            projectileImpactExplosion.destroyOnEnemy = false;
            projectileImpactExplosion.destroyOnWorld = false;
            projectileImpactExplosion.explosionSoundString = "";
            projectileImpactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.impactEffect = null;
            projectileImpactExplosion.lifetime = 0f;
            projectileImpactExplosion.lifetimeAfterImpact = 0f;
            projectileImpactExplosion.lifetimeExpiredSoundString = "";
            projectileImpactExplosion.lifetimeRandomOffset = 0f;
            projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
            projectileImpactExplosion.timerAfterImpact = false;

            projectileImpactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
        }

        private static GameObject CreateGhostPrefab(string ghostName)
        {
            GameObject ghostPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>(ghostName);
            if (!ghostPrefab.GetComponent<NetworkIdentity>()) ghostPrefab.AddComponent<NetworkIdentity>();
            if (!ghostPrefab.GetComponent<ProjectileGhostController>()) ghostPrefab.AddComponent<ProjectileGhostController>();

            Modules.Assets.ConvertAllRenderersToHopooShader(ghostPrefab);

            return ghostPrefab;
        }

        private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/" + prefabName), newPrefabName);
            return newPrefab;
        }

        internal class SwordbeamOnHit : MonoBehaviour, IProjectileImpactBehavior
        {
            public NetworkInstanceId netID;

            public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
            {
                //Implement incrementing of ganon stacks on hit.
                new SwordBeamRegenerateStocksNetworkRequest(netID).Send(NetworkDestination.Clients);
            }
        }
    }
}