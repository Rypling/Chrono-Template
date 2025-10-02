using System.Linq;
using ChronoMod.Modules.DamageTypes;
using ChronoMod.Survivors.Chrono.Components;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class TimeCollapse : BaseSkillState {
        public static float duration = 0.3f;

        private bool stillHoldingKey = true;

        private BullseyeSearch targetFinder = new BullseyeSearch();

        private EquipmentSlot.UserTargetInfo currentTarget;

        private Indicator targetIndicator;

        private float firedStopwatch;

        private float heldRechargeStopwatch;

        private bool IsNewKeyDownAuthority => IsKeyDownAuthority() && !stillHoldingKey;

        private bool hasFired = false;

        public override void OnEnter() {
            base.OnEnter();
            heldRechargeStopwatch = skillLocator.special.rechargeStopwatch;
            targetIndicator = new Indicator(base.gameObject, null);
            ConfigureTargetFinderForEnemies();
        }

        private void ConfigureTargetFinderBase() {
            targetFinder.teamMaskFilter = TeamMask.allButNeutral;
            targetFinder.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            targetFinder.sortMode = BullseyeSearch.SortMode.Angle;
            targetFinder.filterByLoS = true;
            float extraRaycastDistance;
            Ray ray = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), base.gameObject, out extraRaycastDistance);
            targetFinder.searchOrigin = ray.origin;
            targetFinder.searchDirection = ray.direction;
            targetFinder.maxAngleFilter = 10f;
            targetFinder.viewer = characterBody;
        }

        private void ConfigureTargetFinderForEnemies() {
            ConfigureTargetFinderBase();
            targetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
            targetFinder.RefreshCandidates();
            targetFinder.FilterOutGameObject(base.gameObject);
        }

        private void UpdateTargets() {
            ConfigureTargetFinderForEnemies();
            HurtBox source = targetFinder.GetResults().FirstOrDefault();
            currentTarget = new EquipmentSlot.UserTargetInfo(source);
            bool flag5 = currentTarget.transformToIndicateAt;
            if (flag5) {
                targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator");
            }
            targetIndicator.active = flag5;
            targetIndicator.targetTransform = (flag5 ? currentTarget.transformToIndicateAt : null);
        }

        private bool FireTimeCollapse() {
            UpdateTargets();
            HurtBox hurtBox = currentTarget.hurtBox;
            if ((bool)hurtBox) {
                int buffCount = characterBody.GetBuffCount(ChronoBuffs.temporalRiftBuff);
                LightningOrb orb = new LightningOrb {
                    attacker = base.gameObject,
                    origin = transform.position,
                    lightningType = LightningOrb.LightningType.MageLightning,
                    damageColorIndex = DamageColorIndex.Sniper,
                    damageValue = characterBody.GetComponent<ChronoController>().recentDamageTracker * (1 + buffCount / ChronoStaticValues.temporalMaxBuffs),
                    isCrit = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 1f,
                    target = hurtBox
                };
                orb.AddModdedDamageType(TimeCollapseType.damageType);
                OrbManager.instance.AddOrb(orb);
                Log.Info($"recentDamageTracker was at {characterBody.GetComponent<ChronoController>().recentDamageTracker}, multiplied it by {1 + buffCount / ChronoStaticValues.temporalMaxBuffs}");
                currentTarget = default(EquipmentSlot.UserTargetInfo);
                return true;
            }
            return false;
        }

        public override void Update() {
            base.Update();
            UpdateTargets();

            if (!IsKeyDownAuthority() && stillHoldingKey) {
                stillHoldingKey = false;
            }

            if (IsNewKeyDownAuthority && !hasFired) {
                hasFired = true;
                WaitForRelease nextState = new WaitForRelease();
                nextState.skillSlot = SkillSlot.Special;
                outer.SetNextState(nextState);
                skillLocator.special.AddOneStock();
                skillLocator.special.rechargeStopwatch = heldRechargeStopwatch;
                return;
            }

            if (inputBank.skill1.justPressed && !hasFired) {
                if (FireTimeCollapse()) {
                    hasFired = true;
                }
            }

            if (hasFired) {
                firedStopwatch += Time.deltaTime;
            }


            if (isAuthority && firedStopwatch >= duration) {
                outer.SetNextStateToMain();
                return;
            }

        }

        public override void OnExit() {
            base.OnExit();
            targetIndicator.active = false;
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Stun;
        }
    }
}