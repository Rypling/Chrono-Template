using ChronoMod.Modules.BaseStates;
using ChronoMod.Modules.DamageTypes;
using R2API;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class SlashCombo : BaseMeleeAttack {
        public override void OnEnter() {
            hitboxGroupName = "SwingGroup";

            damageType = DamageTypeCombo.GenericPrimary;
            damageType.AddModdedDamageType(TemporalRiftType.damageType);
            damageCoefficient = ChronoStaticValues.swordDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.05f;
            attackEndPercentTime = 0.4f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.6f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 4f;

            swingSoundString = "HenrySwordSwing";
            hitSoundString = "";
            muzzleString = swingIndex % 2 == 0 ? "SwingClubRight" : "SwingClubLeft";
            playbackRateParam = "SwingClub.playbackRate";
            swingEffectPrefab = ChronoAssets.swordSwingEffect;
            hitEffectPrefab = ChronoAssets.swordHitImpactEffect;

            impactSound = ChronoAssets.swordHitSoundEvent.index;

            base.OnEnter();
        }

        protected override void PlayAttackAnimation() {
            // Logic from false son
            bool isStill = !animator.GetBool("isMoving") && animator.GetBool("isGrounded");
            string animationStateName = ((swingIndex == 0) ? "SwingClubRight" : "SwingClubLeft");
            float animDuration = Mathf.Max(duration, 0.2f);
            if (isStill) {
                PlayCrossfade("FullBody, Override", animationStateName, "SwingClub.playbackRate", animDuration, 0.025f);
                return;
            }
            PlayCrossfade("Gesture, Additive", animationStateName, "SwingClub.playbackRate", animDuration, 0.1f);
            PlayCrossfade("Gesture, Override", animationStateName, "SwingClub.playbackRate", animDuration, 0.1f);
        }

        protected override void PlaySwingEffect() {
            base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority() {
            base.OnHitEnemyAuthority();
        }

        public override void OnExit() {
            base.OnExit();
        }
    }
}