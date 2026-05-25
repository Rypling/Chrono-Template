using ChronoMod.Characters.Survivors.Chrono.SkillStates;
using ChronoMod.Survivors.Chrono.Components;
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

        public ChronoController chronoController;

        public override void OnEnter() {
            base.OnEnter();
            if (isAuthority) {
                chronoController = characterBody.GetComponent<ChronoController>();
                chronoController?.MoveUIToCrosshair();
            }
        }

        protected override EntityState PickNextState() {
            TimeCollapseFire nextState = new TimeCollapseFire();
            nextState.chronoController = chronoController;
            nextState.attackOrigin = currentTrajectoryInfo.hitPoint;

            return nextState;
        }

        public override void OnExit() {
            base.OnExit();
            if (isAuthority) {
                chronoController?.MoveUIToSpecial();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Stun;
        }
    }
}