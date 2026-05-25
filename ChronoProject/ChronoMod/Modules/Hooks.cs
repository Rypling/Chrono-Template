using ChronoMod.Characters.Survivors.Chrono.Content;

namespace ChronoMod.Modules {
    public static class Hooks {

        public delegate void Handle_CharacterBodyRecalculateStats(RoR2.CharacterBody self);
        public static Handle_CharacterBodyRecalculateStats Handle_CharacterBodyRecalculateStats_Actions;


        public delegate void Handle_HealthComponentTakeDamageProcess(RoR2.HealthComponent self, RoR2.DamageInfo damageInfo);
        public static Handle_HealthComponentTakeDamageProcess Handle_HealthComponentTakeDamageProcess_Actions;


        public static void AddHooks() {
            ChronoHooks.Init();

            if (Handle_CharacterBodyRecalculateStats_Actions != null) {
                On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            }

            if (Handle_HealthComponentTakeDamageProcess_Actions != null) {
                On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            }
        }

        internal static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self) {
            orig(self);
            Handle_CharacterBodyRecalculateStats_Actions.Invoke(self);

        }

        internal static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo) {
            orig(self, damageInfo);
            Handle_HealthComponentTakeDamageProcess_Actions.Invoke(self, damageInfo);
        }
    }
}
