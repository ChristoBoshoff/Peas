using UnityEngine;

namespace Riftwake
{
    public sealed class ClassSelectController : MonoBehaviour
    {
        HeroPath _highlighted = HeroPath.Steelhand;
        string _status = "Choose your path, Riftborn.";

        void Awake()
        {
            GameSession.Ensure();
            Progression.Ensure();
        }

        void OnGUI()
        {
            DrawBackdrop();

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            title.normal.textColor = new Color(0.92f, 0.86f, 0.7f);

            var subtitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            subtitle.normal.textColor = new Color(0.75f, 0.72f, 0.65f);

            var body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            body.normal.textColor = new Color(0.85f, 0.82f, 0.75f);

            float w = Screen.width;
            float h = Screen.height;

            GUI.Label(new Rect(0, h * 0.08f, w, 54), "RIFTWAKE", title);
            GUI.Label(new Rect(w * 0.2f, h * 0.16f, w * 0.6f, 48),
                "A rift tears open. You fall into the Verge. Choose how you will fight.",
                subtitle);

            DrawPathCard(HeroPath.Steelhand, new Rect(w * 0.08f, h * 0.32f, w * 0.26f, h * 0.38f), body);
            DrawPathCard(HeroPath.Wildbinder, new Rect(w * 0.37f, h * 0.32f, w * 0.26f, h * 0.38f), body);
            DrawPathCard(HeroPath.Spellwright, new Rect(w * 0.66f, h * 0.32f, w * 0.26f, h * 0.38f), body);

            GUI.Label(new Rect(w * 0.2f, h * 0.74f, w * 0.6f, 40), _status, subtitle);

            var enterStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            if (GUI.Button(new Rect(w * 0.35f, h * 0.82f, w * 0.3f, 48), "Begin Campaign", enterStyle))
                BeginAdventure();

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                Highlight(HeroPath.Steelhand);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                Highlight(HeroPath.Wildbinder);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                Highlight(HeroPath.Spellwright);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                BeginAdventure();
        }

        void DrawBackdrop()
        {
            var prev = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.09f, 1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(0.18f, 0.12f, 0.08f, 0.55f);
            GUI.DrawTexture(new Rect(0, Screen.height * 0.55f, Screen.width, Screen.height * 0.45f), Texture2D.whiteTexture);
            GUI.color = prev;
        }

        void DrawPathCard(HeroPath path, Rect rect, GUIStyle body)
        {
            bool selected = _highlighted == path;
            var prev = GUI.color;
            GUI.color = selected ? new Color(0.28f, 0.22f, 0.14f, 0.95f) : new Color(0.12f, 0.13f, 0.15f, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = selected ? new Color(0.95f, 0.78f, 0.35f, 1f) : new Color(0.35f, 0.35f, 0.38f, 1f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f), Texture2D.whiteTexture);
            GUI.color = prev;

            var nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            nameStyle.normal.textColor = selected ? new Color(1f, 0.9f, 0.6f) : new Color(0.85f, 0.85f, 0.82f);

            GUI.Label(new Rect(rect.x, rect.y + 16, rect.width, 32), GameSession.PathDisplayName(path), nameStyle);
            GUI.Label(new Rect(rect.x + 16, rect.y + 60, rect.width - 32, rect.height - 120), GameSession.PathBlurb(path), body);

            string keyHint = path == HeroPath.Steelhand ? "[1]" : path == HeroPath.Wildbinder ? "[2]" : "[3]";
            if (GUI.Button(new Rect(rect.x + 24, rect.yMax - 52, rect.width - 48, 36), $"Select {keyHint}"))
                Highlight(path);
        }

        void Highlight(HeroPath path)
        {
            _highlighted = path;
            _status = $"Path set: {GameSession.PathDisplayName(path)}. Press Enter to begin.";
        }

        void BeginAdventure()
        {
            GameSession.Ensure().ChoosePath(_highlighted);
        }
    }
}
