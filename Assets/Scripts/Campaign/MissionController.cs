using UnityEngine;

namespace Riftwake
{
    /// <summary>Runs any campaign mission inside the Mission scene.</summary>
    public sealed class MissionController : MonoBehaviour
    {
        MissionDef _def;
        GameplayHUD _hud;
        int _kills;
        int _aliveEnemies;
        bool _complete;
        bool _shopBought;
        float _bannerGate;

        public void RegisterKill(EnemyKind kind)
        {
            _kills++;
            _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
            TryComplete();
        }

        void Awake()
        {
            var session = GameSession.Ensure();
            Progression.Ensure();

            if (!session.HasSelectedPath)
                session.SelectPathInPlace(HeroPath.Steelhand);

            _def = CampaignCatalog.Get(session.CurrentMission);
            BuildWorld();
        }

        void BuildWorld()
        {
            var session = GameSession.Instance;
            var prog = Progression.Instance;

            BuildLighting();
            BuildGround(ParseColor(_def.GroundHex), _def.GroundScale);
            BuildProps();

            var mats = CreateMats();
            var player = SpawnPlayer(session.SelectedPath, mats, prog);
            BuildCamera(player.transform);

            _hud = new GameObject("HUD").AddComponent<GameplayHUD>();
            _hud.Bind(
                player.GetComponent<Health>(),
                player.GetComponent<FocusResource>(),
                player.GetComponent<PlayerCombat>(),
                player.GetComponent<PlayerMotor>(),
                session.SelectedPath);
            _hud.SetMissionTitle($"{_def.Title} — {_def.Subtitle}");

            if (_def.SpawnElder)
                SpawnElder(player.transform, mats);

            if (_def.HealOnEnter)
                player.GetComponent<Health>()?.Configure(
                    100f + prog.VitalityBonus);

            foreach (var spawn in _def.Spawns)
                SpawnEnemy(spawn, player.transform, mats);

            _aliveEnemies = _def.Spawns.Length;
            _hud.ShowBanner(_def.ArrivalText, 5f);
            _bannerGate = 2f;

            if (_def.IsHub)
                _hud.ShowBanner(_def.HubPrompt, 8f);

            session.Persist();
        }

        void Update()
        {
            if (_bannerGate > 0f)
                _bannerGate -= Time.deltaTime;

            if (_complete)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    GameSession.Instance.CompleteCurrentMission();
                return;
            }

