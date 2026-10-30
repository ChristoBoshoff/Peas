using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// Path-gated primary attack on LMB / Fire1:
    /// Steelhand Cleaving Strike, Wildbinder Thorn Dart, Spellwright Ember Bolt.
    /// </summary>
    public sealed class PlayerCombat : MonoBehaviour
    {
        HeroPath _path;
        PlayerMotor _motor;
        FocusResource _focus;
        float _cooldown;
        Material _projectileMat;
        Material _meleeMat;

        // Tuned for first-hour tutorial combat feel.
        const float MeleeDamage = 28f;
        const float MeleeRange = 1.55f;
        const float MeleeDuration = 0.18f;
        const float MeleeCooldown = 0.45f;

        const float ThornDamage = 18f;
        const float ThornSpeed = 14f;
        const float ThornFocusCost = 0f;
        const float ThornCooldown = 0.4f;

        const float EmberDamage = 32f;
        const float EmberSpeed = 16f;
        const float EmberFocusCost = 18f;
        const float EmberCooldown = 0.55f;

        public string PrimaryActionName { get; private set; } = "Attack";
        public bool InputEnabled { get; set; } = true;

        public void Configure(HeroPath path, PlayerMotor motor, FocusResource focus, Material projectileMat, Material meleeMat)
        {
            _path = path;
            _motor = motor;
            _focus = focus;
            _projectileMat = projectileMat;
            _meleeMat = meleeMat;

            switch (path)
            {
                case HeroPath.Steelhand:
                    PrimaryActionName = "Cleaving Strike";
                    break;
                case HeroPath.Wildbinder:
                    PrimaryActionName = "Thorn Dart";
                    break;
                case HeroPath.Spellwright:
                    PrimaryActionName = "Ember Bolt";
                    break;
            }
        }

        void Update()
        {
            if (!InputEnabled)
                return;

            if (_cooldown > 0f)
                _cooldown -= Time.deltaTime;

            if (!Input.GetButtonDown("Fire1") && !Input.GetMouseButtonDown(0))
                return;

            if (_cooldown > 0f)
                return;

            switch (_path)
            {
                case HeroPath.Steelhand:
                    PerformMelee();
                    break;
                case HeroPath.Wildbinder:
                    PerformDart(ThornDamage, ThornSpeed, ThornFocusCost, new Color(0.35f, 0.85f, 0.35f), 0.22f);
                    _cooldown = ThornCooldown;
                    break;
                case HeroPath.Spellwright:
                    if (_focus != null && !_focus.TrySpend(EmberFocusCost))
                        return;
                    PerformDart(EmberDamage, EmberSpeed, EmberFocusCost, new Color(1f, 0.45f, 0.15f), 0.28f);
                    _cooldown = EmberCooldown;
                    break;
            }
        }

        void PerformMelee()
        {
            _cooldown = MeleeCooldown;
            var origin = transform.position + Vector3.up * 0.9f + _motor.AimDirection * 0.9f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "CleavingStrike";
            go.tag = "MeleeHit";
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * (MeleeRange * 1.2f);
            Object.Destroy(go.GetComponent<Collider>());

            var render = go.GetComponent<MeshRenderer>();
            if (_meleeMat != null)
                render.sharedMaterial = _meleeMat;
            else
                render.material.color = new Color(0.9f, 0.85f, 0.55f, 0.55f);

            var hitbox = go.AddComponent<MeleeHitbox>();
            hitbox.Arm(MeleeDamage, MeleeDuration, gameObject, "Enemy", MeleeRange * 0.55f);
        }

        void PerformDart(float damage, float speed, float unusedCost, Color color, float scale)
        {
            var origin = transform.position + Vector3.up * 1.0f + _motor.AimDirection * 0.6f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = PrimaryActionName.Replace(" ", string.Empty);
            go.tag = "Projectile";
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * scale;
            Object.Destroy(go.GetComponent<Collider>());

            var render = go.GetComponent<MeshRenderer>();
            if (_projectileMat != null)
            {
                render.sharedMaterial = _projectileMat;
                render.material.color = color;
            }
            else
            {
                render.material.color = color;
            }

            var proj = go.AddComponent<Projectile>();
            proj.Launch(_motor.AimDirection, speed, damage, 2.5f, gameObject, "Enemy");
        }
    }
}
