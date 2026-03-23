using ChronoMod.Characters.Survivors.Chrono.Components;
using EntityStates;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.SkillStates {
    public class ContinuumFreeze : BaseSkillState {
        public static float duration = 0.1f;

        public override void OnEnter() {
            base.OnEnter();

            FireBlastAttack();
            CreateWard();

            if (!characterMotor.isGrounded) {
                SmallHop(characterMotor, 14f);
            }
        }

        private void FireBlastAttack() {
            BlastAttack blastAttack = new BlastAttack {
                attacker = gameObject,
                baseDamage = characterBody.damage * ChronoStaticValues.continuumDamageCoefficient,
                crit = characterBody.RollCrit(),
                position = characterBody.transform.position,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = gameObject,
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f,
                radius = 15f,
                teamIndex = characterBody.teamComponent.teamIndex,
            };
            blastAttack.damageType |= DamageType.Freeze2s;
            blastAttack.Fire();
        }

        private void CreateWard() {
            GameObject ward = Object.Instantiate(ChronoAssets.continuumWardPrefab);
            ward.transform.position = characterBody.transform.position;
            float buffFrac = characterBody.GetBuffCount(ChronoBuffs.temporalRiftBuff) / ChronoStaticValues.temporalMaxBuffs;
            float lifetime = Mathf.Lerp(ChronoStaticValues.continuumWardMinDuration, ChronoStaticValues.continuumWardMaxDuration, buffFrac);
            ward.GetComponent<BuffWard>().expireDuration = lifetime;
            ward.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
            ward.GetComponent<ChildLocator>().FindChild("VFXController").GetComponent<BeginRapidlyActivatingAndDeactivating>().delayBeforeBeginningBlinking = lifetime - 1f;
            ward.AddComponent<ContinuumFreezeController>();
            Util.PlaySound("Play_seeker_skill2_alt_fire", ward);
        }

        public override void FixedUpdate() {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration) {
                outer.SetNextStateToMain();
                return;
            }
        }
    }
}