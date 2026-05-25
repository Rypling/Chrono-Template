using EntityStates;
using EntityStates.FalseSon;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class EchoOfTomorrowTeleport : MeridiansWillTeleport {

        public override void OnEnter() {
            // base base
            if (characterBody) {
                attackSpeedStat = characterBody.attackSpeed;
                damageStat = characterBody.damage;
                critStat = characterBody.crit;
                moveSpeedStat = characterBody.moveSpeed;
            }

            BeginTeleportParamHash = "StepBrothersPrep.playbackRate";
            BeginTeleportStateHash = "StepBrothersPrep";
            safeTeleportBackwordDist = 3f;
            minTeleportCameraLerpDuration = 0.05f;
            maxTeleportCameraLerpDuration = 0.3f;
            initialLightningEffect = ChronoAssets.echoPortalPrefab;// Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.MeridiansWillInitialLightningImpact_prefab).WaitForCompletion();
            delayedLightningEffect = ChronoAssets.echoPortalPrefab;// Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.MeridiansWillSecondaryLightningImpact_prefab).WaitForCompletion();
            blastRadius = 15f;
            vortexEffect = ChronoAssets.echoVortexPrefab;
            speedCoefficient = 25f;
            teleportDelayDuration = 0.1f;
            endTeleportDelayDuration = 0.25f;

            // base
            PlayAnimation("FullBody, Override", BeginTeleportStateHash, BeginTeleportParamHash, teleportDelayDuration);
            characterMotor.velocity = Vector3.zero;
            Vector3 portalRotation = transform.position - aimLocation;
            portalRotation.y = 0f;
            effectData = new EffectData {
                origin = characterBody.footPosition,
                rotation = Quaternion.LookRotation(portalRotation)
            };
            EffectManager.SpawnEffect(initialLightningEffect, effectData, transmit: false);
            portalRotation *= -1f;
            effectData.rotation = Quaternion.LookRotation(portalRotation);
            teleportVector = inputBank.aimDirection;
            modelTransform = GetModelTransform();
            characterModel = modelTransform.GetComponent<CharacterModel>();
            hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            characterModel.invisibilityCount++;
            HurtBoxGroup hurtBoxGroup = hurtboxGroup;
            int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
            hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            endTeleportDelayDuration += teleportDelayDuration;
            teleportVector = aimLocation - characterBody.gameObject.transform.position;
            teleportVector = teleportVector.normalized;
            float num = Vector3.Distance(aimLocation, characterBody.corePosition);
            float num2 = Mathf.Lerp(minTeleportCameraLerpDuration, maxTeleportCameraLerpDuration, num / distanceToCheck);
            if (num2 < 1f) {
                num2 = Mathf.Sqrt(num2);
            }
            cameraTargetParams.AddLerpRequest(num2);

            float buffFrac = characterBody.GetBuffCount(ChronoBuffs.temporalRiftBuff) / ChronoStaticValues.temporalMaxBuffs;
            healthComponent.Heal(healthComponent.fullHealth * Mathf.Lerp(ChronoStaticValues.echoMinHealingFrac, ChronoStaticValues.echoMaxHealingFrac, buffFrac), default);

            Util.PlaySound("Play_huntress_shift_end", characterBody.gameObject);
        }

        public override EntityState InstantiateNextState() {
            return new EchoOfTomorrowFire();
        }
    }
}