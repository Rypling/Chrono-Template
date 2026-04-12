using EntityStates;
using RoR2.Projectile;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class TimeCollapseFire : BaseState {

        public Vector3 attackOrigin;

        private float duration = 0.4f;

        private float stopwatch = 0f;

        //private bool FireTimeCollapse() {
        //    UpdateTargets();
        //    HurtBox hurtBox = currentTarget.hurtBox;
        //    if ((bool)hurtBox) {
        //        int buffCount = characterBody.GetBuffCount(ChronoBuffs.temporalRiftBuff);
        //        LightningOrb orb = new LightningOrb {
        //            attacker = base.gameObject,
        //            origin = transform.position,
        //            lightningType = LightningOrb.LightningType.MageLightning,
        //            damageColorIndex = DamageColorIndex.Sniper,
        //            damageValue = characterBody.GetComponent<ChronoController>().recentDamageTracker * (1 + buffCount / ChronoStaticValues.temporalMaxBuffs),
        //            isCrit = true,
        //            procChainMask = default(ProcChainMask),
        //            procCoefficient = 1f,
        //            target = hurtBox
        //        };
        //        orb.AddModdedDamageType(TimeCollapseType.damageType);
        //        OrbManager.instance.AddOrb(orb);
        //        currentTarget = default(EquipmentSlot.UserTargetInfo);
        //        return true;
        //    }
        //    return false;
        //}

        public override void OnEnter() {
            base.OnEnter();

            if (isAuthority) {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo {
                    projectilePrefab = ChronoAssets.collapseProjectile,
                    position = attackOrigin,
                    rotation = Quaternion.identity,
                    owner = base.gameObject,
                    damage = damageStat,
                    crit = base.characterBody.RollCrit()
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void Update() {
            base.Update();

            stopwatch += Time.deltaTime;
            if (stopwatch > duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority() {
            return InterruptPriority.Stun;
        }
    }
}