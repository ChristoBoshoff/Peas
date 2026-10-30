using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// Survives scene loads. Holds the chosen hero path for the first playthrough slice.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public HeroPath SelectedPath { get; private set; } = HeroPath.Steelhand;
        public bool HasSelectedPath { get; private set; }

        public const string ClassSelectScene = "ClassSelect";
        public const string BrackenHollowScene = "BrackenHollow";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ChoosePath(HeroPath path)
        {
            SelectedPath = path;
            HasSelectedPath = true;
        }

        public static GameSession Ensure()
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject("GameSession");
            return go.AddComponent<GameSession>();
        }

        public static string PathDisplayName(HeroPath path)
        {
            switch (path)
            {
                case HeroPath.Steelhand: return "Steelhand";
                case HeroPath.Wildbinder: return "Wildbinder";
                case HeroPath.Spellwright: return "Spellwright";
                default: return path.ToString();
            }
        }

        public static string PathBlurb(HeroPath path)
        {
            switch (path)
            {
                case HeroPath.Steelhand:
                    return "Close-quarters fighter. Cleaving Strike answers anything that steps near.";
                case HeroPath.Wildbinder:
                    return "Nature-touched hybrid. Thorn Dart keeps foes at bay while you learn the Verge.";
                case HeroPath.Spellwright:
                    return "Focused caster. Ember Bolt spends Focus to strike from range.";
                default:
                    return string.Empty;
            }
        }
    }
}
