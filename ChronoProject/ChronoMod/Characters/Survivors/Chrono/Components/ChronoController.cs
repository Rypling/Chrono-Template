using ChronoMod.Modules.DamageTypes;
using R2API;
using RoR2;
using RoR2.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace ChronoMod.Survivors.Chrono.Components {
    public class ChronoController : MonoBehaviour {

        private const float holdDamageTime = 10f;

        private CharacterBody body;

        private Transform tcDamageRoot;

        private Transform specialRoot;

        private Transform crosshairRoot;

        private TextMeshProUGUI text;

        public float buffStopwatch;

        public float recentDamageTracker;

        private void Start() {
            body = GetComponent<CharacterBody>();
            if (body?.hasEffectiveAuthority == true) {
                GlobalEventManager.onClientDamageNotified += CheckIfDealtDamage;

                specialRoot = HUD.instancesList[0].transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler/TimeCollapseDamageRootSpecial");
                if (specialRoot) {
                    specialRoot.gameObject.SetActive(true);
                    tcDamageRoot = specialRoot.transform.Find("TimeCollapseDamage");
                    text = tcDamageRoot.Find("Text").GetComponent<TextMeshProUGUI>();
                }

                crosshairRoot = HUD.instancesList[0].transform.Find("MainContainer/MainUIArea/CrosshairCanvas/TimeCollapseDamageRootCrosshair");
            }
        }

        public void MoveUIToCrosshair() {
            tcDamageRoot.SetParent(crosshairRoot, false);
        }

        public void MoveUIToSpecial() {
            tcDamageRoot.SetParent(specialRoot, false);
        }

        private static float GetTimeForBuffDecay(int buffs) {
            float step = (buffs + 1f) / (ChronoStaticValues.temporalMaxBuffs - 1f);
            return Mathf.Lerp(ChronoStaticValues.temporalUpperDecayTime, ChronoStaticValues.temporalLowerDecayTime, step);
        }

        private void FixedUpdate() {
            if (NetworkServer.active) {
                if (body.HasBuff(ChronoBuffs.temporalRiftBuff)) {
                    buffStopwatch += Time.deltaTime;
                    if (buffStopwatch >= GetTimeForBuffDecay(body.GetBuffCount(ChronoBuffs.temporalRiftBuff))) {
                        body.RemoveBuff(ChronoBuffs.temporalRiftBuff);
                        buffStopwatch = 0f;
                        body.RecalculateStats();
                    }
                }
            }
        }

        // Method copied from HealthBar component
        private string FormatDamageLabel(float damage) {
            if (damage >= 1000000000f) {
                return Mathf.Round(damage / 1000000000f) + "B";
            } else if (damage >= 1000000f) {
                return Mathf.Round(damage / 1000000f) + "M";
            } else if (damage >= 10000f) {
                return Mathf.Round(damage / 1000f) + "K";
            }
            return Mathf.Round(damage).ToString();
        }

        private void Update() {
            if (text) {
                text.text = FormatDamageLabel(Mathf.Round(recentDamageTracker));
            }
        }

        private void CheckIfDealtDamage(DamageDealtMessage message) {
            if (body.hasEffectiveAuthority && message?.attacker == body.gameObject && message?.victim != body.gameObject && body != null) {
                if (!message.damageType.HasModdedDamageType(TimeCollapseType.damageType)) {
                    float damage = message.damage;
                    recentDamageTracker += damage;
                    this?.StartCoroutine(RemoveRecentDamage(damage));
                }
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
                MoveUIToSpecial();
                specialRoot?.gameObject?.SetActive(false);
            }
        }
    }
}