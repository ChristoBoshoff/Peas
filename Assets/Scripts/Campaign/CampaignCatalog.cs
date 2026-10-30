using System;

namespace Riftwake
{
    public enum MissionId
    {
        BrackenHollow = 0,
        Mireroad = 1,
        DunwickCrossing = 2,
        CinderMines = 3,
        Fellwood = 4,
        VergewatchKeep = 5,
        RiftHeart = 6
    }

    public enum EnemyKind
    {
        Ashmite,
        Bogmaw,
        Shardling,
        KeepGuard,
        RiftWraith
    }

    public enum MissionKind
    {
        CombatClear,
        Hub,
        Boss
    }

    [Serializable]
    public struct EnemySpawnDef
    {
        public EnemyKind Kind;
        public float X;
        public float Z;
    }

    [Serializable]
    public sealed class MissionDef
    {
        public MissionId Id;
        public string Title;
        public string Subtitle;
        public MissionKind Kind;
        public string ArrivalText;
        public string CompleteText;
        public string GroundHex;
        public float GroundScale;
        public int KillTarget; // 0 = kill all spawned, hub ignores
        public bool IsHub;
        public bool HealOnEnter;
        public bool ShopAvailable;
        public int ShopCost;
        public int ShopVitalityBonus;
        public int ShopDamageBonus;
        public EnemySpawnDef[] Spawns;
        public bool SpawnElder;
        public float ElderX;
        public float ElderZ;
        public string HubPrompt;
    }

    public static class CampaignCatalog
    {
        public static readonly MissionId[] Order =
        {
            MissionId.BrackenHollow,
            MissionId.Mireroad,
            MissionId.DunwickCrossing,
            MissionId.CinderMines,
            MissionId.Fellwood,
            MissionId.VergewatchKeep,
            MissionId.RiftHeart
        };

