using UnityEngine;

namespace Riftwake
{
    /// <summary>Tutorial mentor in Bracken Hollow. Proximity dialogue drives the first-hour prompts.</summary>
    public sealed class ElderBram : MonoBehaviour
    {
        Transform _player;
        TutorialDirector _tutorial;
        float _speakCooldown;

        public void Configure(Transform player, TutorialDirector tutorial)
        {
            _player = player;
            _tutorial = tutorial;
        }

        void Update()
        {
            if (_player == null || _tutorial == null)
                return;

            if (_speakCooldown > 0f)
                _speakCooldown -= Time.deltaTime;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < 3.5f && _speakCooldown <= 0f)
            {
                _tutorial.OnNearElder();
                _speakCooldown = 6f;
            }
        }

        void OnGUI()
        {
            if (_player == null)
                return;
            if (Vector3.Distance(transform.position, _player.position) > 5f)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            var screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.4f);
            if (screen.z < 0f)
                return;

            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            style.normal.textColor = new Color(0.95f, 0.9f, 0.75f);
            float y = Screen.height - screen.y;
            GUI.Box(new Rect(screen.x - 90, y - 36, 180, 32), "Elder Bram", style);
        }
    }
}
