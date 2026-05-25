using RoR2.UI;
using TMPro;
using UnityEngine;

namespace ChronoMod.Characters.Survivors.Chrono.Components {

    [RequireComponent(typeof(HUD))]
    public class AddTimeCollapseElement : MonoBehaviour {

        private void Awake() {

            GameObject collapseDamageRootSpecial = new GameObject("TimeCollapseDamageRootSpecial", typeof(RectTransform));
            collapseDamageRootSpecial.transform.SetParent(transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler"));
            collapseDamageRootSpecial.transform.localPosition = new Vector3(3f, 100f, 0f);
            collapseDamageRootSpecial.SetActive(false);

            GameObject collapseDamageRootCrosshair = new GameObject("TimeCollapseDamageRootCrosshair", typeof(RectTransform));
            collapseDamageRootCrosshair.transform.SetParent(transform.Find("MainContainer/MainUIArea/CrosshairCanvas"));
            collapseDamageRootCrosshair.transform.localPosition = new Vector3(0f, 40f, 0f);

            GameObject collapseDamage = Object.Instantiate(GetComponent<HUD>().healthBar.transform.Find("SharedSufferingRoot")?.gameObject, collapseDamageRootSpecial.transform);
            collapseDamage.name = "TimeCollapseDamage";
            collapseDamage.transform.localPosition = Vector3.zero;
            Transform text = collapseDamage.transform.Find("Text");
            text.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            text.transform.localPosition = new Vector3(-175f, 140f, 0f);
            Transform arrow = collapseDamage.transform.Find("Arrow");
            arrow.localPosition = Vector3.zero;
            arrow.localEulerAngles = Vector3.zero;

            Destroy(this);
        }
    }
}
