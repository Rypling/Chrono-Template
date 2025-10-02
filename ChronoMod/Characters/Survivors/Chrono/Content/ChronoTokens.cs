using System;
using ChronoMod.Modules;
using ChronoMod.Survivors.Chrono.Achievements;

namespace ChronoMod.Survivors.Chrono {
    public static class ChronoTokens {
        public static void Init() {
            AddChronoTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Henry.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddChronoTokens() {
            string prefix = ChronoSurvivor.CHRONO_PREFIX;

            string desc = "Henry is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sword is a good all-rounder while Boxing Gloves are better for laying a beatdown on more powerful foes." + Environment.NewLine + Environment.NewLine
             + "< ! > Pistol is a powerful anti air, with its low cooldown and high damage." + Environment.NewLine + Environment.NewLine
             + "< ! > Roll has a lingering armor buff that helps to use it aggressively." + Environment.NewLine + Environment.NewLine
             + "< ! > Bomb can be used to wipe crowds with ease." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so he left, searching for a new identity.";
            string outroFailure = "..and so he vanished, forever a blank slate.";

            Language.Add(prefix + "NAME", "Chrono");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Chosen One");
            Language.Add(prefix + "LORE", "sample lore");
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Temporal Rift");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", "For each stack of <style=cIsDamage>Temporal Rift</style> you have, your <style=cIsUtility>move speed</style>, <style=cIsDamage>critical hit chance</style>, and <style=cIsDamage>critical hit damage</style> increase.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_EDGE_NAME", "Eon's Edge");
            Language.Add(prefix + "PRIMARY_EDGE_DESCRIPTION", $"{Tokens.agilePrefix} Swing in front for for <style=cIsDamage>{100f * ChronoStaticValues.swordDamageCoefficient}% damage</style>. Gain a stack of <style=cIsDamage>Temporal Rift</style> for each enemy hit.");

            Language.Add(prefix + "PRIMARY_THROW_NAME", "Split-Second Throw");
            Language.Add(prefix + "PRIMARY_THROW_DESCRIPTION", $"{Tokens.agilePrefix} Throw a spear for <style=cIsDamage>{100f * ChronoStaticValues.throwDamageCoefficient}% damage</style>. Gain a stack of <style=cIsDamage>Temporal Rift</style> for each enemy hit.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_PIERCER_NAME", "Time Piercer");
            Language.Add(prefix + "SECONDARY_PIERCER_DESCRIPTION", $"{Tokens.agilePrefix} Rapidly fire for <style=cIsDamage>{100f * ChronoStaticValues.piercerDamageCoefficient}% damage</style>, <style=cIsUtility>slowing</style> enemies, additionally <style=cIsDamage>freezing</style> them if you have at least <style=cIsUtility>{(int)(ChronoStaticValues.temporalMaxBuffs * ChronoStaticValues.piercerFreezeFrac)}</style> stacks of <style=cIsDamage>Temporal Rift</style>.");

            Language.Add(prefix + "SECONDARY_HORIZON_NAME", "Event Horizon");
            Language.Add(prefix + "SECONDARY_HORIZON_DESCRIPTION", $"{Tokens.agilePrefix} Throw a piece of time itself for <style=cIsDamage>{100f * ChronoStaticValues.horizonDamageCoefficient}% damage</style>, <style=cIsUtility>slowing</style> enemies for a duration proportional to your <style=cIsDamage>Temporal Rift</style> stacks, and <style=cIsDamage>freezing</style> them on a critical hit.");
            #endregion

            #region Utility
            Language.Add(prefix + "UTILITY_ECHO_NAME", "Echo of Tomorrow");
            Language.Add(prefix + "UTILITY_ECHO_DESCRIPTION", "Dash forward through time to <style=cIsHealing>heal</style> yourself proportional to your <style=cIsDamage>Temporal Rift</style> stacks, and gain <style=cIsDamage>Time Warp</style>.");

            Language.Add(prefix + "UTILITY_CONTINUUM_NAME", "Continuum Freeze");
            Language.Add(prefix + "UTILITY_CONTINUUM_DESCRIPTION", "Create a temporal scar, <style=cIsDamage>freezing</style> all enemies inside. While inside, allies gain bonus <style=cIsDamage>critical hit chance</style>, <style=cIsDamage>critical hit damage</style>, and have <style=cIsHealing>life steal</style>. Duration scales proportional to your <style=cIsDamage>Temporal Rift</style> stacks.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_COLLAPSE_NAME", "Time Collapse");
            Language.Add(prefix + "SPECIAL_COLLAPSE_DESCRIPTION", $"<style=cIsDamage>Critically strike</style> a target for <style=cIsDamage>all the damage you have dealt in the last 10 seconds</style>. Damage scales with your current <style=cIsDamage>Temporal Rift</style> stacks. Consumes all stacks.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(ChronoMasteryAchievement.identifier), "Chrono: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(ChronoMasteryAchievement.identifier), "As Chrono, beat the game or obliterate on Monsoon.");
            #endregion
        }
    }
}
