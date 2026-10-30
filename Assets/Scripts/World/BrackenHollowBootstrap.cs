using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// Spawns the Bracken Hollow tutorial clearing: ground, mentor, player, Ashmites, HUD, camera.
    /// </summary>
    public sealed class BrackenHollowBootstrap : MonoBehaviour
    {
        void Awake()
        {
            var session = GameSession.Ensure();
            if (!session.HasSelectedPath)
            {
                // Direct play from this scene: default Steelhand so the slice is still playable.
                session.ChoosePath(HeroPath.Steelhand);
            }

            BuildLighting();
            BuildGround();
            BuildProps();

            var mats = CreateSharedMats();
            var player = SpawnPlayer(session.SelectedPath, mats);
            var cam = BuildCamera(player.transform);

            var tutorialGo = new GameObject("TutorialDirector");
            var tutorial = tutorialGo.AddComponent<TutorialDirector>();

            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<GameplayHUD>();
            hud.Bind(
                player.GetComponent<Health>(),
                player.GetComponent<FocusResource>(),
                player.GetComponent<PlayerCombat>(),
                player.GetComponent<PlayerMotor>(),
                session.SelectedPath);

            tutorial.Bind(hud, player.GetComponent<PlayerMotor>());

            SpawnElder(player.transform, tutorial, mats);
            SpawnAshmites(player.transform, tutorial, mats);

            hud.ShowBanner(
                $"You wake as a {GameSession.PathDisplayName(session.SelectedPath)}. The Verge does not wait.",
                4f);
        }

        struct SharedMats
        {
            public Material Ground;
            public Material Player;
            public Material Elder;
            public Material Enemy;
            public Material Projectile;
            public Material Melee;
            public Material Tree;
            public Material Stone;
        }

        static SharedMats CreateSharedMats()
        {
            return new SharedMats
            {
                Ground = PrimitiveFactory.MakeColorMat(new Color(0.18f, 0.22f, 0.14f)),
                Player = PrimitiveFactory.MakeColorMat(new Color(0.35f, 0.55f, 0.85f)),
                Elder = PrimitiveFactory.MakeColorMat(new Color(0.65f, 0.55f, 0.35f)),
                Enemy = PrimitiveFactory.MakeColorMat(new Color(0.55f, 0.15f, 0.12f)),
                Projectile = PrimitiveFactory.MakeColorMat(new Color(1f, 0.5f, 0.2f)),
                Melee = PrimitiveFactory.MakeColorMat(new Color(0.95f, 0.9f, 0.55f, 0.45f), transparent: true),
                Tree = PrimitiveFactory.MakeColorMat(new Color(0.12f, 0.28f, 0.12f)),
                Stone = PrimitiveFactory.MakeColorMat(new Color(0.35f, 0.34f, 0.32f))
            };
        }

        static void BuildLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.2f, 0.25f);

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.78f, 0.55f);
            light.intensity = 1.05f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, 35f, 0f);
        }

        static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "HollowGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.17f, 0.2f, 0.13f));

            // Slight rim of darker terrain for “clearing in the woods” read.
            var rim = GameObject.CreatePrimitive(PrimitiveType.Plane);
            rim.name = "DarkRim";
            rim.transform.position = new Vector3(0f, -0.05f, 0f);
            rim.transform.localScale = new Vector3(10f, 1f, 10f);
            rim.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.08f, 0.09f, 0.07f));
        }

        static void BuildProps()
        {
            PlaceTree(new Vector3(-10f, 0f, 8f));
            PlaceTree(new Vector3(-12f, 0f, -4f));
            PlaceTree(new Vector3(11f, 0f, 6f));
            PlaceTree(new Vector3(9f, 0f, -9f));
            PlaceTree(new Vector3(-3f, 0f, 12f));
            PlaceTree(new Vector3(4f, 0f, 13f));
            PlaceTree(new Vector3(-14f, 0f, 2f));
            PlaceTree(new Vector3(14f, 0f, -2f));

            PlaceStone(new Vector3(3.5f, 0.35f, 2.2f), new Vector3(1.4f, 0.7f, 1.1f));
            PlaceStone(new Vector3(-4.5f, 0.25f, -1.5f), new Vector3(1.1f, 0.5f, 1.3f));
            PlaceStone(new Vector3(0.5f, 0.4f, -5.5f), new Vector3(1.8f, 0.8f, 1.2f));
        }

        static void PlaceTree(Vector3 pos)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.position = pos + Vector3.up * 1.2f;
            trunk.transform.localScale = new Vector3(0.55f, 1.2f, 0.55f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.28f, 0.18f, 0.1f));

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.position = pos + Vector3.up * 3.1f;
            canopy.transform.localScale = new Vector3(2.4f, 2.0f, 2.4f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.1f, 0.28f, 0.12f));
        }

        static void PlaceStone(Vector3 pos, Vector3 scale)
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "Stone";
            stone.transform.position = pos;
            stone.transform.localScale = scale;
            stone.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            stone.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.36f, 0.35f, 0.33f));
        }

        static GameObject SpawnPlayer(HeroPath path, SharedMats mats)
        {
            Color tint;
            switch (path)
            {
                case HeroPath.Steelhand:
                    tint = new Color(0.55f, 0.58f, 0.65f);
                    break;
                case HeroPath.Wildbinder:
                    tint = new Color(0.3f, 0.62f, 0.35f);
                    break;
                default:
                    tint = new Color(0.45f, 0.4f, 0.85f);
                    break;
            }

            var player = PrimitiveFactory.CapsuleActor("Riftborn", tint, new Vector3(0f, 0f, -2f));
            player.tag = "Player";
            PrimitiveFactory.AddCharacterController(player);

            var health = player.AddComponent<Health>();
            health.Configure(100f);

            var focus = player.AddComponent<FocusResource>();
            float focusMax = path == HeroPath.Spellwright ? 100f : path == HeroPath.Wildbinder ? 60f : 40f;
            float regen = path == HeroPath.Spellwright ? 10f : 6f;
            focus.Configure(focusMax, regen);

            var motor = player.AddComponent<PlayerMotor>();
            motor.Configure(6.5f, null);

            var combat = player.AddComponent<PlayerCombat>();
            combat.Configure(path, motor, focus, mats.Projectile, mats.Melee);

            return player;
        }

        static Camera BuildCamera(Transform target)
        {
            var camGo = new GameObject("IsometricCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
            cam.fieldOfView = 40f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 120f;
            camGo.AddComponent<AudioListener>();

            var follow = camGo.AddComponent<IsometricCameraFollow>();
            follow.Configure(target, new Vector3(0f, 15f, -12f));
            return cam;
        }

        static void SpawnElder(Transform player, TutorialDirector tutorial, SharedMats mats)
        {
            var elder = PrimitiveFactory.CapsuleActor("ElderBram", new Color(0.7f, 0.58f, 0.38f), new Vector3(-1.5f, 0f, 3.5f), 1.7f);
            // Staff
            var staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            staff.name = "Staff";
            staff.transform.SetParent(elder.transform, false);
            staff.transform.localPosition = new Vector3(0.55f, 1.1f, 0f);
            staff.transform.localScale = new Vector3(0.08f, 1.1f, 0.08f);
            staff.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.4f, 0.28f, 0.15f));
            Object.Destroy(staff.GetComponent<Collider>());

            var mentor = elder.AddComponent<ElderBram>();
            mentor.Configure(player, tutorial);
        }

        static void SpawnAshmites(Transform player, TutorialDirector tutorial, SharedMats mats)
        {
            Vector3[] spots =
            {
                new Vector3(8f, 0f, 4f),
                new Vector3(10f, 0f, -3f),
                new Vector3(-9f, 0f, 5f),
                new Vector3(-7f, 0f, -6f),
                new Vector3(6f, 0f, 9f)
            };

            for (int i = 0; i < spots.Length; i++)
                SpawnAshmite($"Ashmite_{i + 1}", spots[i], player, tutorial, mats);
        }

        static void SpawnAshmite(string name, Vector3 pos, Transform player, TutorialDirector tutorial, SharedMats mats)
        {
            var root = new GameObject(name);
            root.transform.position = pos;
            root.tag = "Enemy";

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.transform.localScale = new Vector3(1.0f, 0.75f, 1.15f);
            body.GetComponent<MeshRenderer>().sharedMaterial = mats.Enemy;
            Object.Destroy(body.GetComponent<Collider>());

            var crest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crest.name = "Crest";
            crest.transform.SetParent(root.transform, false);
            crest.transform.localPosition = new Vector3(0f, 0.95f, 0.2f);
            crest.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            crest.GetComponent<MeshRenderer>().sharedMaterial =
                PrimitiveFactory.MakeColorMat(new Color(0.85f, 0.25f, 0.15f));
            Object.Destroy(crest.GetComponent<Collider>());

            PrimitiveFactory.AddCharacterController(root, height: 1.1f, radius: 0.45f);

            var health = root.AddComponent<Health>();
            health.Configure(45f);
            health.Died += _ => tutorial.RegisterKill();

            var ai = root.AddComponent<AshmiteAI>();
            ai.Configure(player, speed: 3.7f, detect: 11f, atkRange: 1.3f, damage: 9f);
        }
    }
}
