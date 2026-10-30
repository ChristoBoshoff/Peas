using UnityEngine;

namespace Riftwake
{
    /// <summary>
    /// Legacy entry for the old BrackenHollow scene — routes into MissionController.
    /// </summary>
    public sealed class BrackenHollowBootstrap : MonoBehaviour
    {
        void Awake()
        {
            var session = GameSession.Ensure();
            if (!session.HasSelectedPath)
                session.SelectPathInPlace(HeroPath.Steelhand);
            session.SetMission(MissionId.BrackenHollow);
            if (GetComponent<MissionController>() == null)
                gameObject.AddComponent<MissionController>();
        }
    }
}
