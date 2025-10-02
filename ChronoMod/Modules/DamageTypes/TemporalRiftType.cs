using ChronoMod.Survivors.Chrono;
using ChronoMod.Survivors.Chrono.Components;
using R2API;
using RoR2;
using UnityEngine;

namespace ChronoMod.Modules.DamageTypes {
    public static class TemporalRiftType {

        public static DamageAPI.ModdedDamageType damageType;

        public static void Init() {
            damageType = DamageAPI.ReserveDamageType();
            DamageTypeCollection.damageTypes.Add(damageType);
            Hooks.Handle_HealthComponentTakeDamageProcess_Actions += AddRiftBuff;
        }

        private static void AddRiftBuff(HealthComponent self, DamageInfo damageInfo) {
            if (damageInfo.HasModdedDamageType(damageType)) {
                CharacterBody attackerBody = damageInfo.attacker?.GetComponent<CharacterBody>();
                if (attackerBody != null) {
                    ChronoController controller = attackerBody.GetComponent<ChronoController>();
                    if (controller != null) {
                        int buffCount = attackerBody.GetBuffCount(ChronoBuffs.temporalRiftBuff);
                        if (buffCount < ChronoStaticValues.temporalMaxBuffs) {
                            attackerBody.AddBuff(ChronoBuffs.temporalRiftBuff);
                        }
                        controller.buffStopwatch = Mathf.Max(0f, controller.buffStopwatch - ChronoStaticValues.temporalOnBuffSlowTimer);
                        attackerBody.RecalculateStats();
                    }
                }
            }
        }
    }
}
