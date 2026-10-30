using UnityEngine;

namespace Riftwake
{
    public sealed class MainMenuBootstrap : MonoBehaviour
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
                cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
                camGo.AddComponent<AudioListener>();
            }

            if (FindObjectOfType<MainMenuController>() == null)
                gameObject.AddComponent<MainMenuController>();
        }
    }
}
