using System;

namespace Riftwake
{
    [Serializable]
    public sealed class SaveData
    {
        public int Version = 1;
        public int Path; // HeroPath
        public int Mission; // MissionId
        public int Level = 1;
        public int Xp;
        public float VitalityBonus;
        public float DamageBonus;
        public int Gold;
        public bool SecondaryUnlocked;
        public bool CampaignComplete;
        public bool HasSave;
    }
}
