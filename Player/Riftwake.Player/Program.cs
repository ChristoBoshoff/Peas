using System.Numerics;
using System.Text.Json;
using Raylib_cs;

namespace Riftwake.Player;

static class Program
{
    const int W = 1280;
    const int H = 720;

    static Screen _screen = Screen.MainMenu;
    static HeroPath _path = HeroPath.Steelhand;
    static HeroPath _highlight = HeroPath.Steelhand;
    static MissionId _mission = MissionId.BrackenHollow;
    static bool _campaignComplete;

    static int _level = 1, _xp, _gold = 15;
    static float _vitBonus, _dmgBonus;
    static bool _secondaryUnlocked;

    static readonly List<Enemy> Enemies = new();
    static readonly List<Shot> Shots = new();
    static readonly List<Burst> Bursts = new();

    static Vector2 _player;
    static Vector2 _aim = new(0, -1);
    static float _hp = 100, _maxHp = 100;
    static float _focus = 100, _maxFocus = 100;
    static float _cooldown, _secondaryCd;
    static float _bannerT;
    static string _banner = "";
    static string _status = "Choose your path, Riftborn.";
    static bool _dead, _missionDone, _shopBought;
    static int _kills, _killTarget;
    static MissionDef? _def;
    static float _focusRegen = 8f;
    static bool _elder;

    static string SavePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Riftwake", "riftwake_save.json");

