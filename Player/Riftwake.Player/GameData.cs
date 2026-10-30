using Raylib_cs;

namespace Riftwake.Player;

enum HeroPath { Steelhand, Wildbinder, Spellwright }
enum MissionId { BrackenHollow, Mireroad, DunwickCrossing, CinderMines, Fellwood, VergewatchKeep, RiftHeart }
enum EnemyKind { Ashmite, Bogmaw, Shardling, KeepGuard, RiftWraith }
enum Screen { MainMenu, ClassSelect, Mission, Ending }

sealed class SaveData
{
    public int Version { get; set; } = 1;
    public int Path { get; set; }
    public int Mission { get; set; }
    public int Level { get; set; } = 1;
    public int Xp { get; set; }
    public float VitalityBonus { get; set; }
    public float DamageBonus { get; set; }
    public int Gold { get; set; }
    public bool SecondaryUnlocked { get; set; }
    public bool CampaignComplete { get; set; }
    public bool HasSave { get; set; }
}

readonly record struct EnemySpawn(EnemyKind Kind, float X, float Y);

sealed class MissionDef
{
    public required MissionId Id { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string Arrival { get; init; }
    public required string Complete { get; init; }
    public required Color Ground { get; init; }
    public bool IsHub { get; init; }
    public bool HealOnEnter { get; init; }
    public bool Shop { get; init; }
    public int ShopCost { get; init; } = 40;
    public int KillTarget { get; init; } // 0 = all
    public bool SpawnElder { get; init; }
    public required EnemySpawn[] Spawns { get; init; }
    public string HubPrompt { get; init; } = "";
}

static class Campaign
{
    public static readonly MissionId[] Order =
    {
        MissionId.BrackenHollow, MissionId.Mireroad, MissionId.DunwickCrossing,
        MissionId.CinderMines, MissionId.Fellwood, MissionId.VergewatchKeep, MissionId.RiftHeart
    };

    public static MissionDef Get(MissionId id) => id switch
    {
        MissionId.BrackenHollow => new MissionDef
        {
            Id = id, Title = "Bracken Hollow", Subtitle = "Arrival",
            Arrival = "The rift dumps you at Bracken Hollow. Elder Bram waits. Learn to stand.",
            Complete = "The Hollow holds. The road to Mireroad opens.",
            Ground = new Color(44, 56, 36, 255), KillTarget = 3, SpawnElder = true,
            Spawns =
            [
                new(EnemyKind.Ashmite, 280, -140), new(EnemyKind.Ashmite, 340, 100),
                new(EnemyKind.Ashmite, -300, -160), new(EnemyKind.Ashmite, -240, 200),
                new(EnemyKind.Ashmite, 200, 300)
            ]
        },
        MissionId.Mireroad => new MissionDef
        {
            Id = id, Title = "Mireroad", Subtitle = "Ambush",
            Arrival = "Mist clings to Mireroad. Bogmaws hunt beside the Ashmites.",
            Complete = "The ambush breaks. Dunwick Crossing lies ahead.",
            Ground = new Color(36, 48, 40, 255),
            Spawns =
            [
                new(EnemyKind.Ashmite, 240, -80), new(EnemyKind.Ashmite, -260, -100),
                new(EnemyKind.Bogmaw, 300, 160), new(EnemyKind.Bogmaw, -200, 220),
                new(EnemyKind.Ashmite, 100, -320), new(EnemyKind.Bogmaw, -320, -200)
            ]
        },
        MissionId.DunwickCrossing => new MissionDef
        {
            Id = id, Title = "Dunwick Crossing", Subtitle = "Waystation",
            Arrival = "Dunwick Crossing — rest, spend coin, then press on.",
            Complete = "You leave Dunwick stronger. The Cinder Mines wait.",
            Ground = new Color(42, 42, 48, 255), IsHub = true, HealOnEnter = true, Shop = true,
            HubPrompt = "E: buy Field Charm (40 gold). Enter: depart for Cinder Mines.",
            Spawns = []
        },
        MissionId.CinderMines => new MissionDef
        {
            Id = id, Title = "Cinder Mines", Subtitle = "Delve",
            Arrival = "Heat rolls from the Cinder Mines. Shardlings spark in the dark.",
            Complete = "The mines fall quiet. Fellwood waits beyond.",
            Ground = new Color(46, 34, 24, 255),
            Spawns =
            [
                new(EnemyKind.Shardling, 220, -120), new(EnemyKind.Shardling, -240, -140),
                new(EnemyKind.Ashmite, 280, 140), new(EnemyKind.Shardling, -180, 260),
                new(EnemyKind.Bogmaw, 320, -240), new(EnemyKind.Shardling, -340, 80),
                new(EnemyKind.Ashmite, 60, 340)
            ]
        },
        MissionId.Fellwood => new MissionDef
        {
            Id = id, Title = "Fellwood", Subtitle = "Approach",
            Arrival = "Fellwood twists toward Vergewatch Keep. The pack is thicker.",
            Complete = "You break through Fellwood. The Keep looms.",
            Ground = new Color(28, 40, 24, 255),
            Spawns =
            [
                new(EnemyKind.Bogmaw, 260, -100), new(EnemyKind.Bogmaw, -280, -120),
                new(EnemyKind.Shardling, 240, 180), new(EnemyKind.Shardling, -200, 220),
                new(EnemyKind.Ashmite, 340, -220), new(EnemyKind.Ashmite, -360, 100),
                new(EnemyKind.Bogmaw, 0, -360), new(EnemyKind.Shardling, 140, 340)
            ]
        },
        MissionId.VergewatchKeep => new MissionDef
        {
            Id = id, Title = "Vergewatch Keep", Subtitle = "Assault",
            Arrival = "Vergewatch Keep's gates hang open. Keep Guards will not yield.",
            Complete = "The Keep is yours. One path remains — the Rift Heart.",
            Ground = new Color(48, 48, 56, 255),
            Spawns =
            [
                new(EnemyKind.KeepGuard, 200, -80), new(EnemyKind.KeepGuard, -200, -80),
                new(EnemyKind.KeepGuard, 0, -260), new(EnemyKind.Shardling, 300, 160),
                new(EnemyKind.Shardling, -300, 160), new(EnemyKind.Bogmaw, 340, -200),
                new(EnemyKind.KeepGuard, -100, 300)
            ]
        },
        MissionId.RiftHeart => new MissionDef
        {
            Id = id, Title = "Rift Heart", Subtitle = "Finale",
            Arrival = "The Rift Heart pulses. A Rift Wraith coils from the tear — end this.",
            Complete = "The Wraith unravels. The Verge steadies. Your first war is done.",
            Ground = new Color(26, 16, 32, 255), KillTarget = 1,
            Spawns =
            [
                new(EnemyKind.RiftWraith, 0, -260),
                new(EnemyKind.Shardling, 220, -100), new(EnemyKind.Shardling, -220, -100),
                new(EnemyKind.Ashmite, 160, 140), new(EnemyKind.Ashmite, -160, 140)
            ]
        },
        _ => throw new ArgumentOutOfRangeException(nameof(id))
    };

