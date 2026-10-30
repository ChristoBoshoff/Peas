using System;
using UnityEngine;

namespace Riftwake
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] float maxVitality = 100f;
        [SerializeField] float currentVitality = 100f;

        public float Max => maxVitality;
        public float Current => currentVitality;
        public float Normalized => maxVitality <= 0f ? 0f : Mathf.Clamp01(currentVitality / maxVitality);
        public bool IsDead => currentVitality <= 0f;

        public event Action<Health> Died;
        public event Action<Health, float> Damaged;
        public event Action<Health> Changed;

        public void Configure(float max)
        {
            maxVitality = Mathf.Max(1f, max);
            currentVitality = maxVitality;
            Changed?.Invoke(this);
        }

        public void ApplyDamage(float amount, GameObject source)
        {
            if (IsDead || amount <= 0f)
                return;

            currentVitality = Mathf.Max(0f, currentVitality - amount);
            Damaged?.Invoke(this, amount);
            Changed?.Invoke(this);

            if (IsDead)
                Died?.Invoke(this);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            currentVitality = Mathf.Min(maxVitality, currentVitality + amount);
            Changed?.Invoke(this);
        }
    }
}
