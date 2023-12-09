
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vket2023Winter.Circle0000
{
    namespace ButterLand
    {
        [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
        public class StationController : UdonSharpBehaviour
        {
            public bool IsSeated { get; private set; }

            private BoothManager _boothManager;
            private int _id;

            public void Initialize(BoothManager boothManager, int id)
            {
                _boothManager = boothManager;
                _id = id;
                IsSeated = false;
            }

            public override void OnStationEntered(VRCPlayerApi player)
            {
                IsSeated = true;
                _boothManager.ST_C_OnStationEntered(_id);
            }

            public override void OnStationExited(VRCPlayerApi player)
            {
                IsSeated = false;
                _boothManager.ST_C_OnStationExited(_id);
            }
        }
    }
}