            if (_def.IsHub)
            {
                if (_def.ShopAvailable && !_shopBought && Input.GetKeyDown(KeyCode.E))
                {
                    var prog = Progression.Instance;
                    if (prog.TryBuyShop(_def.ShopCost, _def.ShopVitalityBonus, _def.ShopDamageBonus))
                    {
                        _shopBought = true;
                        var player = GameObject.FindGameObjectWithTag("Player");
                        var hp = player != null ? player.GetComponent<Health>() : null;
                        hp?.Configure(100f + prog.VitalityBonus);
                        var combat = player != null ? player.GetComponent<PlayerCombat>() : null;
                        combat?.SetDamageBonus(prog.DamageBonus);
                        _hud.ShowBanner("Field Charm purchased. Vitality and damage improved.", 3f);
                        GameSession.Instance.Persist();
                    }
                    else
                    {
                        _hud.ShowBanner("Not enough gold for a Field Charm.", 2f);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    CompleteMission();
            }
        }

        void TryComplete()
        {
            if (_complete || _def.IsHub)
                return;

            bool cleared;
            if (_def.KillTarget > 0)
                cleared = _kills >= _def.KillTarget;
            else
                cleared = _aliveEnemies <= 0;

            if (cleared)
                CompleteMission();
        }

        void CompleteMission()
        {
            if (_complete)
                return;
            _complete = true;
            GameSession.Instance.Persist();
            _hud.ShowBanner(_def.CompleteText + "  [Enter] continue", 99f);
        }

        static Color ParseColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c))
                return c;
            return new Color(0.17f, 0.2f, 0.13f);
        }

        struct Mats
        {
            public Material Projectile;
            public Material Melee;
            public Material Enemy;
        }

        static Mats CreateMats() => new Mats
        {
            Projectile = PrimitiveFactory.MakeColorMat(new Color(1f, 0.5f, 0.2f)),
            Melee = PrimitiveFactory.MakeColorMat(new Color(0.95f, 0.9f, 0.55f, 0.45f), true),
            Enemy = PrimitiveFactory.MakeColorMat(new Color(0.55f, 0.15f, 0.12f))
        };

        static void BuildLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.2f, 0.25f);
            if (Object.FindObjectOfType<Light>() == null)
            {
                var sun = new GameObject("Sun");
                var light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.78f, 0.55f);
                light.intensity = 1.05f;
                light.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(42f, 35f, 0f);
            }
        }

        static void BuildGround(Color color, float scale)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(scale, 1f, scale);
            ground.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveFactory.MakeColorMat(color);

            var rim = GameObject.CreatePrimitive(PrimitiveType.Plane);
            rim.name = "Rim";
            rim.transform.position = new Vector3(0f, -0.05f, 0f);
            rim.transform.localScale = new Vector3(scale * 1.6f, 1f, scale * 1.6f);
            rim.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(color.r * 0.45f, color.g * 0.45f, color.b * 0.45f));
        }

        static void BuildProps()
        {
            PlaceTree(new Vector3(-10f, 0f, 8f));
            PlaceTree(new Vector3(-12f, 0f, -4f));
            PlaceTree(new Vector3(11f, 0f, 6f));
            PlaceTree(new Vector3(9f, 0f, -9f));
            PlaceTree(new Vector3(-3f, 0f, 12f));
            PlaceTree(new Vector3(14f, 0f, -2f));
            PlaceStone(new Vector3(3.5f, 0.35f, 2.2f), new Vector3(1.4f, 0.7f, 1.1f));
            PlaceStone(new Vector3(-4.5f, 0.25f, -1.5f), new Vector3(1.1f, 0.5f, 1.3f));
        }

        static void PlaceTree(Vector3 pos)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.position = pos + Vector3.up * 1.2f;
            trunk.transform.localScale = new Vector3(0.55f, 1.2f, 0.55f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.28f, 0.18f, 0.1f));

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.transform.position = pos + Vector3.up * 3.1f;
            canopy.transform.localScale = new Vector3(2.4f, 2.0f, 2.4f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.1f, 0.28f, 0.12f));
        }

        static void PlaceStone(Vector3 pos, Vector3 scale)
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.position = pos;
            stone.transform.localScale = scale;
            stone.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.36f, 0.35f, 0.33f));
        }

        static GameObject SpawnPlayer(HeroPath path, Mats mats, Progression prog)
        {
            Color tint;
            switch (path)
            {
                case HeroPath.Steelhand: tint = new Color(0.55f, 0.58f, 0.65f); break;
                case HeroPath.Wildbinder: tint = new Color(0.3f, 0.62f, 0.35f); break;
                default: tint = new Color(0.45f, 0.4f, 0.85f); break;
            }

            var player = PrimitiveFactory.CapsuleActor("Riftborn", tint, new Vector3(0f, 0f, -2f));
            player.tag = "Player";
            PrimitiveFactory.AddCharacterController(player);

            float maxVit = 100f + prog.VitalityBonus;
            var health = player.AddComponent<Health>();
            health.Configure(maxVit);

            var focus = player.AddComponent<FocusResource>();
            float focusMax = path == HeroPath.Spellwright ? 100f : path == HeroPath.Wildbinder ? 60f : 40f;
            focus.Configure(focusMax, path == HeroPath.Spellwright ? 10f : 6f);

            var motor = player.AddComponent<PlayerMotor>();
            motor.Configure(6.5f, null);

            var combat = player.AddComponent<PlayerCombat>();
            combat.Configure(path, motor, focus, mats.Projectile, mats.Melee, prog.DamageBonus);
            return player;
        }

        static void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            GameObject camGo;
            if (cam == null)
            {
                camGo = new GameObject("IsometricCamera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            else camGo = cam.gameObject;

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
            cam.fieldOfView = 40f;

            var follow = camGo.GetComponent<IsometricCameraFollow>() ?? camGo.AddComponent<IsometricCameraFollow>();
            follow.Configure(target, new Vector3(0f, 15f, -12f));
        }

        void SpawnElder(Transform player, Mats mats)
        {
            var elder = PrimitiveFactory.CapsuleActor("ElderBram", new Color(0.7f, 0.58f, 0.38f),
                new Vector3(_def.ElderX, 0f, _def.ElderZ), 1.7f);
            var tutorial = gameObject.AddComponent<TutorialDirector>();
            tutorial.Bind(_hud, player.GetComponent<PlayerMotor>());
            elder.AddComponent<ElderBram>().Configure(player, tutorial);
        }

        void SpawnEnemy(EnemySpawnDef spawn, Transform player, Mats mats)
        {
            var profile = EnemyStats.For(spawn.Kind);
            var root = new GameObject(profile.Name);
            root.transform.position = new Vector3(spawn.X, 0f, spawn.Z);
            root.tag = "Enemy";

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.55f * profile.Scale, 0f);
            body.transform.localScale = new Vector3(1f, 0.75f, 1.15f) * profile.Scale;
            body.GetComponent<MeshRenderer>().sharedMaterial = PrimitiveFactory.MakeColorMat(profile.Color);
            Object.Destroy(body.GetComponent<Collider>());

            if (profile.IsBoss)
            {
                var aura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                aura.transform.SetParent(root.transform, false);
                aura.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                aura.transform.localScale = new Vector3(2.8f, 0.05f, 2.8f);
                aura.GetComponent<MeshRenderer>().sharedMaterial =
                    PrimitiveFactory.MakeColorMat(new Color(0.6f, 0.2f, 0.9f, 0.35f), true);
                Object.Destroy(aura.GetComponent<Collider>());
            }

            PrimitiveFactory.AddCharacterController(root, height: 1.1f * profile.Scale, radius: 0.45f * profile.Scale);
            var health = root.AddComponent<Health>();
            health.Configure(profile.Vitality);
            root.AddComponent<EnemyAI>().Configure(spawn.Kind, player);
        }
    }
}
