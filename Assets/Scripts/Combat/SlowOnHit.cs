using UnityEngine;

namespace Riftwake
{
    /// <summary>Optional projectile rider that slows enemies on hit.</summary>
    public sealed class SlowOnHit : MonoBehaviour
    {
        float _factor = 0.5f;
        float _duration = 2f;

        public void Configure(float factor, float duration)
        {
            _factor = factor;
            _duration = duration;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy"))
                return;
            var ai = other.GetComponentInParent<EnemyAI>();
            ai?.ApplySlow(_factor, _duration);
        }
    }
}
