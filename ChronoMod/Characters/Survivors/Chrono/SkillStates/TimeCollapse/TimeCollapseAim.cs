using ChronoMod.Characters.Survivors.Chrono.SkillStates;
using EntityStates;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class TimeCollapseAim : AimBase {

        public override float maxDistance => 100f;

        public override float rayRadius => 0.7f;

        public override float endpointVisualizerRadiusScale => 4f;

        public override bool toggleActivate => true;

        public override bool hideCrosshair => false;

        public override LayerMask layerMask => LayerIndex.CommonMasks.bullet;

        protected override EntityState PickNextState() {
            TimeCollapseFire nextState = new TimeCollapseFire();
            nextState.attackOrigin = currentTrajectoryInfo.hitPoint;

            return nextState;
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Stun;
        }
    }
}