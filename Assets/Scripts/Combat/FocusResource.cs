using System;
using UnityEngine;

namespace Riftwake
{
    /// <summary>Caster resource (Focus). Regenerates slowly out of spent casts.</summary>
    public sealed class FocusResource : MonoBehaviour
    {
        [SerializeField] float maxFocus = 100f;
        [SerializeField] float currentFocus = 100f;
        [SerializeField] float regenPerSecond = 8f;

        public float Max => maxFocus;
        public float Current => currentFocus;
        public float Normalized => maxFocus <= 0f ? 0f : Mathf.Clamp01(currentFocus / maxFocus);

        public event Action<FocusResource> Changed;

        public void Configure(float max, float regen)
        {
            maxFocus = Mathf.Max(1f, max);
            currentFocus = maxFocus;
            regenPerSecond = Mathf.Max(0f, regen);
            Changed?.Invoke(this);
        }

        void Update()
        {
            if (currentFocus >= maxFocus || regenPerSecond <= 0f)
                return;

            currentFocus = Mathf.Min(maxFocus, currentFocus + regenPerSecond * Time.deltaTime);
            Changed?.Invoke(this);
        }

        public bool TrySpend(float cost)
        {
            if (cost <= 0f)
                return true;
            if (currentFocus < cost)
                return false;

            currentFocus -= cost;
            Changed?.Invoke(this);
            return true;
        }
    }
}
