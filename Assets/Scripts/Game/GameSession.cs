using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riftwake
{
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public HeroPath SelectedPath { get; private set; } = HeroPath.Steelhand;
        public bool HasSelectedPath { get; private set; }
        public MissionId CurrentMission { get; private set; } = MissionId.BrackenHollow;
        public bool CampaignComplete { get; set; }

        public const string MainMenuScene = "MainMenu";
        public const string ClassSelectScene = "ClassSelect";
        public const string MissionScene = "Mission";
        public const string EndingScene = "Ending";

        // Legacy aliases so older scenes still resolve conceptually.
        public const string BrackenHollowScene = MissionScene;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Progression.Ensure();
        }

        public static GameSession Ensure()
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject("GameSession");
            return go.AddComponent<GameSession>();
        }

        public void StartNewGame()
        {
            CampaignComplete = false;
            HasSelectedPath = false;
            CurrentMission = MissionId.BrackenHollow;
            Progression.Ensure().ResetNewGame();
            SceneManager.LoadScene(ClassSelectScene);
        }

        public void ContinueFromSave()
        {
            var data = SaveSystem.Read();
            if (data == null || !data.HasSave)
            {
                StartNewGame();
                return;
            }

            SelectedPath = (HeroPath)data.Path;
            HasSelectedPath = true;
            CurrentMission = (MissionId)data.Mission;
            CampaignComplete = data.CampaignComplete;
            Progression.Ensure().LoadFrom(data);

            if (CampaignComplete)
                SceneManager.LoadScene(EndingScene);
            else
                SceneManager.LoadScene(MissionScene);
        }

        public void SelectPathInPlace(HeroPath path)
        {
            SelectedPath = path;
            HasSelectedPath = true;
            if (Progression.Instance == null || Progression.Instance.Level < 1)
                Progression.Ensure().ResetNewGame();
        }

        public void ChoosePath(HeroPath path)
        {
            SelectPathInPlace(path);
            CurrentMission = MissionId.BrackenHollow;
            Progression.Ensure().ResetNewGame();
            Persist();
            SceneManager.LoadScene(MissionScene);
        }

        public void SetMission(MissionId id)
        {
            CurrentMission = id;
            Persist();
        }

        public void CompleteCurrentMission()
        {
            var next = CampaignCatalog.Next(CurrentMission);
            if (next == null)
            {
                CampaignComplete = true;
                Persist();
                SceneManager.LoadScene(EndingScene);
                return;
            }

            CurrentMission = next.Value;
            Persist();
            SceneManager.LoadScene(MissionScene);
        }

        public void Persist()
        {
            var prog = Progression.Ensure();
            var data = new SaveData
            {
                Path = (int)SelectedPath,
                Mission = (int)CurrentMission,
                CampaignComplete = CampaignComplete,
                HasSave = true
            };
            prog.WriteInto(data);
            SaveSystem.Write(data);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene(MainMenuScene);
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
                    return "Close-quarters fighter. Cleaving Strike, later Iron Guard burst.";
                case HeroPath.Wildbinder:
                    return "Nature-touched hybrid. Thorn Dart, later Vine Snare slow.";
                case HeroPath.Spellwright:
                    return "Focused caster. Ember Bolt, later Flashflare nova.";
                default:
                    return string.Empty;
            }
        }
    }
}
