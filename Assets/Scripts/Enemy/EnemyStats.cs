using UnityEngine;

namespace Riftwake
{
    public static class EnemyStats
    {
        public struct Profile
        {
            public string Name;
            public float Vitality;
            public float Speed;
            public float Detect;
            public float AttackRange;
            public float Damage;
            public float AttackCooldown;
            public float Scale;
            public Color Color;
            public bool IsBoss;
        }

        public static Profile For(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Ashmite:
                    return new Profile
                    {
                        Name = "Ashmite",
                        Vitality = 45f,
                        Speed = 3.7f,
                        Detect = 11f,
                        AttackRange = 1.3f,
                        Damage = 9f,
                        AttackCooldown = 1.1f,
                        Scale = 1f,
                        Color = new Color(0.55f, 0.15f, 0.12f)
                    };
                case EnemyKind.Bogmaw:
                    return new Profile
                    {
                        Name = "Bogmaw",
                        Vitality = 80f,
                        Speed = 2.8f,
                        Detect = 12f,
                        AttackRange = 1.5f,
                        Damage = 14f,
                        AttackCooldown = 1.25f,
                        Scale = 1.25f,
                        Color = new Color(0.22f, 0.4f, 0.28f)
                    };
                case EnemyKind.Shardling:
                    return new Profile
                    {
                        Name = "Shardling",
                        Vitality = 55f,
                        Speed = 4.2f,
                        Detect = 13f,
                        AttackRange = 1.2f,
                        Damage = 12f,
                        AttackCooldown = 0.95f,
                        Scale = 0.9f,
                        Color = new Color(0.75f, 0.45f, 0.2f)
                    };
                case EnemyKind.KeepGuard:
                    return new Profile
                    {
                        Name = "Keep Guard",
                        Vitality = 120f,
                        Speed = 3.1f,
                        Detect = 14f,
                        AttackRange = 1.55f,
                        Damage = 18f,
                        AttackCooldown = 1.15f,
                        Scale = 1.35f,
                        Color = new Color(0.35f, 0.38f, 0.5f)
                    };
                case EnemyKind.RiftWraith:
                    return new Profile
                    {
                        Name = "Rift Wraith",
                        Vitality = 420f,
                        Speed = 3.4f,
                        Detect = 30f,
                        AttackRange = 1.8f,
                        Damage = 22f,
                        AttackCooldown = 0.9f,
                        Scale = 2.1f,
                        Color = new Color(0.55f, 0.2f, 0.75f),
                        IsBoss = true
                    };
                default:
                    return For(EnemyKind.Ashmite);
            }
        }
    }
}
