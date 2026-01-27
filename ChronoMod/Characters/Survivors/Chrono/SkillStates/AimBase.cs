using EntityStates;
using RoR2;
using RoR2BepInExPack.GameAssetPaths.Version_1_39_0;
using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChronoMod.Characters.Survivors.Chrono.SkillStates {

    /// <summary>
    /// Copy of decompiled code from EntityStates.AimThrowableBase but without projectile logic and with some initial numbers
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

        // No touchy
        protected GameObject _endpointVisualizerPrefab; // overcooked

        protected LineRenderer arcVisualizerLineRenderer;

        protected Transform endpointVisualizerTransform;

        protected Transform originOverride;

        protected float projectileBaseSpeed;

        protected float minimumDuration;

        protected bool useGravity;

        protected float addedGravity;

        private AimThrowableBase.CalculateArcPointsJob calculateArcPointsJob;

        private JobHandle calculateArcPointsJobHandle;

        private Vector3[] pointsBuffer = Array.Empty<Vector3>();

        private Action completeArcPointsVisualizerJobMethod;

        protected AimThrowableBase.TrajectoryInfo currentTrajectoryInfo;

        protected float totalGravity => (useGravity ? Physics.gravity.y : 0f) + Physics.gravity.y * addedGravity;

        public override void OnEnter() {
            base.OnEnter();
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
                characterBody.hideCrosshair = true;
            }

            originOverride = FindModelChild(originOverrideString);
            minimumDuration = baseMinimumDuration / attackSpeedStat;
            UpdateVisualizers(currentTrajectoryInfo);
            SceneCamera.onSceneCameraPreRender += OnPreRenderSceneCam;
        }

        public override void OnExit() {
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

            base.OnExit();
        }

        protected virtual bool KeyIsDown() {
            return IsKeyDownAuthority();
        }

        public override void FixedUpdate() {
            base.FixedUpdate();

            if (isAuthority && !KeyIsDown() && fixedAge >= minimumDuration) {
                UpdateTrajectoryInfo(out currentTrajectoryInfo);
                EntityState entityState = PickNextState();

                if (entityState != null) {
                    outer.SetNextState(entityState);
                } else {
                    outer.SetNextStateToMain();
                }
            }
        }

        protected virtual EntityState PickNextState() {
            return null;
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Skill;
        }

        public override void Update() {
            base.Update();

            UpdateTrajectoryInfo(out currentTrajectoryInfo);
            UpdateVisualizers(currentTrajectoryInfo);
        }

        protected virtual void UpdateTrajectoryInfo(out AimThrowableBase.TrajectoryInfo dest) {
            dest = default;
            Ray aimRay = GetAimRay();
            RaycastHit hitInfo = default;
            bool flag = false;

            if (rayRadius > 0f && Util.CharacterSpherecast(base.gameObject, aimRay, rayRadius, out hitInfo, maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal) && (bool)hitInfo.collider.GetComponent<HurtBox>()) {
                flag = true;
            }

            if (!flag) {
                flag = Util.CharacterRaycast(base.gameObject, aimRay, out hitInfo, maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            }

            if (flag) {
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
