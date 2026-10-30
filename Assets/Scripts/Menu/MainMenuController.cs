using UnityEngine;

namespace Riftwake
{
    public sealed class MainMenuController : MonoBehaviour
    {
        void Awake()
        {
            GameSession.Ensure();
            Progression.Ensure();
        }

        void OnGUI()
        {
            DrawBackdrop();

            float w = Screen.width;
            float h = Screen.height;

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            title.normal.textColor = new Color(0.92f, 0.86f, 0.7f);

            var sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            sub.normal.textColor = new Color(0.75f, 0.72f, 0.65f);

            GUI.Label(new Rect(0, h * 0.14f, w, 60), "RIFTWAKE", title);
            GUI.Label(new Rect(w * 0.2f, h * 0.24f, w * 0.6f, 50),
                "A top-down action RPG — full first campaign through the Verge.", sub);

            var btn = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            float bw = w * 0.28f;
            float bx = (w - bw) * 0.5f;

            if (GUI.Button(new Rect(bx, h * 0.42f, bw, 48), "New Campaign", btn))
                GameSession.Ensure().StartNewGame();

            GUI.enabled = SaveSystem.HasSave();
            if (GUI.Button(new Rect(bx, h * 0.42f + 60, bw, 48), "Continue", btn))
                GameSession.Ensure().ContinueFromSave();
            GUI.enabled = true;

            if (GUI.Button(new Rect(bx, h * 0.42f + 120, bw, 48), "Quit", btn))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            var foot = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            foot.normal.textColor = new Color(0.55f, 0.55f, 0.5f);
            GUI.Label(new Rect(0, h * 0.88f, w, 30), "Peas project  ·  Riftwake v2 campaign", foot);
        }

        static void DrawBackdrop()
        {
            var prev = GUI.color;
            GUI.color = new Color(0.05f, 0.06f, 0.08f, 1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.1f, 0.08f, 0.45f);
            GUI.DrawTexture(new Rect(0, Screen.height * 0.6f, Screen.width, Screen.height * 0.4f), Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