    public static MissionId? Next(MissionId current)
    {
        for (int i = 0; i < Order.Length - 1; i++)
            if (Order[i] == current) return Order[i + 1];
        return null;
    }

    public static string PathName(HeroPath p) => p switch
    {
        HeroPath.Steelhand => "Steelhand",
        HeroPath.Wildbinder => "Wildbinder",
        _ => "Spellwright"
    };

    public static string PathBlurb(HeroPath p) => p switch
    {
        HeroPath.Steelhand => "Close-quarters fighter. Cleaving Strike, later Iron Guard.",
        HeroPath.Wildbinder => "Nature hybrid. Thorn Dart, later Vine Snare.",
        _ => "Caster. Ember Bolt, later Flashflare nova."
    };
}

readonly record struct EnemyProfile(
    string Name, float Hp, float Speed, float Detect, float Range, float Damage, float Cooldown, float Radius, Color Color, bool Boss, int Xp, int Gold);

static class EnemyDb
{
    public static EnemyProfile For(EnemyKind k) => k switch
    {
        EnemyKind.Ashmite => new("Ashmite", 45, 140, 380, 42, 9, 1.1f, 14, new Color(140, 40, 30, 255), false, 12, 3),
        EnemyKind.Bogmaw => new("Bogmaw", 80, 105, 400, 48, 14, 1.25f, 18, new Color(55, 100, 70, 255), false, 18, 5),
        EnemyKind.Shardling => new("Shardling", 55, 165, 420, 40, 12, 0.95f, 13, new Color(190, 110, 50, 255), false, 22, 6),
        EnemyKind.KeepGuard => new("Keep Guard", 120, 115, 450, 50, 18, 1.15f, 20, new Color(90, 95, 130, 255), false, 30, 9),
        EnemyKind.RiftWraith => new("Rift Wraith", 420, 125, 9999, 58, 22, 0.9f, 34, new Color(140, 50, 190, 255), true, 120, 40),
        _ => For(EnemyKind.Ashmite)
    };
}
