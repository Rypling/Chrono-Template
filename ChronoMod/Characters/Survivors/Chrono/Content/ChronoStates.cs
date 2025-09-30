using ChronoMod.Survivors.Chrono.SkillStates;

namespace ChronoMod.Survivors.Chrono
{
    public static class ChronoStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(Shoot));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));
        }
    }
}
