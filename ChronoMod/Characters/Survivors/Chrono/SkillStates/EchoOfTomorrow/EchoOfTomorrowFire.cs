using EntityStates;
using RoR2;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class EchoOfTomorrowFire : BaseState {
        public override void OnEnter() {
            base.OnEnter();
            PlayAnimation("FullBody, Override", "StepBrothersLoopExit");
            FireExplosion();
            characterBody.AddTimedBuff(ChronoBuffs.timeWarpBuff.buffIndex, 4f);
        }

        private void FireExplosion() {
            BlastAttack blastAttack = new BlastAttack {
                attacker = gameObject,
                baseDamage = characterBody.damage * ChronoStaticValues.echoDamageCoefficient,
                crit = characterBody.RollCrit(),
                position = characterBody.transform.position,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = gameObject,
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f,
                radius = 10f,
                teamIndex = characterBody.teamComponent.teamIndex,
            };
            blastAttack.damageType |= DamageType.Freeze2s;
            blastAttack.Fire();
        }
    }
}