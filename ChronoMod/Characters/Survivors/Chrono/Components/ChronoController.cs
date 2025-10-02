using System.Collections;
using ChronoMod.Modules.DamageTypes;
using R2API;
using RoR2;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.Components {
    internal class ChronoController : MonoBehaviour {

        private const float holdDamageTime = 10f;

        private CharacterBody body;

        public float buffStopwatch;

        public float recentDamageTracker;

        private void Start() {
            body = GetComponent<CharacterBody>();
            GlobalEventManager.onServerDamageDealt += CheckIfDealtDamage;
        }

        private static float GetTimeForBuffDecay(int buffs) {
            float step = (buffs + 1f) / (ChronoStaticValues.temporalMaxBuffs - 1f);
            return Mathf.Lerp(ChronoStaticValues.temporalUpperDecayTime, ChronoStaticValues.temporalLowerDecayTime, step);
        }

        private void Update() {
            if (body.HasBuff(ChronoBuffs.temporalRiftBuff)) {
                buffStopwatch += Time.deltaTime;
                if (buffStopwatch >= GetTimeForBuffDecay(body.GetBuffCount(ChronoBuffs.temporalRiftBuff))) {
                    body.RemoveBuff(ChronoBuffs.temporalRiftBuff);
                    buffStopwatch = 0f;
                    body.RecalculateStats();
                }
            }
        }

        private void CheckIfDealtDamage(DamageReport damageReport) {
            if (damageReport?.attackerBody == body && damageReport?.victimBody != body && body != null) {
                if (!damageReport.damageInfo.HasModdedDamageType(TimeCollapseType.damageType)) {
                    float damage = damageReport.damageDealt;
                    recentDamageTracker += damage;
                    this?.StartCoroutine(RemoveRecentDamage(damage));
                } else {
                    body.SetBuffCount(ChronoBuffs.temporalRiftBuff.buffIndex, 0);
                    // if used time collapse, stop all coroutines so any new damage doesn't instantly get subtracted by old coroutines
                    StopAllCoroutines();
                    recentDamageTracker = 0f;
                }
            }
        }

        private IEnumerator RemoveRecentDamage(float damage) {
            yield return new WaitForSeconds(holdDamageTime);
            if (gameObject != null) { // nres, somewhere, for some reason
                recentDamageTracker = Mathf.Max(0f, recentDamageTracker - damage);
            }
        }

        private void Destroy() {
            GlobalEventManager.onServerDamageDealt -= CheckIfDealtDamage;
        }
    }
}