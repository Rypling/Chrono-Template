using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationReverser : EditorWindow {
    AnimationClip clip;

    [MenuItem("Tools/Reverse Animation Clip")]
    public static void ShowWindow() {
        GetWindow<AnimationReverser>("Reverse Animation");
    }

    void OnGUI() {
        clip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", clip, typeof(AnimationClip), false);

        if (GUILayout.Button("Reverse Clip") && clip != null) {
            ReverseClip(clip);
        }
    }

    void ReverseClip(AnimationClip clip) {
        string path = AssetDatabase.GetAssetPath(clip);
        AnimationClip newClip = new AnimationClip();
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings) {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            List<Keyframe> reversedKeys = new List<Keyframe>();

            float clipLength = clip.length;

            foreach (var key in curve.keys) {
                float flippedTime = clipLength - key.time;
                float flippedValue = key.value;

                // Optional: invert rotation values for yaw/pitch
                if (binding.propertyName.Contains("localEulerAngles.x") || binding.propertyName.Contains("localEulerAngles.y")) {
                    flippedValue *= -1f;
                }

                reversedKeys.Add(new Keyframe(flippedTime, flippedValue, -key.inTangent, -key.outTangent));
            }

            reversedKeys.Sort((a, b) => a.time.CompareTo(b.time));
            AnimationCurve reversedCurve = new AnimationCurve(reversedKeys.ToArray());
            newClip.SetCurve(binding.path, binding.type, binding.propertyName, reversedCurve);
        }

        string newPath = path.Replace(".anim", "_Reversed.anim");
        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Reversed animation saved at: " + newPath);
    }
}
