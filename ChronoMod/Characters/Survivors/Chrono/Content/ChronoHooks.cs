using ChronoMod.Modules;
using ChronoMod.Survivors.Chrono.Components;

namespace ChronoMod.Characters.Survivors.Chrono.Content {
    public static class ChronoHooks {
        // place for generic hooks for chrono

        public static void Init() {
            Hooks.Handle_CharacterBodyRecalculateStats_Actions += OverCrit;
        }

        private static void OverCrit(RoR2.CharacterBody self) {
            ChronoController controller = self?.GetComponent<ChronoController>();
            if (controller != null) {
                float overCrit = self.crit - 100f;
                if (overCrit > 0f) {
                    self.critMultiplier += overCrit / 100f;
                    self.crit = 100f;
                }
            }
        }
    }
}
