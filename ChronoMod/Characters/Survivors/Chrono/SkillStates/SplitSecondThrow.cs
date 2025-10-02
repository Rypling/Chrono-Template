using EntityStates;
using RoR2.Projectile;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class SplitSecondThrow : GenericProjectileBaseState {

        public override void OnEnter() {
            projectilePrefab = ChronoAssets.throwProjectilePrefab;
            //base.effectPrefab = Modules.Assets.SomeMuzzleEffect;
            // targetMuzzle = "muzzleThrow";

            attackSoundString = "HenryBombThrow";

            baseDuration = 0.8f;
            baseDelayBeforeFiringProjectile = 0.0f;

            damageCoefficient = ChronoStaticValues.throwDamageCoefficient;
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
            return InterruptPriority.PrioritySkill;
        }

        public override void PlayAnimation(float duration) {

            if (GetModelAnimator()) {
                PlayAnimation("Gesture, Override", "ThrowBomb", "ThrowBomb.playbackRate", this.duration);
            }
        }
    }
}