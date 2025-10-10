using ChronoMod.Modules.DamageTypes;
using EntityStates;
using R2API;
using RoR2;
using RoR2BepInExPack.GameAssetPathsBetter;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class TimePiercer : BaseSkillState {
        public static float damageCoefficient = ChronoStaticValues.piercerDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.1f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 800f;
        public static float recoil = 1f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Commando.TracerCommandoShotgun_prefab).WaitForCompletion();
        public static GameObject muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Commando.MuzzleflashFMJ_prefab).WaitForCompletion();
        public static GameObject hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Commando.HitsparkCommandoShotgun_prefab).WaitForCompletion();

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        public override void OnEnter() {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            PlayCrossfade("Gesture, Additive", "FireLunarSpike", "LunarSpike.playbackRate", duration, 0.1f);
            PlayCrossfade("Gesture, Override", "FireLunarSpike", "LunarSpike.playbackRate", duration, 0.1f);
        }

        public override void OnExit() {
            base.OnExit();
        }

        public override void FixedUpdate() {
            base.FixedUpdate();

            if (fixedAge >= fireTime) {
                Fire();
            }

            if (fixedAge >= duration && isAuthority) {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire() {
            if (!hasFired) {
                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleString, false);
                Util.PlaySound("HenryShootPistol", gameObject);

                if (isAuthority) {
                    Ray aimRay = GetAimRay();
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                    BulletAttack bullet = new BulletAttack {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageTypeCombo.GenericSecondary,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = range,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = RollCrit(),
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = procCoefficient,
                        radius = 0.75f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = hitEffectPrefab,
                    };
                    bullet.AddModdedDamageType(TimePiercerType.damageType);
                    bullet.Fire();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.PrioritySkill;
        }
    }
}