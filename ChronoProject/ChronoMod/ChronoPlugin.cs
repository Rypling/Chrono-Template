using BepInEx;
using ChronoMod.Modules;
using ChronoMod.Survivors.Chrono;
using R2API.Utils;
using RoR2;


namespace ChronoMod {
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(R2API.DamageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class ChronoPlugin : BaseUnityPlugin {
        public const string MODUID = "com.Miyowi.ChronoMod";
        public const string MODNAME = "ChronoMod";
        public const string MODVERSION = "1.0.0";

        public const string DEVELOPER_PREFIX = "MIYOWI";

        public static ChronoPlugin instance;

        void Awake() {
            instance = this;

            Log.Init(Logger);

            Modules.Language.Init();

            new ChronoSurvivor().Initialize();

            RoR2Application.onLoadFinished += OnLoadFinished;

            new Modules.ContentPacks().Initialize();
        }

        private void OnLoadFinished() {
            DamageTypeCollection.Init();
            Hooks.AddHooks();
            ChronoAssets.AssignDamageTypes();
        }
    }
}
