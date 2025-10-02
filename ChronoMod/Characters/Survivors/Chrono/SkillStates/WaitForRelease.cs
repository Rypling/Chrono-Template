using System;
using EntityStates;
using RoR2;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class WaitForRelease : BaseState {

        public SkillSlot skillSlot;

        // yoinked
        public override void Update() {
            bool down = skillSlot switch {
                SkillSlot.None => false,
                SkillSlot.Primary => inputBank.skill1.down,
                SkillSlot.Secondary => inputBank.skill2.down,
                SkillSlot.Utility => inputBank.skill3.down,
                SkillSlot.Special => inputBank.skill4.down,
                _ => throw new ArgumentOutOfRangeException(),
            };
            if (!down) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Stun;
        }
    }
}