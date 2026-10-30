using UnityEngine;

namespace Riftwake
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        HeroPath _path;
        PlayerMotor _motor;
        FocusResource _focus;
        float _cooldown;
        float _secondaryCooldown;
        Material _projectileMat;
        Material _meleeMat;
        float _damageBonus;

        const float MeleeDamage = 28f;
        const float MeleeRange = 1.55f;
        const float MeleeDuration = 0.18f;
        const float MeleeCooldown = 0.45f;

        const float ThornDamage = 18f;
        const float ThornSpeed = 14f;
        const float ThornCooldown = 0.4f;

        const float EmberDamage = 32f;
        const float EmberSpeed = 16f;
        const float EmberFocusCost = 18f;
        const float EmberCooldown = 0.55f;

        public string PrimaryActionName { get; private set; } = "Attack";
        public string SecondaryActionName { get; private set; } = "Locked";
        public bool InputEnabled { get; set; } = true;

        public void Configure(HeroPath path, PlayerMotor motor, FocusResource focus, Material projectileMat, Material meleeMat, float damageBonus)
        {
            _path = path;
            _motor = motor;
            _focus = focus;
            _projectileMat = projectileMat;
            _meleeMat = meleeMat;
            _damageBonus = damageBonus;

            switch (path)
            {
                case HeroPath.Steelhand:
                    PrimaryActionName = "Cleaving Strike";
                    SecondaryActionName = "Iron Guard";
                    break;
                case HeroPath.Wildbinder:
                    PrimaryActionName = "Thorn Dart";
                    SecondaryActionName = "Vine Snare";
                    break;
                case HeroPath.Spellwright:
                    PrimaryActionName = "Ember Bolt";
                    SecondaryActionName = "Flashflare";
                    break;
            }
        }

        public void SetDamageBonus(float bonus) => _damageBonus = bonus;

        void Update()
        {
            if (!InputEnabled)
                return;

            if (_cooldown > 0f)
                _cooldown -= Time.deltaTime;
            if (_secondaryCooldown > 0f)
                _secondaryCooldown -= Time.deltaTime;

            bool secondaryUnlocked = Progression.Instance != null && Progression.Instance.SecondaryUnlocked;

            if ((Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0)) && _cooldown <= 0f)
                FirePrimary();

            if (secondaryUnlocked && Input.GetKeyDown(KeyCode.Q) && _secondaryCooldown <= 0f)
                FireSecondary();
        }

        void FirePrimary()
        {
            switch (_path)
            {
                case HeroPath.Steelhand:
                    PerformMelee(MeleeDamage + _damageBonus, MeleeRange, MeleeDuration, new Color(0.9f, 0.85f, 0.55f, 0.55f));
                    _cooldown = MeleeCooldown;
                    break;
                case HeroPath.Wildbinder:
                    PerformDart(ThornDamage + _damageBonus, ThornSpeed, new Color(0.35f, 0.85f, 0.35f), 0.22f, false);
                    _cooldown = ThornCooldown;
                    break;
                case HeroPath.Spellwright:
                    if (_focus != null && !_focus.TrySpend(EmberFocusCost))
                        return;
                    PerformDart(EmberDamage + _damageBonus, EmberSpeed, new Color(1f, 0.45f, 0.15f), 0.28f, false);
                    _cooldown = EmberCooldown;
                    break;
            }
        }

        void FireSecondary()
        {
            switch (_path)
            {
                case HeroPath.Steelhand:
                    // Brief self-heal + wide cleave (Iron Guard).
                    var hp = GetComponent<Health>();
                    hp?.Heal(18f + _damageBonus);
                    PerformMelee(MeleeDamage * 0.75f + _damageBonus, MeleeRange * 1.35f, 0.22f, new Color(0.7f, 0.8f, 1f, 0.5f));
                    _secondaryCooldown = 6f;
                    break;
                case HeroPath.Wildbinder:
                    if (_focus != null && !_focus.TrySpend(12f))
                        return;
                    PerformDart(14f + _damageBonus, 12f, new Color(0.45f, 0.95f, 0.55f), 0.3f, true);
                    _secondaryCooldown = 5f;
                    break;
                case HeroPath.Spellwright:
                    if (_focus != null && !_focus.TrySpend(28f))
                        return;
                    PerformNova(26f + _damageBonus, 3.2f);
                    _secondaryCooldown = 7f;
                    break;
            }
        }

        void PerformMelee(float damage, float range, float duration, Color color)
        {
            var origin = transform.position + Vector3.up * 0.9f + _motor.AimDirection * 0.9f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "MeleeHit";
            go.tag = "MeleeHit";
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * (range * 1.2f);
            Object.Destroy(go.GetComponent<Collider>());

            var render = go.GetComponent<MeshRenderer>();
            if (_meleeMat != null)
                render.sharedMaterial = _meleeMat;
            render.material.color = color;

            var hitbox = go.AddComponent<MeleeHitbox>();
            hitbox.Arm(damage, duration, gameObject, "Enemy", range * 0.55f);
        }

        void PerformDart(float damage, float speed, Color color, float scale, bool applySlow)
        {
            var origin = transform.position + Vector3.up * 1.0f + _motor.AimDirection * 0.6f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Dart";
            go.tag = "Projectile";
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * scale;
            Object.Destroy(go.GetComponent<Collider>());

            var render = go.GetComponent<MeshRenderer>();
            render.material.color = color;

            var proj = go.AddComponent<Projectile>();
            proj.Launch(_motor.AimDirection, speed, damage, 2.5f, gameObject, "Enemy");
            if (applySlow)
                go.AddComponent<SlowOnHit>().Configure(0.45f, 2.5f);
        }

        void PerformNova(float damage, float radius)
        {
            var origin = transform.position + Vector3.up * 0.8f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Flashflare";
            go.tag = "MeleeHit";
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * (radius * 2f);
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.7f, 0.2f, 0.4f);

            var hitbox = go.AddComponent<MeleeHitbox>();
            hitbox.Arm(damage, 0.2f, gameObject, "Enemy", radius);
        }
    }
}
