using System.Collections.Generic;
using UnityEngine;

namespace Riftwake
{
    /// <summary>Short-lived arc volume for Cleaving Strike.</summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class MeleeHitbox : MonoBehaviour
    {
        float _damage;
        float _life;
        GameObject _owner;
        string _targetTag;
        readonly HashSet<Health> _hit = new HashSet<Health>();

        public void Arm(float damage, float lifetime, GameObject owner, string targetTag, float radius)
        {
            _damage = damage;
            _life = lifetime;
            _owner = owner;
            _targetTag = targetTag;

            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            if (GetComponent<Rigidbody>() == null)
            {
                var rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f)
                Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_owner != null && (other.transform == _owner.transform || other.transform.IsChildOf(_owner.transform)))
                return;

            if (!string.IsNullOrEmpty(_targetTag) && !other.CompareTag(_targetTag))
                return;

            var health = other.GetComponentInParent<Health>();
            if (health == null || !_hit.Add(health))
                return;

            health.ApplyDamage(_damage, _owner);
        }
    }
}
