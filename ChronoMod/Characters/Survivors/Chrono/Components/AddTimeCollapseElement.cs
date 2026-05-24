using RoR2.UI;
using TMPro;
using UnityEngine;

namespace ChronoMod.Characters.Survivors.Chrono.Components {

    [RequireComponent(typeof(HUD))]
    public class AddTimeCollapseElement : MonoBehaviour {

        private void Awake() {
            HUD hud = GetComponent<HUD>();
            GameObject collapseDamageRoot = Object.Instantiate(hud.healthBar.transform.Find("SharedSufferingRoot")?.gameObject, transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler"));
            collapseDamageRoot.name = "TimeCollapseDamageRoot";
            collapseDamageRoot.transform.localPosition = new Vector3(3f, 100f, 0f);
            Transform text = collapseDamageRoot.transform.Find("Text");
            text.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            text.transform.localPosition = new Vector3(-175f, 140f, 0f);
            Transform arrow = collapseDamageRoot.transform.Find("Arrow");
            arrow.localPosition = Vector3.zero;
            arrow.localEulerAngles = Vector3.zero;
            collapseDamageRoot.SetActive(false);
            Destroy(this);
        }
    }
}
