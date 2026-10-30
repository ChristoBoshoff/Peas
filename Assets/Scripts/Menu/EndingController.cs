using UnityEngine;

namespace Riftwake
{
    public sealed class EndingController : MonoBehaviour
    {
        void Awake()
        {
            GameSession.Ensure();
            var session = GameSession.Instance;
            session.CampaignComplete = true;
            session.Persist();
        }

        void OnGUI()
        {
            var prev = GUI.color;
            GUI.color = new Color(0.04f, 0.05f, 0.07f, 1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;

            float w = Screen.width;
            float h = Screen.height;

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 40,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            title.normal.textColor = new Color(0.95f, 0.88f, 0.55f);

            var body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            body.normal.textColor = new Color(0.8f, 0.78f, 0.7f);

            string path = GameSession.Instance != null
                ? GameSession.PathDisplayName(GameSession.Instance.SelectedPath)
                : "Riftborn";

            GUI.Label(new Rect(0, h * 0.22f, w, 50), "The Verge Holds", title);
            GUI.Label(new Rect(w * 0.18f, h * 0.34f, w * 0.64f, 120),
                $"As a {path}, you closed the Rift Heart and quieted the tear. " +
                "Bracken Hollow still stands. Your campaign is complete — for now.",
                body);

            var btn = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            if (GUI.Button(new Rect(w * 0.35f, h * 0.62f, w * 0.3f, 48), "Main Menu", btn))
                GameSession.Ensure().ReturnToMenu();

            if (GUI.Button(new Rect(w * 0.35f, h * 0.62f + 60, w * 0.3f, 48), "New Campaign", btn))
            {
                SaveSystem.Delete();
                GameSession.Ensure().StartNewGame();
            }
        }
    }
}
