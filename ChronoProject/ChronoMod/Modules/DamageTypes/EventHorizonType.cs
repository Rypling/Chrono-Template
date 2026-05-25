using ChronoMod.Survivors.Chrono;
using R2API;
using RoR2;
using UnityEngine;

namespace ChronoMod.Modules.DamageTypes {
    public static class EventHorizonType {

        public static DamageAPI.ModdedDamageType damageType;

        public static void Init() {
            damageType = DamageAPI.ReserveDamageType();
            DamageTypeCollection.damageTypes.Add(damageType);
            Hooks.Handle_HealthComponentTakeDamageProcess_Actions += AddSlowBuffs;
        }

        private static void AddSlowBuffs(HealthComponent self, DamageInfo damageInfo) {
            if (damageInfo.HasModdedDamageType(damageType)) {
                CharacterBody attackerBody = damageInfo.attacker?.GetComponent<CharacterBody>();
                if (damageInfo.crit) {
                    self.body?.GetComponent<SetStateOnHurt>()?.SetFrozen(2f);
                }

                float duration = ChronoStaticValues.horizonMinSlowDuration;
                if (attackerBody != null) {
                    float buffFrac = attackerBody.GetBuffCount(ChronoBuffs.temporalRiftBuff) / ChronoStaticValues.temporalMaxBuffs;
                    duration = Mathf.Lerp(ChronoStaticValues.horizonMinSlowDuration, ChronoStaticValues.horizonMaxSlowDuration, buffFrac);
                }

                self.body?.AddTimedBuff(RoR2Content.Buffs.Slow60, duration);
            }
        }
    }
}
