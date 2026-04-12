using ChronoMod.Characters.Survivors.Chrono.SkillStates;
using EntityStates;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class EchoOfTomorrowAim : AimBase {

        private Vector3 safeBackDirection;

        private CameraTargetParams.AimRequest aimRequest;
        public override void OnEnter() {
            aimRequest = base.cameraTargetParams.RequestAimWithData(Vector3.zero, 0.1f, 0.1f);
            base.OnEnter();
        }

        public override void FixedUpdate() {
            base.FixedUpdate();
            if (isAuthority && !IsKeyDownAuthority() && fixedAge >= minimumDuration) {
                aimRequest?.Dispose();
            }
        }

        protected override EntityState PickNextState() {
            return new EchoOfTomorrowTeleport {
                aimLocation = currentTrajectoryInfo.hitPoint,
                safeTeleportBackwordDirection = safeBackDirection,
                distanceToCheck = maxDistance,
                aimRequest = aimRequest
            };
        }

        protected override void UpdateTrajectoryInfo(out AimThrowableBase.TrajectoryInfo dest, out Ray aimRay) {
            base.UpdateTrajectoryInfo(out dest, out aimRay);

            if (hasCollided) {
                safeBackDirection = -aimRay.direction;
            } else {
                safeBackDirection = Vector3.up;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Stun;
        }
    }
}