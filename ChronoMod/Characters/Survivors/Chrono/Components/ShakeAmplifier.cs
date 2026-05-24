using RoR2;
using UnityEngine;

namespace ChronoMod.Characters.Survivors.Chrono.Components {
    public class ShakeAmplifier : MonoBehaviour {

        public ShakeEmitter shakeEmitter;

        private float amplitudeInitial;

        private float radiusInitial;

        public float amplitudeAddition;

        public float radiusAddition;

        public void Start() {
            if (!shakeEmitter) {
                shakeEmitter = GetComponent<ShakeEmitter>();
            }

            if (shakeEmitter) {
                amplitudeInitial = shakeEmitter.wave.amplitude;
                radiusInitial = shakeEmitter.radius;
            } else {
                Log.Warning($"ShakeAmplifier on {gameObject.name} could not find ShakeEmitter!");
            }
        }

        public void Update() {
            if (shakeEmitter) {
                shakeEmitter.wave.amplitude += amplitudeAddition * Time.deltaTime;
                shakeEmitter.radius += radiusAddition * Time.deltaTime;
            }
        }
        public void OnDisable() {
            shakeEmitter.wave.amplitude = amplitudeInitial;
            shakeEmitter.radius = radiusInitial;
        }
    }
}
