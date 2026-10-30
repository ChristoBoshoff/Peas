using UnityEngine;

namespace Riftwake
{
    /// <summary>Builds the class-select presentation scene at runtime.</summary>
    public sealed class ClassSelectBootstrap : MonoBehaviour
    {
        void Awake()
        {
            EnsureTags();
            GameSession.Ensure();

            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
                cam.transform.position = new Vector3(0f, 1f, -10f);
                camGo.AddComponent<AudioListener>();
            }

            if (FindObjectOfType<ClassSelectController>() == null)
                gameObject.AddComponent<ClassSelectController>();

            // Soft key light for atmosphere even though UI is OnGUI-driven.
            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("KeyLight");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.85f, 0.65f);
                light.intensity = 0.6f;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        static void EnsureTags()
        {
            // Tags are defined in TagManager; runtime Find uses them after first Editor import.
        }
    }
}
