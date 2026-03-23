using ChronoMod.Survivors.Chrono;
using RoR2;
using UnityEngine;

namespace ChronoMod.Characters.Survivors.Chrono.Components {
    public class ContinuumFreezeController : MonoBehaviour {
        public void OnDestroy() {
            EffectManager.SpawnEffect(ChronoAssets.continuumEndEffect, new EffectData {
                origin = transform.position,
                rotation = transform.rotation,
                scale = 30f
            }, transmit: false);
        }
    }
}
