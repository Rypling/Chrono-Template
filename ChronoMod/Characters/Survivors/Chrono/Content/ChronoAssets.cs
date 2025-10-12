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
        #endregion effects

        #region projectiles
        private static void CreateProjectiles() {
            throwProjectilePrefab = Asset.LoadAndAddProjectilePrefab(_assetBundle, "ThrowProjectile");
            throwProjectilePrefab.GetComponent<ProjectileImpactExplosion>().explosionEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_Junk_Common_VFX.ImpactLightning_prefab).WaitForCompletion();

            horizonProjectilePrefab = Asset.LoadAndAddProjectilePrefab(_assetBundle, "HorizonProjectile");
            horizonProjectilePrefab.GetComponent<ProjectileController>().ghostPrefab = CreateEventHorizonGhost();
            horizonProjectilePrefab.GetComponent<ProjectileImpactExplosion>().explosionEffect = CreateEventHorizonExplosion();
        }

        private static GameObject CreateEventHorizonGhost() {
            GameObject horizonProjectileGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_Seeker.SpiritPunchFinisherGhost_prefab).WaitForCompletion(), "HorizonProjectileGhost");

            ParticleSystem.MainModule main;

            // Sphere Fresnal
            Transform tSphereFresnel = horizonProjectileGhost.transform.Find("Sphere Fresnel");
            main = tSphereFresnel.GetComponent<ParticleSystem>().main;
            main.loop = true;
            main.startLifetime = 4f;
            Material matSphereFresnel = new Material(tSphereFresnel.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matSphereFresnel.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
            matSphereFresnel.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_DLC2_FalseSonBoss.texFSBLunarSpikeRamp_png).WaitForCompletion());
            tSphereFresnel.GetComponent<ParticleSystemRenderer>().sharedMaterial = matSphereFresnel;

            // Sphere
            Transform tSphere = horizonProjectileGhost.transform.Find("Sphere Fresnel/Sphere");
            main = tSphere.GetComponent<ParticleSystem>().main;
            main.loop = true;
            main.startLifetime = 4f;
            Material matSphere = new Material(tSphere.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matSphere.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
            matSphere.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 1f));
            tSphere.GetComponent<ParticleSystemRenderer>().sharedMaterial = matSphere;

            // Sphere. Waves
            Transform tSphereWaves = horizonProjectileGhost.transform.Find("Sphere Fresnel/Sphere. Waves");
            Object.Destroy(tSphereWaves.gameObject);

            // Sphere, Trail
            Transform tSphereTrail = horizonProjectileGhost.transform.Find("Sphere Fresnel/Sphere, Trail");
            Object.Destroy(tSphereTrail.gameObject);

            // Swirl
            Transform tSwirl = horizonProjectileGhost.transform.Find("Sphere Fresnel/Swirl");
            main = tSwirl.GetComponent<ParticleSystem>().main;
            main.loop = true;
            main.startLifetime = 4f;
            Material matSwirl = new Material(tSwirl.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matSwirl.SetColor("_TintColor", new Color(0.56f, 0.83f, 1f, 1f));
            matSwirl.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 1f));
            tSwirl.GetComponent<ParticleSystemRenderer>().sharedMaterial = matSwirl;

            // Splashes, Trail
            Transform tSplashesTrail = horizonProjectileGhost.transform.Find("Sphere Fresnel/Splashes, Trail");
            Object.Destroy(tSplashesTrail.gameObject);

            // Petals
            Transform tPetals = horizonProjectileGhost.transform.Find("Sphere Fresnel/Petals");
            main = tPetals.GetComponent<ParticleSystem>().main;
            main.loop = true;
            Material matPetals = new Material(tPetals.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matPetals.SetColor("_TintColor", new Color(0.31f, 0.34f, 1f, 1f));
            tPetals.GetComponent<ParticleSystemRenderer>().sharedMaterial = matPetals;

            // Point Light
            Transform tPointLight = horizonProjectileGhost.transform.Find("Point Light");
            tPointLight.GetComponent<Light>().color = new Color(0.86f, 0.98f, 1f, 1f);

            return horizonProjectileGhost;
        }

        private static GameObject CreateEventHorizonExplosion() {
            GameObject horizonProjectileExplosion = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_Seeker.SoulSearchExplosionVFX_prefab).WaitForCompletion(), "HorizonProjectileExplosion", false);
            // Flash, White
            Transform tFlashWhite = horizonProjectileExplosion.transform.Find("Flash, White");
            Material matFlashWhite = new Material(tFlashWhite.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            // matSphereFresnel.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
            matFlashWhite.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_ColorRamps.texRampBrotherPillar_png).WaitForCompletion());
            tFlashWhite.GetComponent<ParticleSystemRenderer>().sharedMaterial = matFlashWhite;

            // Sphere
            Transform tSphere = horizonProjectileExplosion.transform.Find("Flash, White/Sphere");
            Material matSphere = new Material(tSphere.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matSphere.SetColor("_TintColor", new Color(0.04f, 0.15f, 0.64f, 1f));
            matSphere.SetColor("_EmissionColor", new Color(0.05f, 0.05f, 0.61f, 1f));
            tSphere.GetComponent<ParticleSystemRenderer>().sharedMaterial = matSphere;

            // Dissapate, Swipes
            Transform tDissapateSwipes = horizonProjectileExplosion.transform.Find("Flash, White/Dissapate, Swipes");
            Material matDissapateSwipes = new Material(tDissapateSwipes.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matDissapateSwipes.SetColor("_TintColor", new Color(0.38f, 0.60f, 1f, 0.87f));
            matSphere.SetColor("_EmissionColor", new Color(0.095f, 0.29f, 0.37f, 1f));
            tDissapateSwipes.GetComponent<ParticleSystemRenderer>().sharedMaterial = matDissapateSwipes;

            // Petals
            Transform tPetals = horizonProjectileExplosion.transform.Find("Flash, White/Petals");
            Object.Destroy(tPetals.gameObject);
            Material matPetals = new Material(tPetals.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            tPetals.GetComponent<ParticleSystemRenderer>().sharedMaterial = matPetals;

            // Distortion
            // Transform tDistortion = horizonProjectileExplosion.transform.Find("Flash, White/Distortion");
            // Material matDistortion = new Material(tDistortion.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            // tDistortion.GetComponent<ParticleSystemRenderer>().sharedMaterial = matDistortion;

            // Splashes
            Transform tSplashes = horizonProjectileExplosion.transform.Find("Flash, White/Splashes");
            Material matSplashes = new Material(tSplashes.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matSplashes.SetColor("_TintColor", new Color(0f, 0f, 1f, 1f));
            tSplashes.GetComponent<ParticleSystemRenderer>().sharedMaterial = matSplashes;

            // Dash, Bright
            // Transform tDashBright = horizonProjectileExplosion.transform.Find("Flash, White/Dash, Bright");
            // Material matDashBright = new Material(tDashBright.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            // tDashBright.GetComponent<ParticleSystemRenderer>().sharedMaterial = matDashBright;

            // Point Light
            Transform tPointLight = horizonProjectileExplosion.transform.Find("Point Light");
            tPointLight.GetComponent<Light>().color = new Color(0.86f, 0.98f, 1f, 1f);

            Content.CreateAndAddEffectDef(horizonProjectileExplosion);

            return horizonProjectileExplosion;
        }

        #endregion projectiles

        public static void AssignDamageTypes() {
            throwProjectilePrefab.GetComponent<ProjectileDamage>().damageType.AddModdedDamageType(TemporalRiftType.damageType);

            horizonProjectilePrefab.GetComponent<ProjectileDamage>().damageType.AddModdedDamageType(EventHorizonType.damageType);
        }
    }
}
