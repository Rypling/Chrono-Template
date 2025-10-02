using ChronoMod.Survivors.Chrono;
using R2API;
using RoR2;

namespace ChronoMod.Modules.DamageTypes {
    public static class TimePiercerType {

        public static DamageAPI.ModdedDamageType damageType;

        public static void Init() {
            damageType = DamageAPI.ReserveDamageType();
            DamageTypeCollection.damageTypes.Add(damageType);
            Hooks.Handle_HealthComponentTakeDamageProcess_Actions += AddSlowBuffs;
        }

        private static void AddSlowBuffs(HealthComponent self, DamageInfo damageInfo) {
            if (damageInfo.HasModdedDamageType(damageType)) {
                CharacterBody attackerBody = damageInfo.attacker?.GetComponent<CharacterBody>();
                if (attackerBody != null && attackerBody.GetBuffCount(ChronoBuffs.temporalRiftBuff) >= ChronoStaticValues.temporalMaxBuffs * ChronoStaticValues.piercerFreezeFrac) {
                    self.body?.GetComponent<SetStateOnHurt>()?.SetFrozen(2f);
                }
                self.body?.AddTimedBuff(RoR2Content.Buffs.Slow60, 4f);
            }
        }
    }
}
