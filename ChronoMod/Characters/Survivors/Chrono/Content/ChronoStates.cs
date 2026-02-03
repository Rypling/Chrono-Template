using ChronoMod.Survivors.Chrono.SkillStates;

namespace ChronoMod.Survivors.Chrono {
    public static class ChronoStates {
        public static void Init() {
            Modules.Content.AddEntityState(typeof(EonsEdge));

            Modules.Content.AddEntityState(typeof(SplitSecondThrow));

            Modules.Content.AddEntityState(typeof(TimePiercer));

            Modules.Content.AddEntityState(typeof(EventHorizon));

            Modules.Content.AddEntityState(typeof(EchoOfTomorrowAim));

            Modules.Content.AddEntityState(typeof(ContinuumFreeze));

            Modules.Content.AddEntityState(typeof(TimeCollapse));

            Modules.Content.AddEntityState(typeof(WaitForRelease));
        }
    }
}
