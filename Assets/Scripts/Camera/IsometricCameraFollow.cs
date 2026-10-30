using UnityEngine;

namespace Riftwake
{
    /// <summary>Fixed isometric-style chase cam over the Riftborn.</summary>
    public sealed class IsometricCameraFollow : MonoBehaviour
    {
        Transform _target;
        Vector3 _offset = new Vector3(0f, 14f, -11f);
        float _smooth = 8f;

        public void Configure(Transform target, Vector3 offset)
        {
            _target = target;
            _offset = offset;
            if (_target != null)
                transform.position = _target.position + _offset;
        }

        void LateUpdate()
        {
            if (_target == null)
                return;

            var desired = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-_smooth * Time.deltaTime));
            transform.rotation = Quaternion.Euler(48f, 0f, 0f);
        }
    }
}
