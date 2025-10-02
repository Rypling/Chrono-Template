using System.Collections.Generic;
using ChronoMod.Modules.DamageTypes;
using R2API;

namespace ChronoMod.Modules {
    public static class DamageTypeCollection {
        internal static List<DamageAPI.ModdedDamageType> damageTypes = new List<DamageAPI.ModdedDamageType>();

        public static void Init() {
            TemporalRiftType.Init();
            TimePiercerType.Init();
            EventHorizonType.Init();
            TimeCollapseType.Init();
        }
    }
}
