using EntityStates;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Characters.Survivors.Chrono.SkillStates {

    /// <summary>
    /// Heavily modified version of decompiled code from EntityStates.AimThrowableBase.
    /// Removed projectile-specific logic, has an option to be a toggle input (activated with primary) with the toggleActivate field.
    /// Not for use with primary skills
    /// </summary>
    public class AimBase : BaseSkillState {

        // Serlialized fields
        public virtual float maxDistance => 20f;

        public virtual float rayRadius => 5f;

        public virtual GameObject arcVisualizerPrefab => null;

        public virtual GameObject endpointVisualizerPrefab => Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Huntress.HuntressArrowRainIndicator_prefab).WaitForCompletion();

        public virtual float endpointVisualizerRadiusScale => 8f;

        public virtual float baseMinimumDuration => 0.15f;

        public virtual string originOverrideString => "";

        public virtual bool toggleActivate => false;

        public virtual bool hideCrosshair => true;

        public virtual LayerMask layerMask => LayerIndex.world.mask;

        // No touchy

        private bool holdingActivationKey = true;

        private bool holdingCancelKey = false;

        private float heldRechargeStopwatch;

        private bool stateFinished = false;

        private bool IsNewKeyDownAuthority => IsKeyDownAuthority() && !holdingActivationKey;

        protected GameObject _endpointVisualizerPrefab; // overcooked

        protected LineRenderer arcVisualizerLineRenderer;

        protected Transform endpointVisualizerTransform;

        protected Transform originOverride;

        protected float projectileBaseSpeed;

        protected float minimumDuration;

        protected bool useGravity;

        protected float addedGravity;

        protected bool hasCollided;

        private AimThrowableBase.CalculateArcPointsJob calculateArcPointsJob;

        private JobHandle calculateArcPointsJobHandle;

        private Vector3[] pointsBuffer = Array.Empty<Vector3>();

        private Action completeArcPointsVisualizerJobMethod;

        protected AimThrowableBase.TrajectoryInfo currentTrajectoryInfo;

        protected float totalGravity => (useGravity ? Physics.gravity.y : 0f) + Physics.gravity.y * addedGravity;

        public override void OnEnter() {
            base.OnEnter();

            if (isAuthority) {
                heldRechargeStopwatch = skillLocator.special.rechargeStopwatch;

                _endpointVisualizerPrefab = endpointVisualizerPrefab;

                if (arcVisualizerPrefab) {
                    arcVisualizerLineRenderer = UnityEngine.Object.Instantiate(arcVisualizerPrefab, transform.position, Quaternion.identity).GetComponent<LineRenderer>();
                    calculateArcPointsJob = default;
                    completeArcPointsVisualizerJobMethod = CompleteArcVisualizerJob;
                    RoR2Application.onLateUpdate += completeArcPointsVisualizerJobMethod;
                }

                if (_endpointVisualizerPrefab) {
                    endpointVisualizerTransform = UnityEngine.Object.Instantiate(_endpointVisualizerPrefab, transform.position, Quaternion.identity).transform;
                }

                if (characterBody) {
                    characterBody.hideCrosshair = hideCrosshair;
                }

                originOverride = FindModelChild(originOverrideString);
                minimumDuration = baseMinimumDuration / attackSpeedStat;
                UpdateVisualizers(currentTrajectoryInfo);
                SceneCamera.onSceneCameraPreRender += OnPreRenderSceneCam;
            }
        }

        public override void OnExit() {
            if (isAuthority) {
                SceneCamera.onSceneCameraPreRender -= OnPreRenderSceneCam;

                if (characterBody) {
                    characterBody.hideCrosshair = false;
                }

                calculateArcPointsJobHandle.Complete();
                if (arcVisualizerLineRenderer) {
                    Destroy(arcVisualizerLineRenderer.gameObject);
                    arcVisualizerLineRenderer = null;
                }

                if (completeArcPointsVisualizerJobMethod != null) {
                    RoR2Application.onLateUpdate -= completeArcPointsVisualizerJobMethod;
                    completeArcPointsVisualizerJobMethod = null;
                }

                calculateArcPointsJob.Dispose();

                pointsBuffer = Array.Empty<Vector3>();

                if (endpointVisualizerTransform) {
                    Destroy(endpointVisualizerTransform.gameObject);
                    endpointVisualizerTransform = null;
                }
            }

            base.OnExit();
        }

        protected virtual EntityState PickNextState() {
            return null;
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Skill;
        }

        public override void Update() {
            base.Update();

            if (stateFinished) {
                return;
            }

            if (isAuthority) {

                Ray aimRay;

                UpdateTrajectoryInfo(out currentTrajectoryInfo, out aimRay);
                UpdateVisualizers(currentTrajectoryInfo);

                if (!IsKeyDownAuthority()) {

                    if (!toggleActivate && age >= minimumDuration) {

                        // hold - activation by releasing
                        NextState();

                    } else if (toggleActivate) {
                        if (holdingActivationKey) {

                            // toggle - released from activation press
                            holdingActivationKey = false;

                        } else if (holdingCancelKey) {

                            // toggle - released from cancel press (confirmed cancel)
                            activatorSkillSlot.AddOneStock();
                            activatorSkillSlot.rechargeStopwatch = heldRechargeStopwatch;
                            outer.SetNextStateToMain();
                            stateFinished = true;
                            return;

                        }
                    }


                } else if (toggleActivate) {
                    if (IsNewKeyDownAuthority && !holdingCancelKey) {

                        // toggle - second press of skill button (step before cancel)
                        holdingCancelKey = true;

                    }
                }

                if (toggleActivate && inputBank.skill1.justPressed && age >= minimumDuration) {

                    // toggle - activation with primary
                    NextState();

                }
            }
        }

        protected virtual void NextState() {
            EntityState entityState = PickNextState();

            if (entityState != null) {
                outer.SetNextState(entityState);
            } else {
                outer.SetNextStateToMain();
            }

            stateFinished = true;
        }

        protected virtual void UpdateTrajectoryInfo(out AimThrowableBase.TrajectoryInfo dest, out Ray aimRay) {
            dest = default;
            RaycastHit hitInfo = default;
            hasCollided = false;
            aimRay = GetAimRay();

            if (rayRadius > 0f && Util.CharacterSpherecast(base.gameObject, aimRay, rayRadius, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal) && (bool)hitInfo.collider.GetComponent<HurtBox>()) {
                hasCollided = true;
            }

            if (!hasCollided) {
                hasCollided = Util.CharacterRaycast(base.gameObject, aimRay, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal);
            }

            if (hasCollided) {
                dest.hitPoint = hitInfo.point;
                dest.hitNormal = hitInfo.normal;
            } else {
                dest.hitPoint = aimRay.GetPoint(maxDistance);
                dest.hitNormal = -aimRay.direction;
            }

            if (originOverride != null) {
                aimRay.origin = originOverride.position;
            }

            Vector3 vector = dest.hitPoint - aimRay.origin;
            if (useGravity || addedGravity != 0f) {
                float num = projectileBaseSpeed;
                Vector2 vector2 = new Vector2(vector.x, vector.z);
                float magnitude = vector2.magnitude;
                float y = Trajectory.CalculateInitialYSpeed(magnitude / num, vector.y, totalGravity);
                Vector3 vector3 = new Vector3(vector2.x / magnitude * num, y, vector2.y / magnitude * num);
                dest.speedOverride = vector3.magnitude;
                dest.finalRay = new Ray(aimRay.origin, vector3 / dest.speedOverride);
                dest.travelTime = Trajectory.CalculateGroundTravelTime(num, magnitude);
            } else {
                dest.speedOverride = projectileBaseSpeed;
                dest.finalRay = aimRay;
                dest.travelTime = projectileBaseSpeed / vector.magnitude;
            }
        }

        private void CompleteArcVisualizerJob() {
            calculateArcPointsJobHandle.Complete();
            if (arcVisualizerLineRenderer) {
                Array.Resize(ref pointsBuffer, calculateArcPointsJob.outputPositions.Length);
                calculateArcPointsJob.outputPositions.CopyTo(pointsBuffer);
                arcVisualizerLineRenderer.SetPositions(pointsBuffer);
            }
        }

        private void UpdateVisualizers(AimThrowableBase.TrajectoryInfo trajectoryInfo) {
            if (arcVisualizerLineRenderer && calculateArcPointsJobHandle.IsCompleted) {
                calculateArcPointsJob.SetParameters(trajectoryInfo.finalRay.origin, trajectoryInfo.finalRay.direction * trajectoryInfo.speedOverride, trajectoryInfo.travelTime, arcVisualizerLineRenderer.positionCount, totalGravity);
                calculateArcPointsJobHandle = calculateArcPointsJob.Schedule(calculateArcPointsJob.outputPositions.Length, 32);
            }
            if (endpointVisualizerTransform) {
                endpointVisualizerTransform.SetPositionAndRotation(trajectoryInfo.hitPoint, Util.QuaternionSafeLookRotation(trajectoryInfo.hitNormal));
                if (!endpointVisualizerRadiusScale.Equals(0f)) {
                    endpointVisualizerTransform.localScale = new Vector3(endpointVisualizerRadiusScale, endpointVisualizerRadiusScale, endpointVisualizerRadiusScale);
                }
            }
        }

        private void OnPreRenderSceneCam(SceneCamera sceneCam) {
            if (arcVisualizerLineRenderer) {
                arcVisualizerLineRenderer.renderingLayerMask = ((sceneCam.cameraRigController.target == gameObject) ? 1u : 0u);
            }
            if (endpointVisualizerTransform) {
                endpointVisualizerTransform.gameObject.layer = ((sceneCam.cameraRigController.target == gameObject) ? LayerIndex.defaultLayer.intVal : LayerIndex.noDraw.intVal);
            }
        }
    }
}