    static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.VSyncHint);
        Raylib.InitWindow(W, H, "Riftwake");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            Update(dt);
            Draw();
        }

        Raylib.CloseWindow();
    }

    static void Update(float dt)
    {
        if (_bannerT > 0) _bannerT -= dt;
        switch (_screen)
        {
            case Screen.MainMenu: UpdateMenu(); break;
            case Screen.ClassSelect: UpdateClassSelect(); break;
            case Screen.Mission: UpdateMission(dt); break;
            case Screen.Ending: UpdateEnding(); break;
        }
    }

    static void UpdateMenu()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One) || Raylib.IsKeyPressed(KeyboardKey.N) || Raylib.IsKeyPressed(KeyboardKey.Enter))
            StartNew();
        if (Raylib.IsKeyPressed(KeyboardKey.Two) || Raylib.IsKeyPressed(KeyboardKey.C))
            ContinueSave();
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.Q))
            Raylib.CloseWindow();

        var mouse = Raylib.GetMousePosition();
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (HitBtn(mouse, 0)) StartNew();
            else if (HitBtn(mouse, 1) && File.Exists(SavePath)) ContinueSave();
            else if (HitBtn(mouse, 2)) Raylib.CloseWindow();
        }
    }

    static bool HitBtn(Vector2 mouse, int index)
    {
        float bw = 360, bh = 48;
        float bx = (W - bw) * 0.5f;
        float by = H * 0.42f + index * 60;
        return mouse.X >= bx && mouse.X <= bx + bw && mouse.Y >= by && mouse.Y <= by + bh;
    }

    static void UpdateClassSelect()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One)) { _highlight = HeroPath.Steelhand; _status = "Path: Steelhand"; }
        if (Raylib.IsKeyPressed(KeyboardKey.Two)) { _highlight = HeroPath.Wildbinder; _status = "Path: Wildbinder"; }
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) { _highlight = HeroPath.Spellwright; _status = "Path: Spellwright"; }
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
            BeginCampaign(_highlight);

        var mouse = Raylib.GetMousePosition();
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            for (int i = 0; i < 3; i++)
            {
                float cw = W * 0.26f, ch = H * 0.38f;
                float cx = W * (0.08f + i * 0.29f), cy = H * 0.32f;
                if (mouse.X >= cx && mouse.X <= cx + cw && mouse.Y >= cy && mouse.Y <= cy + ch)
                    _highlight = (HeroPath)i;
                // begin button
            }
            float bw = W * 0.3f, bh = 48, bx = W * 0.35f, by = H * 0.82f;
            if (mouse.X >= bx && mouse.X <= bx + bw && mouse.Y >= by && mouse.Y <= by + bh)
                BeginCampaign(_highlight);
        }
    }

    static void UpdateEnding()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Escape))
            _screen = Screen.MainMenu;
        if (Raylib.IsKeyPressed(KeyboardKey.N))
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            StartNew();
        }
    }

    static void UpdateMission(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            Persist();
            _screen = Screen.MainMenu;
            return;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.F5))
        {
            Persist();
            Banner("Campaign saved.", 1.5f);
        }

        if (_dead)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                _dead = false;
                _hp = _maxHp;
                Banner("You rise again.", 2f);
            }
            return;
        }

        if (_missionDone)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
                AdvanceMission();
            return;
        }

        var def = _def!;
        if (def.IsHub)
        {
            if (def.Shop && !_shopBought && Raylib.IsKeyPressed(KeyboardKey.E))
            {
                if (_gold >= def.ShopCost)
                {
                    _gold -= def.ShopCost;
                    _vitBonus += 25;
                    _dmgBonus += 6;
                    _maxHp = 100 + _vitBonus;
                    _hp = _maxHp;
                    _shopBought = true;
                    Banner("Field Charm purchased.", 3f);
                    Persist();
                }
                else Banner("Not enough gold.", 2f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
                CompleteMission();
            // still allow movement in hub
        }

        // Movement
        var move = Vector2.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up)) move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down)) move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) move.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) move.X += 1;
        if (move.LengthSquared() > 0) move = Vector2.Normalize(move);
        _player += move * 220f * dt;
        _player.X = Math.Clamp(_player.X, -420, 420);
        _player.Y = Math.Clamp(_player.Y, -320, 320);

        // Aim toward mouse in world
        var mouse = Raylib.GetMousePosition();
        var worldMouse = ScreenToWorld(mouse);
        var aim = worldMouse - _player;
        if (aim.LengthSquared() > 1f) _aim = Vector2.Normalize(aim);

        // Focus regen
        _focus = Math.Min(_maxFocus, _focus + _focusRegen * dt);
        if (_cooldown > 0) _cooldown -= dt;
        if (_secondaryCd > 0) _secondaryCd -= dt;

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && _cooldown <= 0)
            FirePrimary();
        if (_secondaryUnlocked && Raylib.IsKeyPressed(KeyboardKey.Q) && _secondaryCd <= 0)
            FireSecondary();

        // Shots
        for (int i = Shots.Count - 1; i >= 0; i--)
        {
            var s = Shots[i];
            s.Pos += s.Vel * dt;
            s.Life -= dt;
            bool hit = false;
            foreach (var e in Enemies)
            {
                if (!e.Alive) continue;
                if (Vector2.Distance(s.Pos, e.Pos) < e.Profile.Radius + 8)
                {
                    e.Hp -= s.Damage;
                    if (s.Slow) { e.SlowT = 2.5f; e.SlowF = 0.45f; }
                    hit = true;
                    if (e.Hp <= 0) KillEnemy(e);
                    break;
                }
            }
            if (hit || s.Life <= 0) Shots.RemoveAt(i);
            else Shots[i] = s;
        }

        // Bursts
        for (int i = Bursts.Count - 1; i >= 0; i--)
        {
            var b = Bursts[i];
            b.Life -= dt;
            if (!b.Applied)
            {
                foreach (var e in Enemies)
                {
                    if (!e.Alive) continue;
                    if (Vector2.Distance(b.Pos, e.Pos) <= b.Radius)
                    {
                        e.Hp -= b.Damage;
                        if (e.Hp <= 0) KillEnemy(e);
                    }
                }
                b.Applied = true;
            }
            if (b.Life <= 0) Bursts.RemoveAt(i);
            else Bursts[i] = b;
        }

        // Enemies
        foreach (var e in Enemies)
        {
            if (!e.Alive) continue;
            if (e.SlowT > 0) { e.SlowT -= dt; if (e.SlowT <= 0) e.SlowF = 1f; }
            if (e.Cd > 0) e.Cd -= dt;

            var to = _player - e.Pos;
            float dist = to.Length();
            if (!e.Aggro && dist <= e.Profile.Detect) e.Aggro = true;
            if (!e.Aggro) continue;

            if (dist > e.Profile.Range)
            {
                var dir = Vector2.Normalize(to);
                e.Pos += dir * e.Profile.Speed * e.SlowF * dt;
            }
            else if (e.Cd <= 0)
            {
                _hp -= e.Profile.Damage;
                e.Cd = e.Profile.Cooldown;
                if (_hp <= 0)
                {
                    _hp = 0;
                    _dead = true;
                    Banner("You fall. Press R to rise, or Esc for menu.", 5f);
                }
            }
        }

        if (!def.IsHub && !_missionDone)
        {
            bool cleared = def.KillTarget > 0
                ? _kills >= def.KillTarget
                : Enemies.TrueForAll(e => !e.Alive);
            // For RiftHeart KillTarget=1 means kill the wraith specifically counted in kills of any - actually killtarget 1 with wraith - better check wraith dead
            if (def.Id == MissionId.RiftHeart)
                cleared = Enemies.Exists(e => e.Kind == EnemyKind.RiftWraith && !e.Alive);
            else if (def.KillTarget > 0)
                cleared = _kills >= def.KillTarget;
            else
                cleared = Enemies.Count == 0 || Enemies.TrueForAll(e => !e.Alive);

            if (cleared) CompleteMission();
        }
    }

    static void FirePrimary()
    {
        float dmg = _dmgBonus;
        switch (_path)
        {
            case HeroPath.Steelhand:
                SpawnBurst(_player + _aim * 36, 52, 28 + dmg, 0.18f, new Color(230, 210, 120, 160));
                _cooldown = 0.45f;
                break;
            case HeroPath.Wildbinder:
                Shots.Add(new Shot(_player + _aim * 20, _aim * 420, 18 + dmg, 2.2f, new Color(90, 210, 90, 255), false));
                _cooldown = 0.4f;
                break;
            case HeroPath.Spellwright:
                if (_focus < 18) return;
                _focus -= 18;
                Shots.Add(new Shot(_player + _aim * 20, _aim * 480, 32 + dmg, 2.2f, new Color(255, 120, 40, 255), false));
                _cooldown = 0.55f;
                break;
        }
    }

    static void FireSecondary()
    {
        float dmg = _dmgBonus;
        switch (_path)
        {
            case HeroPath.Steelhand:
                _hp = Math.Min(_maxHp, _hp + 18 + dmg);
                SpawnBurst(_player, 70, 22 + dmg, 0.22f, new Color(160, 190, 255, 140));
                _secondaryCd = 6f;
                break;
            case HeroPath.Wildbinder:
                if (_focus < 12) return;
                _focus -= 12;
                Shots.Add(new Shot(_player + _aim * 20, _aim * 380, 14 + dmg, 2.2f, new Color(120, 240, 140, 255), true));
                _secondaryCd = 5f;
                break;
            case HeroPath.Spellwright:
                if (_focus < 28) return;
                _focus -= 28;
                SpawnBurst(_player, 110, 26 + dmg, 0.2f, new Color(255, 180, 50, 120));
                _secondaryCd = 7f;
                break;
        }
    }

    static void SpawnBurst(Vector2 pos, float radius, float damage, float life, Color color)
    {
        Bursts.Add(new Burst(pos, radius, damage, life, color, false));
    }

    static void KillEnemy(Enemy e)
    {
        if (!e.Alive) return;
        e.Alive = false;
        _kills++;
        _xp += e.Profile.Xp;
        _gold += e.Profile.Gold;
        while (_xp >= XpToNext(_level))
        {
            _xp -= XpToNext(_level);
            _level++;
            _vitBonus += 12;
            _dmgBonus += 3;
            if (_level >= 3) _secondaryUnlocked = true;
            _maxHp = 100 + _vitBonus;
            _hp = Math.Min(_maxHp, _hp + 20);
            Banner($"Level {_level}!", 2f);
        }
    }

    static int XpToNext(int level) => 40 + (level - 1) * 30;

    static void CompleteMission()
    {
        _missionDone = true;
        Persist();
        Banner((_def?.Complete ?? "Done.") + "  [Enter] continue", 99f);
    }

    static void AdvanceMission()
    {
        var next = Campaign.Next(_mission);
        if (next == null)
        {
            _campaignComplete = true;
            Persist();
            _screen = Screen.Ending;
            return;
        }
        _mission = next.Value;
        Persist();
        LoadMission();
    }

    static void StartNew()
    {
        _screen = Screen.ClassSelect;
        _status = "Choose your path, Riftborn.";
        _highlight = HeroPath.Steelhand;
        _campaignComplete = false;
    }

    static void BeginCampaign(HeroPath path)
    {
        _path = path;
        _mission = MissionId.BrackenHollow;
        _level = 1; _xp = 0; _gold = 15;
        _vitBonus = 0; _dmgBonus = 0; _secondaryUnlocked = false;
        _campaignComplete = false;
        ConfigureFocus();
        Persist();
        LoadMission();
        _screen = Screen.Mission;
    }

    static void ConfigureFocus()
    {
        _maxFocus = _path switch
        {
            HeroPath.Spellwright => 100,
            HeroPath.Wildbinder => 60,
            _ => 40
        };
        _focus = _maxFocus;
        _focusRegen = _path == HeroPath.Spellwright ? 10 : 6;
    }

    static void ContinueSave()
    {
        var data = ReadSave();
        if (data == null || !data.HasSave) { StartNew(); return; }
        _path = (HeroPath)data.Path;
        _mission = (MissionId)data.Mission;
        _level = Math.Max(1, data.Level);
        _xp = data.Xp;
        _gold = data.Gold;
        _vitBonus = data.VitalityBonus;
        _dmgBonus = data.DamageBonus;
        _secondaryUnlocked = data.SecondaryUnlocked;
        _campaignComplete = data.CampaignComplete;
        ConfigureFocus();
        if (_campaignComplete) { _screen = Screen.Ending; return; }
        LoadMission();
        _screen = Screen.Mission;
    }

    static void LoadMission()
    {
        _def = Campaign.Get(_mission);
        Enemies.Clear(); Shots.Clear(); Bursts.Clear();
        _player = new Vector2(0, 80);
        _aim = new Vector2(0, -1);
        _maxHp = 100 + _vitBonus;
        _hp = _maxHp;
        _focus = _maxFocus;
        _cooldown = _secondaryCd = 0;
        _dead = _missionDone = _shopBought = false;
        _kills = 0;
        _killTarget = _def.KillTarget;
        _elder = _def.SpawnElder;
        foreach (var s in _def.Spawns)
        {
            var p = EnemyDb.For(s.Kind);
            Enemies.Add(new Enemy
            {
                Kind = s.Kind,
                Pos = new Vector2(s.X, s.Y),
                Hp = p.Hp,
                Profile = p,
                Alive = true,
                SlowF = 1f
            });
        }
        if (_def.HealOnEnter) _hp = _maxHp;
        Banner(_def.IsHub ? _def.HubPrompt : _def.Arrival, _def.IsHub ? 8 : 5);
    }

    static void Banner(string text, float seconds)
    {
        _banner = text;
        _bannerT = seconds;
    }

    static void Persist()
    {
        var data = new SaveData
        {
            Path = (int)_path,
            Mission = (int)_mission,
            Level = _level,
            Xp = _xp,
            Gold = _gold,
            VitalityBonus = _vitBonus,
            DamageBonus = _dmgBonus,
            SecondaryUnlocked = _secondaryUnlocked,
            CampaignComplete = _campaignComplete,
            HasSave = true
        };
        var dir = Path.GetDirectoryName(SavePath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(SavePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    static SaveData? ReadSave()
    {
        try
        {
            if (!File.Exists(SavePath)) return null;
            return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(SavePath));
        }
        catch { return null; }
    }

    static Vector2 WorldToScreen(Vector2 world)
    {
        // Isometric-ish: camera follows player, y compressed
        float sx = W * 0.5f + (world.X - _player.X);
        float sy = H * 0.55f + (world.Y - _player.Y) * 0.75f;
        return new Vector2(sx, sy);
    }

    static Vector2 ScreenToWorld(Vector2 screen)
    {
        float wx = screen.X - W * 0.5f + _player.X;
        float wy = (screen.Y - H * 0.55f) / 0.75f + _player.Y;
        return new Vector2(wx, wy);
    }

    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(12, 14, 18, 255));
        switch (_screen)
        {
            case Screen.MainMenu: DrawMenu(); break;
            case Screen.ClassSelect: DrawClassSelect(); break;
            case Screen.Mission: DrawMission(); break;
            case Screen.Ending: DrawEnding(); break;
        }
        Raylib.EndDrawing();
    }

    static void DrawMenu()
    {
        Raylib.DrawRectangle(0, (int)(H * 0.6f), W, (int)(H * 0.4f), new Color(50, 28, 20, 110));
        DrawCentered("RIFTWAKE", 48, (int)(H * 0.16f), new Color(235, 220, 175, 255));
        DrawCentered("Full campaign through the Verge — Peas project", 18, (int)(H * 0.26f), new Color(180, 175, 160, 255));
        DrawMenuBtn(0, "New Campaign  [Enter]");
        DrawMenuBtn(1, File.Exists(SavePath) ? "Continue  [C]" : "Continue (no save)");
        DrawMenuBtn(2, "Quit  [Esc]");
        DrawCentered("WASD move · Mouse aim · LMB attack · Q secondary (Lv3) · F5 save", 14, (int)(H * 0.88f), new Color(120, 120, 110, 255));
    }

    static void DrawMenuBtn(int index, string label)
    {
        float bw = 360, bh = 48;
        float bx = (W - bw) * 0.5f;
        float by = H * 0.42f + index * 60;
        bool hover = HitBtn(Raylib.GetMousePosition(), index);
        Raylib.DrawRectangle((int)bx, (int)by, (int)bw, (int)bh, hover ? new Color(70, 55, 35, 255) : new Color(35, 38, 45, 255));
        Raylib.DrawRectangleLines((int)bx, (int)by, (int)bw, (int)bh, new Color(180, 150, 80, 255));
        int tw = Raylib.MeasureText(label, 20);
        Raylib.DrawText(label, (int)(bx + (bw - tw) * 0.5f), (int)(by + 14), 20, new Color(230, 220, 200, 255));
    }

    static void DrawClassSelect()
    {
        DrawCentered("RIFTWAKE", 42, (int)(H * 0.08f), new Color(235, 220, 175, 255));
        DrawCentered("A rift tears open. Choose how you will fight.", 16, (int)(H * 0.16f), new Color(180, 175, 160, 255));
        for (int i = 0; i < 3; i++)
        {
            var path = (HeroPath)i;
            float cw = W * 0.26f, ch = H * 0.38f;
            float cx = W * (0.08f + i * 0.29f), cy = H * 0.32f;
            bool sel = _highlight == path;
            Raylib.DrawRectangle((int)cx, (int)cy, (int)cw, (int)ch, sel ? new Color(70, 55, 35, 240) : new Color(30, 32, 38, 230));
            Raylib.DrawRectangle((int)cx, (int)cy, (int)cw, 3, sel ? new Color(240, 200, 90, 255) : new Color(90, 90, 95, 255));
            string name = Campaign.PathName(path) + $"  [{i + 1}]";
            int tw = Raylib.MeasureText(name, 22);
            Raylib.DrawText(name, (int)(cx + (cw - tw) * 0.5f), (int)(cy + 20), 22, sel ? new Color(255, 230, 150, 255) : Color.RayWhite);
            DrawWrapped(Campaign.PathBlurb(path), (int)(cx + 16), (int)(cy + 70), (int)(cw - 32), 16, new Color(210, 205, 190, 255));
        }
        DrawCentered(_status, 16, (int)(H * 0.74f), new Color(190, 180, 150, 255));
        float bw = W * 0.3f, bh = 48, bx = W * 0.35f, by = H * 0.82f;
        Raylib.DrawRectangle((int)bx, (int)by, (int)bw, (int)bh, new Color(70, 55, 35, 255));
        string bl = "Begin Campaign  [Enter]";
        int btw = Raylib.MeasureText(bl, 20);
        Raylib.DrawText(bl, (int)(bx + (bw - btw) * 0.5f), (int)(by + 14), 20, Color.RayWhite);
    }

    static void DrawMission()
    {
        var def = _def!;
        // ground
        var origin = WorldToScreen(Vector2.Zero);
        Raylib.DrawCircle((int)origin.X, (int)origin.Y, 520, def.Ground);
        Raylib.DrawCircle((int)origin.X, (int)origin.Y, 520, new Color(0, 0, 0, 40));

        // trees / stones decoration
        DrawProp(new Vector2(-320, -200), 18, new Color(30, 70, 35, 255));
        DrawProp(new Vector2(340, -180), 20, new Color(30, 70, 35, 255));
        DrawProp(new Vector2(-300, 220), 16, new Color(30, 70, 35, 255));
        DrawProp(new Vector2(280, 240), 17, new Color(30, 70, 35, 255));

        if (_elder)
        {
            var ep = WorldToScreen(new Vector2(-50, -120));
            Raylib.DrawCircle((int)ep.X, (int)ep.Y, 16, new Color(180, 150, 95, 255));
            Raylib.DrawText("Elder Bram", (int)ep.X - 40, (int)ep.Y - 36, 14, new Color(240, 220, 170, 255));
        }

        foreach (var e in Enemies)
        {
            if (!e.Alive) continue;
            var sp = WorldToScreen(e.Pos);
            if (e.Profile.Boss)
                Raylib.DrawCircle((int)sp.X, (int)sp.Y, (int)(e.Profile.Radius + 10), new Color(120, 40, 180, 80));
            Raylib.DrawCircle((int)sp.X, (int)sp.Y, (int)e.Profile.Radius, e.Profile.Color);
            float pct = Math.Clamp(e.Hp / e.Profile.Hp, 0, 1);
            Raylib.DrawRectangle((int)(sp.X - 18), (int)(sp.Y - e.Profile.Radius - 12), 36, 5, new Color(40, 40, 40, 200));
            Raylib.DrawRectangle((int)(sp.X - 18), (int)(sp.Y - e.Profile.Radius - 12), (int)(36 * pct), 5, new Color(200, 50, 50, 255));
        }

        foreach (var s in Shots)
        {
            var sp = WorldToScreen(s.Pos);
            Raylib.DrawCircle((int)sp.X, (int)sp.Y, 6, s.Color);
        }
        foreach (var b in Bursts)
        {
            var sp = WorldToScreen(b.Pos);
            Raylib.DrawCircle((int)sp.X, (int)sp.Y, b.Radius, b.Color);
        }

        var pp = WorldToScreen(_player);
        Color pc = _path switch
        {
            HeroPath.Steelhand => new Color(140, 148, 165, 255),
            HeroPath.Wildbinder => new Color(75, 160, 90, 255),
            _ => new Color(115, 100, 215, 255)
        };
        Raylib.DrawCircle((int)pp.X, (int)pp.Y, 16, pc);
        var nose = pp + _aim * 18;
        Raylib.DrawCircle((int)nose.X, (int)nose.Y, 5, Color.RayWhite);

        // HUD
        DrawBar(24, 20, 260, 22, "Vitality", _hp / _maxHp, new Color(190, 45, 45, 255));
        if (_path != HeroPath.Steelhand)
            DrawBar(24, 50, 260, 18, "Focus", _focus / _maxFocus, new Color(65, 115, 230, 255));
        int iy = _path == HeroPath.Steelhand ? 50 : 78;
        Raylib.DrawText($"Lv {_level}   XP {_xp}/{XpToNext(_level)}   Gold {_gold}", 24, iy, 16, new Color(220, 210, 180, 255));
        string title = $"{def.Title} — {def.Subtitle}";
        int tw = Raylib.MeasureText(title, 18);
        Raylib.DrawText(title, W - tw - 24, 20, 18, new Color(230, 215, 160, 255));

        string primary = _path switch
        {
            HeroPath.Steelhand => "Cleaving Strike",
            HeroPath.Wildbinder => "Thorn Dart",
            _ => "Ember Bolt"
        };
        string secondary = _path switch
        {
            HeroPath.Steelhand => "Iron Guard",
            HeroPath.Wildbinder => "Vine Snare",
            _ => "Flashflare"
        };
        Raylib.DrawText($"Path: {Campaign.PathName(_path)}  |  LMB: {primary}" +
                        (_secondaryUnlocked ? $"  |  Q: {secondary}" : "  |  Q: (unlock Lv 3)"),
            24, H - 56, 16, new Color(210, 205, 185, 255));
        Raylib.DrawText("WASD move · Mouse aim · F5 save · Esc menu", 24, H - 32, 16, new Color(160, 155, 140, 255));

        if (_bannerT > 0 && !string.IsNullOrEmpty(_banner))
        {
            Raylib.DrawRectangle((int)(W * 0.15f), 88, (int)(W * 0.7f), 64, new Color(20, 20, 25, 210));
            DrawWrapped(_banner, (int)(W * 0.15f) + 16, 100, (int)(W * 0.7f) - 32, 18, new Color(240, 230, 190, 255));
        }

        if (_dead)
        {
            int dw = Raylib.MeasureText("Fallen", 36);
            Raylib.DrawText("Fallen", (W - dw) / 2, H / 2 - 20, 36, new Color(220, 70, 60, 255));
        }
    }

    static void DrawEnding()
    {
        DrawCentered("The Verge Holds", 40, (int)(H * 0.22f), new Color(240, 220, 130, 255));
        DrawWrapped(
            $"As a {Campaign.PathName(_path)}, you closed the Rift Heart and quieted the tear. " +
            "Bracken Hollow still stands. Your campaign is complete — for now.",
            (int)(W * 0.18f), (int)(H * 0.36f), (int)(W * 0.64f), 18, new Color(200, 195, 175, 255));
        DrawCentered("[Enter] Main Menu    [N] New Campaign", 20, (int)(H * 0.65f), new Color(220, 210, 180, 255));
    }

    static void DrawProp(Vector2 world, float r, Color c)
    {
        var s = WorldToScreen(world);
        Raylib.DrawCircle((int)s.X, (int)s.Y, r, c);
    }

    static void DrawBar(int x, int y, int w, int h, string label, float fill, Color color)
    {
        Raylib.DrawRectangle(x, y, w, h, new Color(25, 25, 30, 220));
        Raylib.DrawRectangle(x + 2, y + 2, (int)((w - 4) * Math.Clamp(fill, 0, 1)), h - 4, color);
        Raylib.DrawText(label, x + 8, y + 3, 14, Color.RayWhite);
    }

    static void DrawCentered(string text, int size, int y, Color color)
    {
        int tw = Raylib.MeasureText(text, size);
        Raylib.DrawText(text, (W - tw) / 2, y, size, color);
    }

    static void DrawWrapped(string text, int x, int y, int maxW, int size, Color color)
    {
        // simple wrap
        var words = text.Split(' ');
        string line = "";
        int ly = y;
        foreach (var word in words)
        {
            string test = string.IsNullOrEmpty(line) ? word : line + " " + word;
            if (Raylib.MeasureText(test, size) > maxW)
            {
                Raylib.DrawText(line, x, ly, size, color);
                ly += size + 4;
                line = word;
            }
            else line = test;
        }
        if (!string.IsNullOrEmpty(line))
            Raylib.DrawText(line, x, ly, size, color);
    }

    sealed class Enemy
    {
        public EnemyKind Kind;
        public Vector2 Pos;
        public float Hp;
        public EnemyProfile Profile;
        public bool Alive;
        public bool Aggro;
        public float Cd;
        public float SlowT;
        public float SlowF = 1f;
    }

    struct Shot
    {
        public Vector2 Pos, Vel;
        public float Damage, Life;
        public Color Color;
        public bool Slow;
        public Shot(Vector2 pos, Vector2 vel, float damage, float life, Color color, bool slow)
        { Pos = pos; Vel = vel; Damage = damage; Life = life; Color = color; Slow = slow; }
    }

    struct Burst
    {
        public Vector2 Pos;
        public float Radius, Damage, Life;
        public Color Color;
        public bool Applied;
        public Burst(Vector2 pos, float radius, float damage, float life, Color color, bool applied)
        { Pos = pos; Radius = radius; Damage = damage; Life = life; Color = color; Applied = applied; }
    }
}
