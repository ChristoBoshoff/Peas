using UnityEngine;

namespace Riftwake
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class Projectile : MonoBehaviour
    {
        Vector3 _velocity;
        float _damage;
        float _life;
        GameObject _owner;
        string _targetTag;

        public void Launch(Vector3 direction, float speed, float damage, float lifetime, GameObject owner, string targetTag)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
            _life = lifetime;
            _owner = owner;
            _targetTag = targetTag;

            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.25f;

            if (GetComponent<Rigidbody>() == null)
            {
                var rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        void Update()
        {
            transform.position += _velocity * Time.deltaTime;
            _life -= Time.deltaTime;
            if (_life <= 0f)
                Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_owner != null && (other.transform == _owner.transform || other.transform.IsChildOf(_owner.transform)))
                return;

            if (!string.IsNullOrEmpty(_targetTag) && !other.CompareTag(_targetTag))
            {
                // Still die on solid world geometry (non-trigger, non-actor).
                if (!other.isTrigger && other.GetComponentInParent<Health>() == null)
                    Destroy(gameObject);
                return;
            }

            var health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                health.ApplyDamage(_damage, _owner);
                Destroy(gameObject);
            }
        }
    }
}
