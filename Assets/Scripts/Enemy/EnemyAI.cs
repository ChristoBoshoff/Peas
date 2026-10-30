using UnityEngine;

namespace Riftwake
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour
    {
        EnemyKind _kind;
        EnemyStats.Profile _profile;
        Transform _player;
        CharacterController _controller;
        Health _health;
        float _cooldown;
        float _verticalVelocity;
        bool _aggro;
        float _slowTimer;
        float _slowFactor = 1f;

        public EnemyKind Kind => _kind;

        public void Configure(EnemyKind kind, Transform player)
        {
            _kind = kind;
            _profile = EnemyStats.For(kind);
            _player = player;
        }

        public void ApplySlow(float factor, float seconds)
        {
            _slowFactor = Mathf.Clamp(factor, 0.2f, 1f);
            _slowTimer = seconds;
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
            Progression.Ensure().AddKillReward(_kind);
            var runner = FindObjectOfType<MissionController>();
            if (runner != null)
                runner.RegisterKill(_kind);
            Destroy(gameObject);
        }

        void Update()
        {
            if (_health.IsDead)
                return;

            if (_slowTimer > 0f)
            {
                _slowTimer -= Time.deltaTime;
                if (_slowTimer <= 0f)
                    _slowFactor = 1f;
            }

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

            if (!_aggro && dist <= _profile.Detect)
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

            float speed = _profile.Speed * _slowFactor;

            if (dist <= _profile.AttackRange)
            {
                ApplyGravityOnly();
                if (_cooldown <= 0f)
                {
                    var hp = _player.GetComponent<Health>();
                    if (hp != null)
                        hp.ApplyDamage(_profile.Damage, gameObject);
                    _cooldown = _profile.AttackCooldown;
                }
                return;
            }

            var move = toPlayer.normalized * speed;
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += -20f * Time.deltaTime;
            move.y = _verticalVelocity;
            _controller.Move(move * Time.deltaTime);
        }

        void ApplyGravityOnly()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += -20f * Time.deltaTime;
            _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }
    }
}
