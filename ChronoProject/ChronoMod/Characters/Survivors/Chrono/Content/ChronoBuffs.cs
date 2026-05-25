using ChronoMod.Modules;
using ChronoMod.Survivors.Chrono.Components;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Survivors.Chrono {
    public static class ChronoBuffs {

        public static BuffDef temporalRiftBuff;

        public static BuffDef timeWarpBuff;

        public static BuffDef continuumFreezeBuff;

        public static void Init(AssetBundle assetBundle) {
            temporalRiftBuff = Content.CreateAndAddBuff(
                "ChronoTemporalRiftBuff",
                Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Nullifier.texBuffNullifyStackIcon_tif).WaitForCompletion(),
                new Color(0f, 0.62f, 0.85f),
                true,
                false,
                BuffDef.StackingDisplayMethod.Percentage
                );

            timeWarpBuff = Content.CreateAndAddBuff(
                "ChronoTimeWarpBuff",
                Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Nullifier.texBuffNullifiedIcon_tif).WaitForCompletion(),
                new Color(0f, 0.73f, 1f),
                false,
                false
                );

            continuumFreezeBuff = Content.CreateAndAddBuff(
                "ChronoContinuumFreezeBuff",
                Addressables.LoadAssetAsync<Sprite>(RoR2_Base_RoboBallBoss.texBuffEngiShieldIcon_tif).WaitForCompletion(),
                new Color(0f, 0.73f, 1f),
                false,
                false
                );

            SetupBehaviours();
        }

        private static void SetupBehaviours() {
            R2API.RecalculateStatsAPI.GetStatCoefficients += TemporalRiftStatIncrease;
            R2API.RecalculateStatsAPI.GetStatCoefficients += TimeWarpStatIncrease;
            R2API.RecalculateStatsAPI.GetStatCoefficients += ContinuumFreezeStatIncrease;
            Hooks.Handle_HealthComponentTakeDamageProcess_Actions += ContinuumFreezeLifesteal;
        }

        private static void TemporalRiftStatIncrease(CharacterBody self, R2API.RecalculateStatsAPI.StatHookEventArgs args) {
            ChronoController controller = self.GetComponent<ChronoController>();
            if (self.HasBuff(temporalRiftBuff) && controller != null) {
                float buffFrac = self.GetBuffCount(temporalRiftBuff) / ChronoStaticValues.temporalMaxBuffs;
                args.moveSpeedMultAdd += ChronoStaticValues.temporalMaxSpdMultAdd * buffFrac;
                args.critAdd += ChronoStaticValues.temporalMaxCritAdd * buffFrac;
                args.critDamageMultAdd += ChronoStaticValues.temporalMaxCritDmgMultAdd * buffFrac;
            }
        }

        private static void TimeWarpStatIncrease(CharacterBody self, R2API.RecalculateStatsAPI.StatHookEventArgs args) {
            if (self.HasBuff(timeWarpBuff)) {
                args.moveSpeedMultAdd += 0.20f;
                args.damageMultAdd += 0.15f;
            }
        }

        private static void ContinuumFreezeStatIncrease(CharacterBody self, R2API.RecalculateStatsAPI.StatHookEventArgs args) {
            if (self.HasBuff(continuumFreezeBuff)) {
                args.critAdd += 20f;
                args.critDamageMultAdd += 0.25f;
            }
        }

        private static void ContinuumFreezeLifesteal(HealthComponent self, DamageInfo damageInfo) {
            if (damageInfo.attacker) {
                HealthComponent attackerHealth = damageInfo?.attacker?.GetComponent<HealthComponent>();
                CharacterBody body = attackerHealth?.body;
                float damage = damageInfo?.damage ?? 0f;
                if (body != null && body.HasBuff(continuumFreezeBuff) && damage > 0f) {
                    attackerHealth?.Heal(damage * 0.1f, default(ProcChainMask));
                }
            }
        }
    }
}
