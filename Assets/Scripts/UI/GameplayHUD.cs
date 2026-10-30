using UnityEngine;

namespace Riftwake
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        Health _health;
        FocusResource _focus;
        PlayerCombat _combat;
        PlayerMotor _motor;
        HeroPath _path;
        string _banner = string.Empty;
        float _bannerTime;
        bool _dead;
        string _missionTitle = string.Empty;

        public void Bind(Health health, FocusResource focus, PlayerCombat combat, PlayerMotor motor, HeroPath path)
        {
            _health = health;
            _focus = focus;
            _combat = combat;
            _motor = motor;
            _path = path;
            if (_health != null)
                _health.Died += OnPlayerDied;
        }

        public void SetMissionTitle(string title) => _missionTitle = title;

        void OnDestroy()
        {
            if (_health != null)
                _health.Died -= OnPlayerDied;
        }

        void OnPlayerDied(Health _)
        {
            _dead = true;
            SetPlayerInput(false);
            ShowBanner("You fall. Press R to rise, or Esc for main menu (progress is saved).");
        }

        void SetPlayerInput(bool enabled)
        {
            if (_motor != null) _motor.InputEnabled = enabled;
            if (_combat != null) _combat.InputEnabled = enabled;
        }

        public void ShowBanner(string text, float seconds = 4f)
        {
            _banner = text;
            _bannerTime = seconds;
        }

        void Update()
        {
            if (_bannerTime > 0f)
                _bannerTime -= Time.deltaTime;

            if (_dead && Input.GetKeyDown(KeyCode.R))
            {
                _dead = false;
                var prog = Progression.Instance;
                float max = 100f + (prog != null ? prog.VitalityBonus : 0f);
                if (_health != null)
                    _health.Configure(max);
                SetPlayerInput(true);
                ShowBanner("You rise again.", 2f);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameSession.Instance?.Persist();
                GameSession.Ensure().ReturnToMenu();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                GameSession.Instance?.Persist();
                ShowBanner("Campaign saved.", 1.5f);
            }
        }

        void OnGUI()
        {
            DrawBar(new Rect(24, 20, 260, 22), "Vitality",
                _health != null ? _health.Normalized : 0f,
                new Color(0.75f, 0.18f, 0.18f));

            bool showFocus = _path == HeroPath.Spellwright || _path == HeroPath.Wildbinder;
            if (showFocus)
            {
                DrawBar(new Rect(24, 50, 260, 18), "Focus",
                    _focus != null ? _focus.Normalized : 0f,
                    new Color(0.25f, 0.45f, 0.9f));
            }

            var prog = Progression.Instance;
            var info = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            info.normal.textColor = new Color(0.85f, 0.82f, 0.72f);

            float infoY = showFocus ? 78f : 50f;
            if (prog != null)
            {
                GUI.Label(new Rect(24, infoY, 420, 20),
                    $"Lv {prog.Level}   XP {prog.Xp}/{Progression.XpToNext(prog.Level)}   Gold {prog.Gold}", info);
                infoY += 20f;
            }

            if (!string.IsNullOrEmpty(_missionTitle))
            {
                var mt = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight };
                mt.normal.textColor = new Color(0.9f, 0.85f, 0.65f);
                GUI.Label(new Rect(Screen.width - 420, 18, 400, 24), _missionTitle, mt);
            }

            string action = _combat != null ? _combat.PrimaryActionName : "Attack";
            string secondary = _combat != null ? _combat.SecondaryActionName : "";
            bool unlocked = prog != null && prog.SecondaryUnlocked;
            GUI.Label(new Rect(24, Screen.height - 72, 720, 24),
                $"Path: {GameSession.PathDisplayName(_path)}  |  LMB: {action}" +
                (unlocked ? $"  |  Q: {secondary}" : "  |  Q: (unlock at Lv 3)"), info);
            GUI.Label(new Rect(24, Screen.height - 48, 720, 24),
                "WASD move · Mouse aim · F5 save · Esc menu", info);

            if (_bannerTime > 0f && !string.IsNullOrEmpty(_banner))
            {
                var bannerStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
                bannerStyle.normal.textColor = new Color(0.95f, 0.9f, 0.75f);
                GUI.Box(new Rect(Screen.width * 0.15f, 88, Screen.width * 0.7f, 64), _banner, bannerStyle);
            }

            if (_dead)
            {
                var deadStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                deadStyle.normal.textColor = new Color(0.9f, 0.3f, 0.25f);
                GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 40), "Fallen", deadStyle);
            }
        }

        static void DrawBar(Rect rect, string label, float fill, Color fillColor)
        {
            var prev = GUI.color;
            GUI.color = new Color(0.1f, 0.1f, 0.12f, 0.85f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(fill), rect.height - 4), Texture2D.whiteTexture);
            GUI.color = prev;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 8, rect.y - 1, rect.width, rect.height), label, style);
        }
    }
}
