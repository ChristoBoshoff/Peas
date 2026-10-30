using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// First-hour hostile: detect player, chase, melee when in range.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class AshmiteAI : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 3.6f;
        [SerializeField] float detectRange = 12f;
        [SerializeField] float attackRange = 1.35f;
        [SerializeField] float attackDamage = 10f;
        [SerializeField] float attackCooldown = 1.1f;
        [SerializeField] float gravity = -20f;

        Transform _player;
        CharacterController _controller;
        Health _health;
        float _cooldown;
        float _verticalVelocity;
        bool _aggro;

        public void Configure(Transform player, float speed, float detect, float atkRange, float damage)
        {
            _player = player;
            moveSpeed = speed;
            detectRange = detect;
            attackRange = atkRange;
            attackDamage = damage;
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            _health.Died += OnDied;
        }

        void OnDestroy()
        {
            if (_health != null)
                _health.Died -= OnDied;
        }

        void OnDied(Health _)
        {
            Destroy(gameObject);
        }

        void Update()
        {
            if (_health.IsDead)
                return;

            if (_player == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null)
                    _player = tagged.transform;
                else
                    return;
            }

            if (_cooldown > 0f)
                _cooldown -= Time.deltaTime;

            var toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            if (!_aggro && dist <= detectRange)
                _aggro = true;

            if (!_aggro)
            {
                ApplyGravityOnly();
                return;
            }

            if (dist > 0.05f)
            {
                var face = toPlayer.normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face, Vector3.up), 10f * Time.deltaTime);
            }

            if (dist <= attackRange)
            {
                ApplyGravityOnly();
                if (_cooldown <= 0f)
                {
                    var hp = _player.GetComponent<Health>();
                    if (hp != null)
                        hp.ApplyDamage(attackDamage, gameObject);
                    _cooldown = attackCooldown;
                }
                return;
            }

            var move = toPlayer.normalized * moveSpeed;
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;
            _controller.Move(move * Time.deltaTime);
        }

        void ApplyGravityOnly()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }
    }
}
