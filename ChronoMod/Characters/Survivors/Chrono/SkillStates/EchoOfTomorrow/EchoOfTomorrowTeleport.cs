using EntityStates;
using EntityStates.FalseSon;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class EchoOfTomorrowTeleport : MeridiansWillTeleport {

        public override void OnEnter() {
            BeginTeleportParamHash = "StepBrothersPrep.playbackRate";
            BeginTeleportStateHash = "StepBrothersPrep";
            safeTeleportBackwordDist = 3f;
            minTeleportCameraLerpDuration = 0.05f;
            maxTeleportCameraLerpDuration = 0.3f;
            initialLightningEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.MeridiansWillInitialLightningImpact_prefab).WaitForCompletion();
            delayedLightningEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.MeridiansWillSecondaryLightningImpact_prefab).WaitForCompletion();
            blastRadius = 15f;
            vortexEffect = Addressables.LoadAssetAsync<GameObject>(RoR2_DLC2_FalseSon.FalseSonMeridiansWillVortexVFX_prefab).WaitForCompletion();
            speedCoefficient = 25f;

            teleportDelayDuration = 0.1f;
            endTeleportDelayDuration = 0.1f;
            base.OnEnter();

            float buffFrac = characterBody.GetBuffCount(ChronoBuffs.temporalRiftBuff) / ChronoStaticValues.temporalMaxBuffs;
            healthComponent.Heal(healthComponent.fullHealth * Mathf.Lerp(ChronoStaticValues.echoMinHealingFrac, ChronoStaticValues.echoMaxHealingFrac, buffFrac), default);
        }

        public override EntityState InstantiateNextState() {
            return new EchoOfTomorrowFire();
        }
    }
}