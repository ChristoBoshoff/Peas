using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// Beats for the Bracken Hollow intro: arrive → move → aim/attack → survive Ashmites.
    /// </summary>
    public sealed class TutorialDirector : MonoBehaviour
    {
        public enum Beat
        {
            Arrival,
            LearnMove,
            LearnCombat,
            FirstBlood,
            HoldTheHollow,
            Complete
        }

        Beat _beat = Beat.Arrival;
        GameplayHUD _hud;
        PlayerMotor _motor;
        float _timer;
        int _kills;
        bool _moved;
        bool _attacked;

        public Beat Current => _beat;

        public void Bind(GameplayHUD hud, PlayerMotor motor)
        {
            _hud = hud;
            _motor = motor;
            _timer = 0f;
            Advance(Beat.Arrival, "The rift dumps you at the edge of Bracken Hollow. Elder Bram waits ahead.");
        }

        public void RegisterKill()
        {
            _kills++;
            if (_beat == Beat.LearnCombat || _beat == Beat.FirstBlood)
                Advance(Beat.FirstBlood, "Ashmite down. More will smell the fight — keep moving.");
            if (_kills >= 3 && _beat != Beat.Complete)
                Advance(Beat.Complete, "The Hollow holds for now. First trial complete — this is how the Verge begins.");
        }

        public void OnNearElder()
        {
            switch (_beat)
            {
                case Beat.Arrival:
                case Beat.LearnMove:
                    Advance(Beat.LearnMove, "Bram: \"Find your feet. WASD to walk the path. Look with the mouse.\"");
                    break;
                case Beat.LearnCombat:
                    Advance(Beat.LearnCombat, "Bram: \"When they come, face them and strike. Left mouse is your primary.\"");
                    break;
                case Beat.HoldTheHollow:
                    _hud?.ShowBanner("Bram: \"Clear the Ashmites. The Hollow remembers those who stand.\"");
                    break;
                case Beat.Complete:
                    _hud?.ShowBanner("Bram: \"You learn quickly, Riftborn. Stronger trials wait beyond this clearing.\"");
                    break;
            }
        }

        void Update()
        {
            _timer += Time.deltaTime;

            if (_motor != null && _motor.IsMoving)
                _moved = true;

            if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
                _attacked = true;

            if (_beat == Beat.Arrival && _timer > 3.5f)
                Advance(Beat.LearnMove, "Move with WASD or the arrow keys. Face the mouse cursor.");

            if (_beat == Beat.LearnMove && _moved && _timer > 2f)
                Advance(Beat.LearnCombat, "Left-click to use your path's primary. Ashmites stir in the brush.");

            if (_beat == Beat.LearnCombat && _attacked && _timer > 1.5f)
                Advance(Beat.HoldTheHollow, "Survive the Ashmites. Stay near Bram if you need a word.");
        }

        void Advance(Beat next, string banner)
        {
            if (_beat == Beat.Complete && next != Beat.Complete)
                return;

            _beat = next;
            _timer = 0f;
            _hud?.ShowBanner(banner, 5f);
        }
    }
}
