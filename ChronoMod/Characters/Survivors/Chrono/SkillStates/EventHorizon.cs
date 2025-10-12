using EntityStates;
using RoR2.Projectile;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class EventHorizon : GenericProjectileBaseState {

        public override void OnEnter() {
            projectilePrefab = ChronoAssets.horizonProjectilePrefab;
            //base.effectPrefab = Modules.Assets.SomeMuzzleEffect;
            //targetmuzzle = "muzzleThrow"

            attackSoundString = "HenryBombThrow";

            baseDuration = 0.6f;
            baseDelayBeforeFiringProjectile = 0.0f;

            damageCoefficient = ChronoStaticValues.horizonDamageCoefficient;
            //proc coefficient is set on the components of the projectile prefab
            force = 80f;

            //base.projectilePitchBonus = 0;
            //base.minSpread = 0;
            //base.maxSpread = 0;

            recoilAmplitude = 0.1f;
            bloom = 10;

            base.OnEnter();
        }

        public override void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo) {
            base.ModifyProjectileInfo(ref fireProjectileInfo);
        }

        public override void FixedUpdate() {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Skill;
        }

        public override void PlayAnimation(float duration) {
            PlayCrossfade("Gesture, Additive", "FireLunarSpike", "LunarSpike.playbackRate", duration, 0.1f);
            PlayCrossfade("Gesture, Override", "FireLunarSpike", "LunarSpike.playbackRate", duration, 0.1f);
        }
    }
}