using System;
using UnityEngine;

namespace Riftwake
{
    /// <summary>XP, levels, gold, and combat bonuses across the campaign.</summary>
    public sealed class Progression : MonoBehaviour
    {
        public static Progression Instance { get; private set; }

        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int Gold { get; private set; }
        public float VitalityBonus { get; private set; }
        public float DamageBonus { get; private set; }
        public bool SecondaryUnlocked { get; private set; }

        public event Action<Progression> Changed;

        public static int XpToNext(int level) => 40 + (level - 1) * 30;

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

        public static Progression Ensure()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("Progression");
            return go.AddComponent<Progression>();
        }

        public void ResetNewGame()
        {
            Level = 1;
            Xp = 0;
            Gold = 15;
            VitalityBonus = 0f;
            DamageBonus = 0f;
            SecondaryUnlocked = false;
            Changed?.Invoke(this);
        }

        public void LoadFrom(SaveData data)
        {
            Level = Mathf.Max(1, data.Level);
            Xp = Mathf.Max(0, data.Xp);
            Gold = Mathf.Max(0, data.Gold);
            VitalityBonus = data.VitalityBonus;
            DamageBonus = data.DamageBonus;
            SecondaryUnlocked = data.SecondaryUnlocked;
            Changed?.Invoke(this);
        }

        public void AddKillReward(EnemyKind kind)
        {
            int xp;
            int gold;
            switch (kind)
            {
                case EnemyKind.Ashmite: xp = 12; gold = 3; break;
                case EnemyKind.Bogmaw: xp = 18; gold = 5; break;
                case EnemyKind.Shardling: xp = 22; gold = 6; break;
                case EnemyKind.KeepGuard: xp = 30; gold = 9; break;
                case EnemyKind.RiftWraith: xp = 120; gold = 40; break;
                default: xp = 10; gold = 2; break;
            }
            AddXp(xp);
            Gold += gold;
            Changed?.Invoke(this);
        }

        public void AddXp(int amount)
        {
            Xp += amount;
            while (Xp >= XpToNext(Level))
            {
                Xp -= XpToNext(Level);
                Level++;
                VitalityBonus += 12f;
                DamageBonus += 3f;
                if (Level >= 3)
                    SecondaryUnlocked = true;
            }
            Changed?.Invoke(this);
        }

        public bool TryBuyShop(int cost, float vit, float dmg)
        {
            if (Gold < cost) return false;
            Gold -= cost;
            VitalityBonus += vit;
            DamageBonus += dmg;
            Changed?.Invoke(this);
            return true;
        }

        public void WriteInto(SaveData data)
        {
            data.Level = Level;
            data.Xp = Xp;
            data.Gold = Gold;
            data.VitalityBonus = VitalityBonus;
            data.DamageBonus = DamageBonus;
            data.SecondaryUnlocked = SecondaryUnlocked;
        }
    }
}