        public static MissionDef Get(MissionId id)
        {
            switch (id)
            {
                case MissionId.BrackenHollow:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Bracken Hollow",
                        Subtitle = "Arrival",
                        Kind = MissionKind.CombatClear,
                        ArrivalText = "The rift dumps you at Bracken Hollow. Elder Bram waits. Learn to stand.",
                        CompleteText = "The Hollow holds. The road to Mireroad opens.",
                        GroundHex = "#2C3824",
                        GroundScale = 6f,
                        KillTarget = 3,
                        SpawnElder = true,
                        ElderX = -1.5f,
                        ElderZ = 3.5f,
                        Spawns = new[]
                        {
                            S(EnemyKind.Ashmite, 8, 4),
                            S(EnemyKind.Ashmite, 10, -3),
                            S(EnemyKind.Ashmite, -9, 5),
                            S(EnemyKind.Ashmite, -7, -6),
                            S(EnemyKind.Ashmite, 6, 9)
                        }
                    };
                case MissionId.Mireroad:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Mireroad",
                        Subtitle = "Ambush",
                        Kind = MissionKind.CombatClear,
                        ArrivalText = "Mist clings to Mireroad. Bogmaws hunt beside the Ashmites.",
                        CompleteText = "The ambush breaks. Dunwick Crossing lies ahead.",
                        GroundHex = "#243028",
                        GroundScale = 7f,
                        KillTarget = 0,
                        Spawns = new[]
                        {
                            S(EnemyKind.Ashmite, 7, 2),
                            S(EnemyKind.Ashmite, -8, 3),
                            S(EnemyKind.Bogmaw, 9, -5),
                            S(EnemyKind.Bogmaw, -6, -7),
                            S(EnemyKind.Ashmite, 3, 10),
                            S(EnemyKind.Bogmaw, -10, 8)
                        }
                    };
                case MissionId.DunwickCrossing:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Dunwick Crossing",
                        Subtitle = "Waystation",
                        Kind = MissionKind.Hub,
                        ArrivalText = "Dunwick Crossing — a battered waystation. Rest, spend coin, then press on.",
                        CompleteText = "You leave Dunwick stronger. The Cinder Mines wait.",
                        GroundHex = "#2A2A30",
                        GroundScale = 5f,
                        IsHub = true,
                        HealOnEnter = true,
                        ShopAvailable = true,
                        ShopCost = 40,
                        ShopVitalityBonus = 25,
                        ShopDamageBonus = 6,
                        HubPrompt = "Press E to buy a Field Charm (+Vitality, +Damage) if you can afford it. Press Enter to depart for the Cinder Mines.",
                        Spawns = Array.Empty<EnemySpawnDef>()
                    };
                case MissionId.CinderMines:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Cinder Mines",
                        Subtitle = "Delve",
                        Kind = MissionKind.CombatClear,
                        ArrivalText = "Heat rolls from the Cinder Mines. Shardlings spark in the dark.",
                        CompleteText = "The mines fall quiet. Fellwood's canopy waits beyond.",
                        GroundHex = "#2E2218",
                        GroundScale = 6.5f,
                        KillTarget = 0,
                        Spawns = new[]
                        {
                            S(EnemyKind.Shardling, 6, 4),
                            S(EnemyKind.Shardling, -7, 5),
                            S(EnemyKind.Ashmite, 8, -4),
                            S(EnemyKind.Shardling, -5, -8),
                            S(EnemyKind.Bogmaw, 10, 8),
                            S(EnemyKind.Shardling, -11, -2),
                            S(EnemyKind.Ashmite, 2, 11)
                        }
                    };
                case MissionId.Fellwood:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Fellwood",
                        Subtitle = "Approach",
                        Kind = MissionKind.CombatClear,
                        ArrivalText = "Fellwood twists toward Vergewatch Keep. The pack is thicker here.",
                        CompleteText = "You break through Fellwood. The Keep looms.",
                        GroundHex = "#1C2818",
                        GroundScale = 7.5f,
                        KillTarget = 0,
                        Spawns = new[]
                        {
                            S(EnemyKind.Bogmaw, 8, 3),
                            S(EnemyKind.Bogmaw, -9, 4),
                            S(EnemyKind.Shardling, 7, -6),
                            S(EnemyKind.Shardling, -6, -7),
                            S(EnemyKind.Ashmite, 11, 7),
                            S(EnemyKind.Ashmite, -12, -3),
                            S(EnemyKind.Bogmaw, 0, 12),
                            S(EnemyKind.Shardling, 4, -11)
                        }
                    };
                case MissionId.VergewatchKeep:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Vergewatch Keep",
                        Subtitle = "Assault",
                        Kind = MissionKind.CombatClear,
                        ArrivalText = "Vergewatch Keep's gates hang open. Keep Guards will not yield the hall.",
                        CompleteText = "The Keep is yours. One path remains — the Rift Heart.",
                        GroundHex = "#303038",
                        GroundScale = 7f,
                        KillTarget = 0,
                        Spawns = new[]
                        {
                            S(EnemyKind.KeepGuard, 6, 2),
                            S(EnemyKind.KeepGuard, -6, 2),
                            S(EnemyKind.KeepGuard, 0, 8),
                            S(EnemyKind.Shardling, 9, -5),
                            S(EnemyKind.Shardling, -9, -5),
                            S(EnemyKind.Bogmaw, 11, 6),
                            S(EnemyKind.KeepGuard, -3, -9)
                        }
                    };
                case MissionId.RiftHeart:
                    return new MissionDef
                    {
                        Id = id,
                        Title = "Rift Heart",
                        Subtitle = "Finale",
                        Kind = MissionKind.Boss,
                        ArrivalText = "The Rift Heart pulses. A Rift Wraith coils from the tear — end this.",
                        CompleteText = "The Wraith unravels. The Verge steadies. Your first war is done.",
                        GroundHex = "#1A1020",
                        GroundScale = 8f,
                        KillTarget = 1,
                        Spawns = new[]
                        {
                            S(EnemyKind.RiftWraith, 0, 8),
                            S(EnemyKind.Shardling, 7, 3),
                            S(EnemyKind.Shardling, -7, 3),
                            S(EnemyKind.Ashmite, 5, -4),
                            S(EnemyKind.Ashmite, -5, -4)
                        }
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        public static MissionId? Next(MissionId current)
        {
            for (int i = 0; i < Order.Length - 1; i++)
            {
                if (Order[i] == current)
                    return Order[i + 1];
            }
            return null;
        }

        public static int IndexOf(MissionId id)
        {
            for (int i = 0; i < Order.Length; i++)
                if (Order[i] == id) return i;
            return 0;
        }

        static EnemySpawnDef S(EnemyKind kind, float x, float z) =>
            new EnemySpawnDef { Kind = kind, X = x, Z = z };
    }
}
