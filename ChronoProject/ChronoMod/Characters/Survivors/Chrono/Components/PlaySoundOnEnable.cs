using RoR2.Audio;
using UnityEngine;

namespace ChronoMod.Characters.Survivors.Chrono.Components {
    public class PlaySoundOnEnable : MonoBehaviour {

        public string soundEvent;

        public void OnEnable() {
            PointSoundManager.EmitSoundLocal(soundEvent, transform.position);
        }
    }
}
