using UnityEngine;

namespace Riftwake
{
    public sealed class MissionBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (GetComponent<MissionController>() == null)
                gameObject.AddComponent<MissionController>();
        }
    }
}
