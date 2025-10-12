using System.Collections;
using EntityStates;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class SplitSecondThrow : GenericProjectileBaseState, SteppedSkillDef.IStepSetter {
        public int swingIndex;

        public override void OnEnter() {
            projectilePrefab = ChronoAssets.throwProjectilePrefab;
            //base.effectPrefab = Modules.Assets.SomeMuzzleEffect;
            targetMuzzle = "HandL";

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
            // Logic from slashcombo which is from false son.... hm
            Animator animator = GetModelAnimator();
            bool isStill = !animator.GetBool("isMoving") && animator.GetBool("isGrounded");
            string animationStateName = ((swingIndex == 0) ? "SwingClubRight" : "SwingClubLeft");
            float animDuration = Mathf.Max(duration, 0.2f);

            characterBody.StartCoroutine(HideSpearMesh(animDuration * 0.6f));

            if (swingIndex == 0) {
                animDuration *= 0.5f;
            }

            if (isStill) {
                PlayCrossfade("FullBody, Override", animationStateName, "SwingClub.playbackRate", animDuration, 0.025f);
                return;
            }
            PlayCrossfade("Gesture, Additive", animationStateName, "SwingClub.playbackRate", animDuration, 0.1f);
            PlayCrossfade("Gesture, Override", animationStateName, "SwingClub.playbackRate", animDuration, 0.1f);
        }

        protected IEnumerator HideSpearMesh(float duration) {
            GameObject spear = GetModelChildLocator()?.FindChild("SpearMesh")?.gameObject;
            if (spear != null && spear.activeSelf) {
                spear.SetActive(false);
                yield return new WaitForSeconds(duration);
                spear.SetActive(true);
            }
        }

        public override void OnSerialize(NetworkWriter writer) {
            base.OnSerialize(writer);
            writer.Write(swingIndex);
        }

        public override void OnDeserialize(NetworkReader reader) {
            base.OnDeserialize(reader);
            swingIndex = reader.ReadInt32();
        }

        public void SetStep(int i) {
            swingIndex = i;
        }
    }
}