using R2API;

namespace ChronoMod.Modules.DamageTypes {
    public static class TimeCollapseType {

        public static DamageAPI.ModdedDamageType damageType;

        public static void Init() {
            damageType = DamageAPI.ReserveDamageType();
            DamageTypeCollection.damageTypes.Add(damageType);
        }
    }
}
