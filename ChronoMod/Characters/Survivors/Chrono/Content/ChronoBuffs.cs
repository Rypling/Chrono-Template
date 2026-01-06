using System;
using ChronoMod.Modules;
using ChronoMod.Survivors.Chrono.Components;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono {
    public static class ChronoBuffs {

        public static BuffDef temporalRiftBuff;

        public static BuffDef timeWarpBuff;

        public static BuffDef continuumFreezeBuff;

        public static void Init(AssetBundle assetBundle) {
            temporalRiftBuff = Modules.Content.CreateAndAddBuff(
                "ChronoTemporalRiftBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/AffixLunar").iconSprite,
                Color.cyan,
                true,
                false
                );

            timeWarpBuff = Modules.Content.CreateAndAddBuff(
                "ChronoTimeWarpBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/MercExpose").iconSprite,
                Color.cyan,
                false,
                false
                );

            continuumFreezeBuff = Modules.Content.CreateAndAddBuff(
                "ChronoContinuumFreezeBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/Warbanner").iconSprite,
                Color.cyan,
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
            try {
                if (damageInfo.attacker) {
                    HealthComponent attackerHealth = damageInfo?.attacker?.GetComponent<HealthComponent>();
                    CharacterBody body = attackerHealth?.body;
                    float damage = damageInfo?.damage ?? 0f;
                    if (body != null && body.HasBuff(continuumFreezeBuff) && damage > 0f) {
                        attackerHealth?.Heal(damage * 0.1f, default(ProcChainMask));
                    }
                }
            } catch (NullReferenceException e) {
                Log.Warning("ContinuumFreezeLifesteal NRE");
            }
        }
    }
}
