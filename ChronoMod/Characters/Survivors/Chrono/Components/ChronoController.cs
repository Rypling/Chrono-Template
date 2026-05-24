using ChronoMod.Modules.DamageTypes;
using R2API;
using RoR2;
using RoR2.UI;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ChronoMod.Survivors.Chrono.Components {
    internal class ChronoController : MonoBehaviour {

        private const float holdDamageTime = 10f;

        private CharacterBody body;

        private GameObject root;

        private TextMeshProUGUI text;

        public float buffStopwatch;

        public float recentDamageTracker;

        private void Start() {
            body = GetComponent<CharacterBody>();
            if (body?.hasEffectiveAuthority == true) {
                GlobalEventManager.onClientDamageNotified += CheckIfDealtDamage;

                root = HUD.instancesList[0].transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler/TimeCollapseDamageRoot")?.gameObject;
                if (root) {
                    root.SetActive(true);
                    text = root.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private static float GetTimeForBuffDecay(int buffs) {
            float step = (buffs + 1f) / (ChronoStaticValues.temporalMaxBuffs - 1f);
            return Mathf.Lerp(ChronoStaticValues.temporalUpperDecayTime, ChronoStaticValues.temporalLowerDecayTime, step);
        }

        private void FixedUpdate() {
            if (body.HasBuff(ChronoBuffs.temporalRiftBuff)) {
                buffStopwatch += Time.deltaTime;
                if (buffStopwatch >= GetTimeForBuffDecay(body.GetBuffCount(ChronoBuffs.temporalRiftBuff))) {
                    body.RemoveBuff(ChronoBuffs.temporalRiftBuff);
                    buffStopwatch = 0f;
                    body.RecalculateStats();
                }
            }
        }

        private void Update() {
            if (text) {
                text.text = FormatDamageLabel(Mathf.Round(recentDamageTracker));
            }
        }

        // Method copied from HealthBar component
        private string FormatDamageLabel(float damage) {
            if (damage >= 10000f) {
                return Mathf.Round(damage / 1000f) + "K";
            }
            if (damage >= 1000000f) {
                return Mathf.Round(damage / 1000000f) + "M";
            }
            return Mathf.Round(damage).ToString();
        }

        private void CheckIfDealtDamage(DamageDealtMessage message) {
            if (message?.attacker == body.gameObject && message?.victim != body.gameObject && body != null) {
                if (!message.damageType.HasModdedDamageType(TimeCollapseType.damageType)) {
                    float damage = message.damage;
                    recentDamageTracker += damage;
                    this?.StartCoroutine(RemoveRecentDamage(damage));
                }//  else {
                //    body.SetBuffCount(ChronoBuffs.temporalRiftBuff.buffIndex, 0);
                //    // if used time collapse, stop all coroutines so any new damage doesn't instantly get subtracted by old coroutines
                //    StopAllCoroutines();
                //    recentDamageTracker = 0f;
                //}
            }
        }

        private IEnumerator RemoveRecentDamage(float damage) {
            yield return new WaitForSeconds(holdDamageTime);
            if (gameObject != null) { // nres, somewhere, for some reason
                recentDamageTracker = Mathf.Max(0f, recentDamageTracker - damage);
            }
        }

        private void OnDestroy() {
            if (body?.hasEffectiveAuthority == true) {
                GlobalEventManager.onClientDamageNotified -= CheckIfDealtDamage;
                if (root) {
                    root.SetActive(false);
                }
            }
        }
    }
}