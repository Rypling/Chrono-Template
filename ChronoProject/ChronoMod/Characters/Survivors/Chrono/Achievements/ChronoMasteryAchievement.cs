using ChronoMod.Modules.Achievements;
using RoR2;

namespace ChronoMod.Survivors.Chrono.Achievements {
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class ChronoMasteryAchievement : BaseMasteryAchievement {
        public const string identifier = ChronoSurvivor.CHRONO_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = ChronoSurvivor.CHRONO_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => ChronoSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}