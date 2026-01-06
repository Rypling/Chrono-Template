using System.Security;
using System.Security.Permissions;
using BepInEx;
using ChronoMod.Modules;
using ChronoMod.Survivors.Chrono;
using R2API.Utils;
using RoR2;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace ChronoMod {
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency("com.groovesalad.TestPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class ChronoPlugin : BaseUnityPlugin {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.Rypling.ChronoMod";
        public const string MODNAME = "ChronoMod";
        public const string MODVERSION = "1.0.0";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "RYPLING";

        public static ChronoPlugin instance;

        void Awake() {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new ChronoSurvivor().Initialize();

            RoR2Application.onLoadFinished += OnLoadFinished;

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }

        private void OnLoadFinished() {
            DamageTypeCollection.Init();
            Hooks.AddHooks();
            ChronoAssets.AssignDamageTypes();
        }
    }
}
