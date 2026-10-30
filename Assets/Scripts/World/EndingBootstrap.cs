using UnityEngine;

namespace Riftwake
{
    public sealed class EndingBootstrap : MonoBehaviour
    {
        void Awake()
        {
            GameSession.Ensure();
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
                camGo.AddComponent<AudioListener>();
            }

            if (FindObjectOfType<EndingController>() == null)
                gameObject.AddComponent<EndingController>();
        }
    }
}
