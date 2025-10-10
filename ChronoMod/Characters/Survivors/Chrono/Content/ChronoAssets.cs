using ChronoMod.Modules;
using ChronoMod.Modules.DamageTypes;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono {
    public static class ChronoAssets {
        // particle effects
        public static GameObject swordSwingEffect;

        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        //projectiles
        public static GameObject bombProjectilePrefab;

        public static GameObject throwProjectilePrefab;

        public static GameObject horizonProjectilePrefab;

        public static GameObject continuumWardPrefab;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle) {

            _assetBundle = assetBundle;

            ChronoPlugin.instance.StartCoroutine(ShaderSwapper.ShaderSwapper.UpgradeStubbedShadersAsync(assetBundle));

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("HenrySwordHit");

            CreateEffects();

            CreateProjectiles();
        }

        #region effects
        private static void CreateEffects() {
            CreateBombExplosionEffect();

            swordSwingEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Merc.MercSwordSlash_prefab).WaitForCompletion();// _assetBundle.LoadEffect("HenrySwordSwingEffect", true);
            EffectComponent effect = swordSwingEffect.AddComponent<EffectComponent>(); // why does it not have one by default. how queer
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = true;
            effect.positionAtReferencedTransform = true;
            Content.CreateAndAddEffectDef(swordSwingEffect);

            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactHenrySlash");

            continuumWardPrefab = _assetBundle.LoadAsset<GameObject>("ContinuumWard");
            continuumWardPrefab.GetComponent<BuffWard>().buffDef = ChronoBuffs.continuumFreezeBuff;
        }

        private static void CreateBombExplosionEffect() {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "HenryBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles() {
            CreateBombProjectile();
            Content.AddProjectilePrefab(bombProjectilePrefab);

            throwProjectilePrefab = Asset.LoadAndAddProjectilePrefab(_assetBundle, "ThrowProjectile");

            horizonProjectilePrefab = Asset.LoadAndAddProjectilePrefab(_assetBundle, "HorizonProjectile");
        }

        private static void CreateBombProjectile() {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            bombProjectilePrefab = Asset.CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(bombProjectilePrefab.GetComponent<ProjectileImpactExplosion>());
            ProjectileImpactExplosion bombImpactExplosion = bombProjectilePrefab.AddComponent<ProjectileImpactExplosion>();

            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.blastDamageCoefficient = 1f;
            bombImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = bombExplosionEffect;
            bombImpactExplosion.lifetimeExpiredSound = Content.CreateAndAddNetworkSoundEventDef("HenryBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombProjectilePrefab.GetComponent<ProjectileController>();

            if (_assetBundle.LoadAsset<GameObject>("HenryBombGhost") != null)
                bombController.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("HenryBombGhost");

            bombController.startSound = "";
        }
        #endregion projectiles

        public static void AssignDamageTypes() {
            throwProjectilePrefab.GetComponent<ProjectileDamage>().damageType.AddModdedDamageType(TemporalRiftType.damageType);

            horizonProjectilePrefab.GetComponent<ProjectileDamage>().damageType.AddModdedDamageType(EventHorizonType.damageType);
        }
    }
}
