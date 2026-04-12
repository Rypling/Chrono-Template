using ChronoMod.Characters.Survivors.Chrono.Components;
using ChronoMod.Modules;
using ChronoMod.Modules.DamageTypes;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono {
    public static class ChronoAssets {

        public static GameObject swordSwingEffect;

        public static GameObject swordHitImpactEffect;

        public static GameObject throwProjectileExplosionEffect;

        public static NetworkSoundEventDef swordHitSoundEvent;

        public static GameObject throwProjectilePrefab;

        public static GameObject horizonProjectilePrefab;

        public static GameObject continuumWardPrefab;

        public static GameObject continuumEndEffect;

        public static GameObject echoPortalPrefab;

        public static GameObject echoVortexPrefab;

        public static GameObject collapseVacuumPrefab;

        public static GameObject collapseExplosionPrefab;

        public static GameObject collapseProjectile;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle) {

            _assetBundle = assetBundle;

            ChronoPlugin.instance.StartCoroutine(ShaderSwapper.ShaderSwapper.UpgradeStubbedShadersAsync(assetBundle));

            CreateSounds();

            CreateEffects();

            CreateProjectiles();
        }

        private static void CreateSounds() {
            swordHitSoundEvent = Addressables.LoadAssetAsync<NetworkSoundEventDef>(RoR2_Base_Merc.nseMercSwordImpact_asset).WaitForCompletion();
        }

        #region effects
        private static void CreateEffects() {
            CreateSwordSwing();

            swordHitImpactEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Merc.OmniImpactVFXSlashMerc_prefab).WaitForCompletion();

            CreateContinuumWard();
            CreateContinuumEnd();

            CreateEchoPortal();
            CreateEchoVortex();

            CreateCollapseVacuum();
            CreateCollapseExplosion();
        }

        private static void CreateContinuumWard() {
            continuumWardPrefab = _assetBundle.LoadAsset<GameObject>("ContinuumWard");
            continuumWardPrefab.GetComponent<BuffWard>().buffDef = ChronoBuffs.continuumFreezeBuff;
            continuumWardPrefab.AddComponent<ContinuumFreezeController>();

            ChildLocator wardChildren = continuumWardPrefab.GetComponent<ChildLocator>();
            wardChildren.FindChild("Sphere").GetComponent<MeshFilter>().mesh = Addressables.LoadAssetAsync<Mesh>(RoR2_Base_Pearl.mdlPearl_fbx_Sphere__Unwrapped_).WaitForCompletion();

            PrefabAPI.RegisterNetworkPrefab(continuumWardPrefab);
        }

        private static void CreateContinuumEnd() {
            continuumEndEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Engi.BubbleShieldEndEffect_prefab).WaitForCompletion(), "ContinuumEndEffect", false);

            Object.Destroy(continuumEndEffect.transform.Find("OmniExplosionVFXEngiTurretDeath").gameObject);

            Transform tParticleSphere = continuumEndEffect.transform.Find("ParticleSphere");
            tParticleSphere.GetComponent<ParticleSystemRenderer>().sharedMaterial = _assetBundle.LoadMaterial("matContinuumSphereIndicator");

            Transform tQuads = continuumEndEffect.transform.Find("Quads");
            Material matQuads = new Material(tQuads.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matQuads.SetTexture("_RemapTex", _assetBundle.LoadAsset<Texture>("texRampContinuumWard"));
            tQuads.GetComponent<ParticleSystemRenderer>().sharedMaterial = matQuads;

            Content.CreateAndAddEffectDef(continuumEndEffect);
        }

        private static void CreateSwordSwing() {
            swordSwingEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Merc.MercSwordSlash_prefab).WaitForCompletion();
            EffectComponent effect = swordSwingEffect.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = true;
            effect.positionAtReferencedTransform = true;

            Content.CreateAndAddEffectDef(swordSwingEffect);
        }

        private static void CreateEchoPortal() {
            ParticleSystem settings = _assetBundle.LoadAsset<GameObject>("PortalParticleSettings").GetComponent<ParticleSystem>();

            echoPortalPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierSpawnEffect_prefab).WaitForCompletion(), "EchoPortal", false);

            Transform tVacuumStars = echoPortalPrefab.transform.Find("Vacuum Stars");
            Object.Destroy(tVacuumStars.gameObject);
            Transform tVacuumStarsTrails = echoPortalPrefab.transform.Find("Vacuum Stars, Trails");
            Object.Destroy(tVacuumStarsTrails.gameObject);
            Transform tVacuumRadial = echoPortalPrefab.transform.Find("Vacuum Radial");
            Object.Destroy(tVacuumRadial.gameObject);
            Transform tLight = echoPortalPrefab.transform.Find("Point light");
            Light pointLight = tLight.GetComponent<Light>();
            pointLight.range = 15f;
            pointLight.color = new Color(0f, 0.78f, 0.83f, 1f);

            foreach (Transform child in echoPortalPrefab.transform) {
                child.transform.localPosition += Vector3.up * 2f;
                child.transform.localScale *= 1.6f;
            }

            Transform tRing = echoPortalPrefab.transform.Find("Ring");
            Material matRing = new Material(tRing.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matRing.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_ColorRamps.texRampHuntressSoft2_png).WaitForCompletion());
            tRing.GetComponent<ParticleSystemRenderer>().sharedMaterial = matRing;

            Transform tRingRing = tRing.transform.Find("Ring");
            Material matRingRing = new Material(tRingRing.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matRingRing.SetInt("_Cull", 1);
            matRingRing.SetColor("_Color", new Color(0.05f, 0f, 0.63f, 1f));
            tRingRing.GetComponent<ParticleSystemRenderer>().sharedMaterial = matRingRing;

            Transform[] rings = {
                tRing,
                tRingRing
            };

            foreach (Transform ring in rings) {
                ParticleSystem particleSystem = ring.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particleSystem.main;
                main.duration = settings.main.duration;
                main.startLifetime = settings.main.startLifetime;
                ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
                sizeOverLifetime.x = settings.sizeOverLifetime.x;
                sizeOverLifetime.y = settings.sizeOverLifetime.y;
                sizeOverLifetime.z = settings.sizeOverLifetime.z;
                ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleSystem.rotationOverLifetime;
                rotationOverLifetime.x = settings.rotationOverLifetime.x;
                rotationOverLifetime.y = settings.rotationOverLifetime.y;
                rotationOverLifetime.z = settings.rotationOverLifetime.z;
            }

            Content.CreateAndAddEffectDef(echoPortalPrefab);
        }

        private static void CreateEchoVortex() {
            echoVortexPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.FalseSonMeridiansWillVortexVFX_prefab).WaitForCompletion(), "EchoVortex", false);

            foreach (ShakeEmitter emitter in echoVortexPrefab.GetComponents<ShakeEmitter>()) {
                Object.Destroy(emitter);
            }
            Object.Destroy(echoVortexPrefab.GetComponent<AkEvent>());
            Object.Destroy(echoVortexPrefab.transform.Find("Distortion/Debris").gameObject);

            Content.CreateAndAddEffectDef(echoVortexPrefab);
        }

        private static void CreateCollapseVacuum() {
            collapseVacuumPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierDeathBombGhost_prefab).WaitForCompletion(), "CollapseVacuum", false);

            EffectComponent effect = collapseVacuumPrefab.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = true;
            effect.positionAtReferencedTransform = true;

            Object.Destroy(collapseVacuumPrefab.transform.Find("Scale/AreaIndicator").gameObject);
            Object.Destroy(collapseVacuumPrefab.transform.Find("Scale/AreaIndicator, Front").gameObject);
            Object.Destroy(collapseVacuumPrefab.transform.Find("Scale/AreaIndicator, Back").gameObject);

            ParticleSystem particleSystem;
            ParticleSystem.MainModule main;
            ParticleSystem.EmissionModule emission;

            collapseVacuumPrefab.transform.Find("Scale/Point light").GetComponent<LightIntensityCurve>().enableNegativeLights = false;

            // Vacuum Stars
            Transform tVacuumStars = collapseVacuumPrefab.transform.Find("Scale/Vacuum Stars");
            particleSystem = tVacuumStars.GetComponent<ParticleSystem>();
            Material matVacuumStars = new Material(tVacuumStars.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matVacuumStars.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_DLC3.texRampFire02_png).WaitForCompletion());
            matVacuumStars.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_VFX_ParticleMasks.texShockwaveMask_psd).WaitForCompletion());
            tVacuumStars.GetComponent<ParticleSystemRenderer>().sharedMaterial = matVacuumStars;
            emission = particleSystem.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 5f),
                new Keyframe(0.16f, 8f),
                new Keyframe(0.32f, 30f)
            ));

            // Vacuum Stars, Trails
            Transform tVacuumStarsTrails = collapseVacuumPrefab.transform.Find("Scale/Vacuum Stars, Trails");
            particleSystem = tVacuumStarsTrails.GetComponent<ParticleSystem>();
            main = particleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 0.94f, 1f, 1f), new Color(1f, 0f, 0.13f, 1f));
            Material matVacuumStarsTrails = new Material(tVacuumStarsTrails.GetComponent<ParticleSystemRenderer>().trailMaterial);
            matVacuumStarsTrails.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_ColorRamps.texRampWhiteAlphaOnly_png).WaitForCompletion());
            tVacuumStarsTrails.GetComponent<ParticleSystemRenderer>().trailMaterial = matVacuumStarsTrails;
            emission = particleSystem.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 2f),
                new Keyframe(0.16f, 8f),
                new Keyframe(0.32f, 30f)
            ));

            // Vacuum Radial
            Transform tVacuumRadial = collapseVacuumPrefab.transform.Find("Scale/Vacuum Radial");
            main = tVacuumRadial.GetComponent<ParticleSystem>().main;
            main.startSize = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1.5f),
                new Keyframe(0.16f, 2f),
                new Keyframe(0.32f, 6f)
            ));
            Material matVacuumRadial = new Material(tVacuumRadial.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matVacuumRadial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_DLC2_Items_SpeedBoostPickup.texSpeedBoostPickupThornRamp_png).WaitForCompletion());
            tVacuumRadial.GetComponent<ParticleSystemRenderer>().sharedMaterial = matVacuumRadial;

            GameObject laserVFX = Object.Instantiate(_assetBundle.LoadAsset<GameObject>("LaserVFX"));
            laserVFX.transform.parent = collapseVacuumPrefab.transform.Find("Scale");
            laserVFX.transform.localScale = Vector3.one;

            Content.CreateAndAddEffectDef(collapseVacuumPrefab);
        }

        private static void CreateCollapseExplosion() {
            collapseExplosionPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_Items_MeteorAttackOnHighDamage.RunicMeteorStrikeImpact_prefab).WaitForCompletion(), "CollapseExplosion", false);
            collapseExplosionPrefab.AddComponent<DestroyOnTimer>().duration = 3f;
            collapseExplosionPrefab.transform.localScale = new Vector3(6f, 6f, 6f);

            //EffectComponent effect = collapseVacuumPrefab.AddComponent<EffectComponent>();
            //effect.applyScale = false;
            //effect.effectIndex = EffectIndex.Invalid;
            //effect.parentToReferencedTransform = true;
            //effect.positionAtReferencedTransform = true;

            Object.Destroy(collapseExplosionPrefab.GetComponent<DestroyOnParticleEnd>());
            Object.Destroy(collapseExplosionPrefab.transform.Find("FallingProjectile").gameObject);

            ParticleSystem particleSystem;
            ParticleSystem.MainModule main;
            ParticleSystem.ShapeModule shape;

            foreach (Transform child in collapseExplosionPrefab.transform) {
                child.localPosition = Vector3.zero;
                particleSystem = child.GetComponent<ParticleSystem>();
                if (particleSystem != null) {
                    main = particleSystem.main;
                    main.startDelay = 0f;
                }
            }

            shape = collapseExplosionPrefab.transform.Find("Dust, Directional").GetComponent<ParticleSystem>().shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;

            GameObject volatileBatteryExplosion = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_QuestVolatileBattery.VolatileBatteryExplosion_prefab).WaitForCompletion());
            Object.Destroy(volatileBatteryExplosion.transform.Find("InitialBurst/Chunks, Sharp").gameObject);
            Object.Destroy(volatileBatteryExplosion.transform.Find("InitialBurst/Chunks, Solid").gameObject);
            Object.Destroy(volatileBatteryExplosion.transform.Find("InitialBurst/Chunks, Billboards").gameObject);
            Transform initialBurst = volatileBatteryExplosion.transform.Find("InitialBurst");
            initialBurst.parent = collapseExplosionPrefab.transform;
            initialBurst.localPosition = Vector3.zero;
            initialBurst.localScale = Vector3.one * 0.5f;
            Object.Destroy(volatileBatteryExplosion);
            //Object.Destroy(collapseVacuumPrefab.transform.Find("Scale/AreaIndicator, Front").gameObject);
            //Object.Destroy(collapseVacuumPrefab.transform.Find("Scale/AreaIndicator, Back").gameObject);

            //ParticleSystem particleSystem;
            //ParticleSystem.MainModule main;
            //ParticleSystem.EmissionModule emission;

            //collapseVacuumPrefab.transform.Find("Scale/Point light").GetComponent<LightIntensityCurve>().enableNegativeLights = false;

            //// Vacuum Stars
            //Transform tVacuumStars = collapseVacuumPrefab.transform.Find("Scale/Vacuum Stars");
            //particleSystem = tVacuumStars.GetComponent<ParticleSystem>();
            //Material matVacuumStars = new Material(tVacuumStars.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            //matVacuumStars.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_DLC3.texRampFire02_png).WaitForCompletion());
            //matVacuumStars.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_VFX_ParticleMasks.texShockwaveMask_psd).WaitForCompletion());
            //tVacuumStars.GetComponent<ParticleSystemRenderer>().sharedMaterial = matVacuumStars;
            //emission = particleSystem.emission;
            //emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            //    new Keyframe(0f, 5f),
            //    new Keyframe(0.16f, 8f),
            //    new Keyframe(0.32f, 30f)
            //));

            //// Vacuum Stars, Trails
            //Transform tVacuumStarsTrails = collapseVacuumPrefab.transform.Find("Scale/Vacuum Stars, Trails");
            //particleSystem = tVacuumStarsTrails.GetComponent<ParticleSystem>();
            //main = particleSystem.main;
            //main.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 0.94f, 1f, 1f), new Color(1f, 0f, 0.13f, 1f));
            //Material matVacuumStarsTrails = new Material(tVacuumStarsTrails.GetComponent<ParticleSystemRenderer>().trailMaterial);
            //matVacuumStarsTrails.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_ColorRamps.texRampWhiteAlphaOnly_png).WaitForCompletion());
            //tVacuumStarsTrails.GetComponent<ParticleSystemRenderer>().trailMaterial = matVacuumStarsTrails;
            //emission = particleSystem.emission;
            //emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            //    new Keyframe(0f, 2f),
            //    new Keyframe(0.16f, 8f),
            //    new Keyframe(0.32f, 30f)
            //));

            //// Vacuum Radial
            //Transform tVacuumRadial = collapseVacuumPrefab.transform.Find("Scale/Vacuum Radial");
            //main = tVacuumRadial.GetComponent<ParticleSystem>().main;
            //main.startSize = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            //    new Keyframe(0f, 1.5f),
            //    new Keyframe(0.16f, 2f),
            //    new Keyframe(0.32f, 6f)
            //));
            //Material matVacuumRadial = new Material(tVacuumRadial.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            //matVacuumRadial.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>(RoR2_DLC2_Items_SpeedBoostPickup.texSpeedBoostPickupThornRamp_png).WaitForCompletion());
            //tVacuumRadial.GetComponent<ParticleSystemRenderer>().sharedMaterial = matVacuumRadial;

            //GameObject laserVFX = Object.Instantiate(_assetBundle.LoadAsset<GameObject>("LaserVFX"));
            //laserVFX.transform.parent = collapseVacuumPrefab.transform.Find("Scale");
            //laserVFX.transform.localScale = Vector3.one;

            Content.CreateAndAddEffectDef(collapseExplosionPrefab);
        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles() {
            CreateThrowProjectile();

            CreateEventHorizonProjectile();

            CreateCollapseProjectile();
        }

        private static void CreateThrowProjectile() {
            throwProjectilePrefab = Asset.LoadAndAddProjectilePrefab(_assetBundle, "ThrowProjectile");
            throwProjectileExplosionEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_Junk_Common_VFX.ImpactLightning_prefab).WaitForCompletion(), "ImpactLightningScaled", false);
            foreach (Transform child in throwProjectileExplosionEffect.transform) {
                child.localScale = Vector3.one * 2.5f;
            }
            throwProjectileExplosionEffect.GetComponent<EffectComponent>().soundName = "Play_mage_m1_impact";
            throwProjectilePrefab.GetComponent<ProjectileImpactExplosion>().explosionEffect = throwProjectileExplosionEffect;

            Content.CreateAndAddEffectDef(throwProjectileExplosionEffect);
        }

        private static void CreateEventHorizonProjectile() {
            horizonProjectilePrefab = Asset.LoadAndAddProjectilePrefab(_assetBundle, "HorizonProjectile");
            ProjectileController horizonProjectileController = horizonProjectilePrefab.GetComponent<ProjectileController>();
            horizonProjectileController.flightSoundLoop = Addressables.LoadAssetAsync<LoopSoundDef>(RoR2_DLC1_VoidSurvivor.lsdVoidMegaBlasterFlight_asset).WaitForCompletion();
            horizonProjectileController.ghostPrefab = CreateEventHorizonGhost();
            horizonProjectilePrefab.GetComponent<ProjectileImpactExplosion>().explosionEffect = CreateEventHorizonExplosion();
        }

        private static void CreateCollapseProjectile() {
            collapseProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierDeathBombProjectile_prefab).WaitForCompletion(), "TimeCollapseProjectile");

            ProjectileController projectileController = collapseProjectile.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = collapseVacuumPrefab;
            projectileController.flightSoundLoop = null;
            projectileController.startSound = "Play_lemurianBruiser_m1_charge";

            ProjectileImpactExplosion projectileImpactExplosion = collapseProjectile.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.lifetime = 1.6f;
            projectileImpactExplosion.impactEffect = collapseExplosionPrefab;

            Content.AddProjectilePrefab(collapseProjectile);
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
            Object.Destroy(tSwirl.gameObject);

            /*
            main = tSwirl.GetComponent<ParticleSystem>().main;
            main.loop = true;
            main.startLifetime = 4f;
            Material matSwirl = new Material(tSwirl.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            matSwirl.SetColor("_TintColor", new Color(0.56f, 0.83f, 1f, 1f));
            matSwirl.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 1f));
            tSwirl.GetComponent<ParticleSystemRenderer>().sharedMaterial = matSwirl;
            */

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
            horizonProjectileExplosion.GetComponent<EffectComponent>().soundName = "Play_voidman_m2_explode";

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
