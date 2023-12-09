using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Vket2023Winter.Circle0000
{
    namespace ButterLand
    {
        enum ASC_AvatarEyeHeightChangeState
        {
            OutOfBooth,
            InBoothNotChnaged,
            Changed,
        }

        enum JTC_CoasterState
        {
            Wait,
            Carried,
            FreeFall,
            Deceleration,
        }

        enum PIC_Role
        {
            None,
            JetCoaster,
            Wheel,
        }

        [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
        public class BoothManager : UdonSharpBehaviour
        {
            [SerializeField] Transform _boothRoot;

            bool _initialized;
            bool _isInBooth;
            bool _isDesktopMode;
            Vector3 _myPlayerPos;

            void Start()
            {
                _isInBooth = false;
                _myPlayerPos = Vector3.zero;
                _isDesktopMode = Networking.LocalPlayer == null || !Networking.LocalPlayer.IsUserInVR();

                TM_Initialize();
                CTMI_Initialize();
                ASC_Initialize();
                WHL_Initialize();
                JTC_C_Initialize();
                ST_C_Initialize();
                PIDF_Initialize();
                BT_E_Initialize();
                BT_J_Initialize();

                _initialized = true;

                // for test
                OnBoothEnter();
            }

            void Update()
            {
                if (!_initialized)
                {
                    return;
                }

                if (Networking.LocalPlayer != null)
                {
                    _myPlayerPos = Networking.LocalPlayer.GetPosition();
                }

                TM_Update();
                CTMI_Update();
                ASC_Update();
                WHL_Update();
                JTC_C_Update();
                ST_C_Update();
                PIDF_Update();
                BT_E_Update();
                BT_J_Update();
            }

            void OnBoothEnter()
            {
                _isInBooth = true;

                TM_OnBoothEnter();
                ASC_OnBoothEnter();
                CTMI_U_OnBoothEnter();
            }

            void OnBoothExit()
            {
                _isInBooth = false;

                ASC_OnBoothExit();
                PIDF_OnBoothExit();
            }

            bool GetMoveInput()
            {
                if (_isDesktopMode)
                {
                    return
                        Input.GetKeyDown(KeyCode.W) ||
                        Input.GetKeyDown(KeyCode.A) ||
                        Input.GetKeyDown(KeyCode.S) ||
                        Input.GetKeyDown(KeyCode.D);
                }

                return
                    Mathf.Abs(Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical")) > 0.8f ||
                    Mathf.Abs(Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal")) > 0.8f;
            }

            /// <summary>
            /// CTMI_U: Circle Timer Image Utility
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            [SerializeField] Transform[] CTMI_U_timerAnchors;
            [SerializeField] Transform CTMI_U_timer;

            void CTMI_U_SetForJetCoaster(int anchorIdx)
            {
                CTMI_SetupTimer(Time.time + 60 - TM_fSec, 60);
                CTMI_U_timer.parent = CTMI_U_timerAnchors[anchorIdx];
                CTMI_U_timer.localPosition = Vector3.zero;
                CTMI_U_timer.localEulerAngles = Vector3.zero;
            }

            void CTMI_U_OnBoothEnter()
            {
                CTMI_U_SetForJetCoaster(0);
            }

            /// <summary>
            /// CTMI: Circle Timer Image
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            [SerializeField] Image CTMI_circleImage;
            [SerializeField] Image CTMI_iconImage;

            bool CTMI_isCounting;
            float CTMI_targetTime, CTMI_totalTime;

            void CTMI_Initialize()
            {
                CTMI_Reset();
            }

            void CTMI_Update()
            {
                if (!CTMI_isCounting)
                {
                    return;
                }

                var diff = CTMI_targetTime - Time.time;
                if (diff <= 0)
                {
                    CTMI_Reset();
                    return;
                }

                CTMI_SetValue(diff / CTMI_totalTime);
            }

            void CTMI_Reset()
            {
                CTMI_isCounting = false;
                CTMI_SetValue(0);
            }

            void CTMI_SetupTimer(float targetTime, float totalTime)
            {
                CTMI_targetTime = targetTime;
                CTMI_totalTime = totalTime;
                CTMI_isCounting = true;
            }

            void CTMI_SetValue(float amount)
            {
                CTMI_circleImage.fillAmount = Mathf.Clamp01(amount);
            }

            /// <summary>
            /// BT_J: Butter Jet Coaster
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const float BT_J_ANIMATION_SPEED_RATE = 4;
            const float BT_J_ANIMATION_SPEED_MIN = 1;

            [SerializeField] Transform BT_J_body2;
            [SerializeField] Transform BT_J_anchor2;
            [SerializeField] Animator[] BT_J_animators;
            [SerializeField] Transform[] BT_J_handles;
            [SerializeField] Transform[] BT_J_hands;

            void BT_J_Initialize()
            {
                BT_J_body2.parent = BT_J_anchor2;
                BT_J_body2.localPosition = Vector3.zero;

                for (int i = 0; i < BT_J_handles.Length; i++)
                {
                    BT_J_handles[i].parent = BT_J_hands[i];
                    BT_J_handles[i].localPosition = Vector3.zero;
                }
            }

            void BT_J_Update()
            {
                float speed = 0;
                if (JTC_coasterState != JTC_CoasterState.Wait)
                {
                    speed = JTC_v * BT_J_ANIMATION_SPEED_RATE;
                    if (speed < BT_J_ANIMATION_SPEED_MIN)
                    {
                        speed = BT_J_ANIMATION_SPEED_MIN;
                    }
                }
                foreach (var animator in BT_J_animators)
                {
                    animator.speed = speed;
                }
            }

            /// <summary>
            /// BT_E: Butter Entrance
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const float BT_E_MAX_ANGLE = 20;
            const float BT_E_ANGLE_RATE = 0.33f;

            [SerializeField] Transform BT_E_body;
            [SerializeField] Transform BT_E_t1, BT_E_t2, BT_E_t3;

            void BT_E_Initialize() { }

            void BT_E_Update()
            {
                var angle = Vector3.SignedAngle(BT_E_body.forward, Vector3.ProjectOnPlane(_myPlayerPos - BT_E_body.position, Vector3.up), Vector3.up);
                angle *= BT_E_ANGLE_RATE;
                if (Mathf.Abs(angle) > BT_E_MAX_ANGLE)
                {
                    angle = BT_E_MAX_ANGLE * Mathf.Sign(angle);
                }

                var angles = BT_E_t1.localEulerAngles;
                angles.y = angle;
                BT_E_t1.localEulerAngles = angles;

                angles = BT_E_t2.localEulerAngles;
                angles.y = angle;
                BT_E_t2.localEulerAngles = angles;

                angles = BT_E_t3.localEulerAngles;
                angles.y = angle;
                BT_E_t3.localEulerAngles = angles;
            }

            /// <summary>
            /// ST_C: Seat Controller
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            float[] ST_C_distanceList;
            int[] ST_C_sortIdxList;

            public void ST_C_OnStationEntered(int id)
            {
            }

            public void ST_C_OnStationExited(int id)
            {
                STT_OnLeaveSeat(id);
            }

            void ST_C_Initialize()
            {
                PIC_Initialize();
                STT_Initialize();

                ST_C_distanceList = new float[WHL_Seats.Length + JTC_Seats.Length];
                ST_C_sortIdxList = new int[PIC_pickups.Length];
            }

            void ST_C_Update()
            {
                PIC_Update();
                STT_Update();

                ST_C_AssignPicRoles();
                if (PIDF_isFollowing)
                {
                    if (GetMoveInput())
                    {
                        STT_OnLeaveDummySeat();
                        PIDF_Reset();
                    }
                }
            }

            void ST_C_UseSeat(int pickId, PIC_Role role, int targetId)
            {
                PIC_Release(pickId);
                PIC_RemoveRole(pickId, role, targetId);
                PIC_DisableAll();

                ST_C_SetSeatState(role, targetId, true);

                // to avoid station bug
                // if (STT_UseStation(role, targetId))
                // {
                //     return;
                // };

                ST_C_UseDummyStation(role, targetId);
            }

            void ST_C_LeaveSeat(PIC_Role role, int targetId)
            {
                ST_C_SetSeatState(role, targetId, false);
            }

            void ST_C_SetSeatState(PIC_Role role, int targetId, bool isSeated)
            {
                switch (role)
                {
                    case PIC_Role.JetCoaster:
                        {
                            JTC_isSeated[targetId] = isSeated;
                            return;
                        }

                    case PIC_Role.Wheel:
                        {
                            WHL_isSeated[targetId] = isSeated;
                            return;
                        }
                }
            }

            Transform ST_C_GetSeatAnchor(PIC_Role role, int targetId)
            {
                switch (role)
                {
                    case PIC_Role.JetCoaster:
                        return JTC_Seats[targetId];

                    case PIC_Role.Wheel:
                        return WHL_Seats[targetId];
                }
                return null;
            }

            int ST_C_GetSeatCount()
            {
                return WHL_Seats.Length + JTC_Seats.Length;
            }

            Transform ST_C_GetSeat(int i)
            {
                if (i < WHL_Seats.Length)
                {
                    return WHL_Seats[i];
                }

                i -= WHL_Seats.Length;
                if (i < JTC_Seats.Length)
                {
                    return JTC_Seats[i];
                }

                return null;
            }

            bool ST_C_GetIsSeated(int i)
            {
                if (i < WHL_Seats.Length)
                {
                    return WHL_isSeated[i];
                }

                i -= WHL_Seats.Length;
                if (i < JTC_Seats.Length)
                {
                    return JTC_isSeated[i];
                }

                return true;
            }

            void ST_C_AssignPicRoles()
            {
                for (int i = 0; i < ST_C_distanceList.Length; i++)
                {
                    if (!ST_C_GetIsSeated(i))
                    {
                        var t = ST_C_GetSeat(i);
                        ST_C_distanceList[i] = (_myPlayerPos - t.position).sqrMagnitude;
                    }
                    else
                    {
                        ST_C_distanceList[i] = float.MaxValue;
                    }
                }

                for (int i = 0; i < ST_C_sortIdxList.Length; i++)
                {
                    ST_C_sortIdxList[i] = -1;
                }

                for (int i = 0; i < ST_C_distanceList.Length; i++)
                {
                    if (ST_C_distanceList[i] == float.MaxValue)
                    {
                        continue;
                    }

                    var j = ST_C_sortIdxList.Length - 1;
                    if (ST_C_sortIdxList[j] != -1 && ST_C_distanceList[ST_C_sortIdxList[j]] <= ST_C_distanceList[i])
                    {
                        continue;
                    }
                    ST_C_sortIdxList[j] = i;
                    for (; j >= 1; j--)
                    {
                        var k = j - 1;
                        if (ST_C_sortIdxList[k] != -1 && ST_C_distanceList[ST_C_sortIdxList[k]] <= ST_C_distanceList[ST_C_sortIdxList[j]])
                        {
                            break;
                        }

                        var t = ST_C_sortIdxList[k];
                        ST_C_sortIdxList[k] = ST_C_sortIdxList[j];
                        ST_C_sortIdxList[j] = t;
                    }
                }

                for (int i = 0; i < ST_C_sortIdxList.Length; i++)
                {
                    var j = ST_C_sortIdxList[i];
                    if (j < 0)
                    {
                        continue;
                    }

                    if (j < WHL_Seats.Length)
                    {
                        WHL_hasPic[j] = true;
                        PIC_SetRole(i, PIC_Role.Wheel, j);
                        continue;
                    }

                    j -= WHL_Seats.Length;
                    if (j < JTC_Seats.Length)
                    {
                        JTC_hasPic[j] = true;
                        PIC_SetRole(i, PIC_Role.JetCoaster, j);
                        continue;
                    }
                }
            }

            void ST_C_UseDummyStation(PIC_Role role, int targetId)
            {
                STT_dummySeatRole[PIC_ROLE_IDX] = (int)role;
                STT_dummySeatRole[PIC_ROLE_TARGET_IDX] = targetId;
                PIDF_Start(ST_C_GetSeatAnchor(role, targetId));
            }

            /// <summary>
            /// STT: Station
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            [SerializeField] Transform STT_stationRoot;

            VRCStation[] STT_stations;
            StationController[] STT_stationControllers;
            int[][] STT_roles;
            int[] STT_dummySeatRole;

            void STT_Initialize()
            {
                STT_stations = new VRCStation[STT_stationRoot.childCount];
                STT_stationControllers = new StationController[STT_stationRoot.childCount];
                for (int i = 0; i < STT_stationRoot.childCount; i++)
                {
                    STT_stations[i] = STT_stationRoot.GetChild(i).GetComponent<VRCStation>();
                    STT_stationControllers[i] = STT_stations[i].GetComponent<StationController>();
                    STT_stationControllers[i].Initialize(this, i);
                }

                STT_roles = new int[STT_stations.Length][];
                for (int i = 0; i < STT_stations.Length; i++)
                {
                    STT_roles[i] = new int[PIC_ROLE_INFO_COUNT];
                    STT_roles[i][PIC_ROLE_IDX] = (int)PIC_Role.None;
                }

                STT_dummySeatRole = new int[PIC_ROLE_INFO_COUNT];
                STT_dummySeatRole[PIC_ROLE_IDX] = (int)PIC_Role.None;
            }

            void STT_Update()
            {
            }

            void STT_OnLeaveSeat(int i)
            {
                var role = (PIC_Role)STT_roles[i][PIC_ROLE_IDX];
                var targetId = STT_roles[i][PIC_ROLE_TARGET_IDX];

                STT_roles[i][PIC_ROLE_IDX] = (int)PIC_Role.None;

                ST_C_LeaveSeat(role, targetId);
                PIC_EnableAll();
            }

            void STT_OnLeaveDummySeat()
            {
                var role = (PIC_Role)STT_dummySeatRole[PIC_ROLE_IDX];
                var targetId = STT_dummySeatRole[PIC_ROLE_TARGET_IDX];

                STT_dummySeatRole[PIC_ROLE_IDX] = (int)PIC_Role.None;

                ST_C_LeaveSeat(role, targetId);
                PIC_EnableAll();
            }

            bool STT_UseStation(PIC_Role role, int targetId)
            {
                if (Networking.LocalPlayer == null)
                {
                    return false;
                }

                var statinId = GetEmptyStationId();
                if (statinId == -1)
                {
                    return false;
                }

                var seatAnchor = ST_C_GetSeatAnchor(role, targetId);
                STT_stations[statinId].transform.parent = seatAnchor;
                STT_stations[statinId].transform.localPosition = Vector3.zero;
                STT_stations[statinId].transform.localEulerAngles = Vector3.zero;
                STT_stations[statinId].UseStation(Networking.LocalPlayer);

                STT_roles[statinId][PIC_ROLE_IDX] = (int)role;
                STT_roles[statinId][PIC_ROLE_TARGET_IDX] = targetId;
                return true;
            }

            int GetEmptyStationId()
            {
                for (int i = 0; i < STT_stations.Length; i++)
                {
                    if (!STT_stationControllers[i].IsSeated)
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>
            /// PIC: Pickup
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const int PIC_ROLE_INFO_COUNT = 2;
            const int PIC_ROLE_IDX = 0, PIC_ROLE_TARGET_IDX = 1;

            [SerializeField] Transform PIC_pickupRoot;

            VRC_Pickup[] PIC_pickups;
            int[][] PIC_roles;
            bool[] PIC_isHeld;
            BoxCollider[] PIC_colliders;

            void PIC_Initialize()
            {
                PIC_pickups = new VRC_Pickup[PIC_pickupRoot.childCount];
                PIC_colliders = new BoxCollider[PIC_pickupRoot.childCount];
                for (int i = 0; i < PIC_pickupRoot.childCount; i++)
                {
                    PIC_pickups[i] = PIC_pickupRoot.GetChild(i).GetComponent<VRC_Pickup>();
                    PIC_pickups[i].gameObject.SetActive(false);
                    PIC_colliders[i] = PIC_pickups[i].GetComponent<BoxCollider>();
                }

                PIC_isHeld = new bool[PIC_pickups.Length];
                PIC_roles = new int[PIC_pickups.Length][];
                for (int i = 0; i < PIC_roles.Length; i++)
                {
                    PIC_isHeld[i] = false;
                    PIC_roles[i] = new int[PIC_ROLE_INFO_COUNT];
                    PIC_roles[i][PIC_ROLE_IDX] = (int)PIC_Role.None;
                }
            }

            void PIC_Update()
            {
                PIC_CheckState();
            }

            void PIC_CheckState()
            {
                for (int i = 0; i < PIC_pickups.Length; i++)
                {
                    if (PIC_pickups[i].IsHeld != PIC_isHeld[i])
                    {
                        PIC_isHeld[i] = PIC_pickups[i].IsHeld;

                        if (PIC_isHeld[i])
                        {
                            PIC_OnHeld(i);
                        }
                    }
                }
            }

            void PIC_OnHeld(int i)
            {
                ST_C_UseSeat(i, (PIC_Role)PIC_roles[i][PIC_ROLE_IDX], PIC_roles[i][PIC_ROLE_TARGET_IDX]);
            }

            void PIC_Release(int i)
            {
                PIC_pickups[i].Drop();
                PIC_isHeld[i] = false;
            }

            void PIC_SetRole(int pickId, PIC_Role role, int targetId)
            {
                PIC_roles[pickId][PIC_ROLE_IDX] = (int)role;
                PIC_roles[pickId][PIC_ROLE_TARGET_IDX] = targetId;

                var anchor = ST_C_GetSeatAnchor(role, targetId);
                PIC_pickups[pickId].transform.parent = anchor;
                PIC_pickups[pickId].transform.localPosition = Vector3.zero;
                PIC_pickups[pickId].transform.localEulerAngles = Vector3.zero;
                PIC_pickups[pickId].gameObject.SetActive(true);
            }

            void PIC_RemoveRole(int pickId)
            {
                PIC_RemoveRole(pickId, PIC_Role.None, 0);
            }

            void PIC_RemoveRole(int pickId, PIC_Role roleBefore, int targetIdBefore)
            {
                PIC_roles[pickId][PIC_ROLE_IDX] = (int)PIC_Role.None;
                PIC_pickups[pickId].transform.parent = null;
                PIC_pickups[pickId].gameObject.SetActive(false);

                switch (roleBefore)
                {
                    case PIC_Role.JetCoaster:
                        {
                            JTC_hasPic[targetIdBefore] = false;
                            return;
                        }

                    case PIC_Role.Wheel:
                        {
                            WHL_hasPic[targetIdBefore] = false;
                            return;
                        }
                }
            }

            void PIC_EnableAll()
            {
                PIC_SetEnableAll(true);
            }

            void PIC_DisableAll()
            {
                PIC_SetEnableAll(false);
            }

            void PIC_SetEnableAll(bool active)
            {
                for (int i = 0; i < PIC_pickups.Length; i++)
                {
                    PIC_colliders[i].enabled = active;
                }
            }

            /// <summary>
            /// JTC_C: Jet Coaster Controller
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const int JTC_C_startSec = 0;

            void JTC_C_Initialize()
            {
                JTC_Initialize();

                CTMI_U_SetForJetCoaster(0);
            }

            void JTC_C_Update()
            {
                JTC_Update();
            }

            void JTC_C_OnSecChanged()
            {
                if (TM_iSec == JTC_C_startSec)
                {
                    JTC_Start();
                }
            }

            /// <summary>
            /// JTC: Jet Coaster
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const string JTC_STATE_LABEL_FREE_FALL = "FreeFall_";
            const string JTC_STATE_LABEL_DECELERATE = "Decelerate_";

            [SerializeField] Transform JTC_anchorRoot;
            [SerializeField] Transform JTC_coasterRoot;
            [SerializeField] Transform[] JTC_Seats;

            float[] JTC_pointLengthList;
            float[][] JTC_pointLengthTTable;
            Transform[] JTC_anchors;
            Vector3[] JTC_subAnchorPoints;
            Transform[] JTC_coasters;
            float JTC_speedRate = 0.23f;
            float JTC_carrySpeed = 0.4f;
            float JTC_decelerationValue = 0.8f;
            int JTC_currentAnchorIdx, JTC_nextAnchorIdx;
            float JTC_v;
            float JTC_maxHeight;
            float JTC_x, JTC_distance;
            Vector3 JTC_currentPos, JTC_nextPos;
            JTC_CoasterState JTC_coasterState;
            int JTC_startIdx, JTC_carryIdx, JTC_freeFallIdx, JTC_decelerateIdx;
            float JTC_coasterDistance = 0.25f;
            bool[] JTC_isSeated;
            bool[] JTC_hasPic;

            void JTC_Initialize()
            {
                JTC_anchors = new Transform[JTC_anchorRoot.childCount];
                for (int i = 0; i < JTC_anchorRoot.childCount; i++)
                {
                    var child = JTC_anchorRoot.GetChild(i);
                    JTC_anchors[i] = child;
                    JTC_maxHeight = Mathf.Max(JTC_maxHeight, child.position.y);
                    if (i == 0)
                    {
                        JTC_startIdx = i;
                        JTC_carryIdx = i;
                    }
                    else if (child.name.StartsWith(JTC_STATE_LABEL_FREE_FALL))
                    {
                        JTC_freeFallIdx = i;
                    }
                    else if (child.name.StartsWith(JTC_STATE_LABEL_DECELERATE))
                    {
                        JTC_decelerateIdx = i;
                    }
                }
                JTC_maxHeight += 0.01f;

                JTC_subAnchorPoints = new Vector3[JTC_anchors.Length];
                for (int i = 0; i < JTC_anchors.Length; i++)
                {
                    int j = (i + 1) % JTC_anchors.Length;
                    JTC_subAnchorPoints[i] = (JTC_anchors[i].position + JTC_anchors[j].position) * 0.5f;
                }

                JTC_pointLengthList = new float[JTC_anchors.Length];
                JTC_pointLengthTTable = new float[JTC_anchors.Length][];

                JTC_coasters = new Transform[JTC_coasterRoot.childCount];
                for (int i = 0; i < JTC_coasterRoot.childCount; i++)
                {
                    JTC_coasters[i] = JTC_coasterRoot.GetChild(i);
                }

                JTC_isSeated = new bool[JTC_Seats.Length];
                JTC_hasPic = new bool[JTC_Seats.Length];
                for (int i = 0; i < JTC_Seats.Length; i++)
                {
                    JTC_isSeated[i] = false;
                    JTC_hasPic[i] = false;
                }

                JTC_x = 0;
                JTC_v = 0;
                JTC_coasterState = JTC_CoasterState.Wait;

                JTC_currentAnchorIdx = 0;
                JTC_CalcStep();

                JTC_UpdateCoasterPosition();
            }

            void JTC_Update()
            {
                switch (JTC_coasterState)
                {
                    case JTC_CoasterState.Wait:
                        {
                            return;
                        }

                    case JTC_CoasterState.Carried:
                    case JTC_CoasterState.FreeFall:
                    case JTC_CoasterState.Deceleration:
                        {
                            JTC_Move();
                            return;
                        }
                }

            }

            void JTC_Start()
            {
                if (JTC_coasterState == JTC_CoasterState.Wait)
                {
                    JTC_coasterState = JTC_CoasterState.Carried;

                    CTMI_U_SetForJetCoaster(1);
                }
            }

            float JTC_GetSpeedAcc()
            {
                return JTC_GetSpeed();
            }

            float JTC_GetSpeed()
            {
                float speed = 0;
                switch (JTC_coasterState)
                {
                    case JTC_CoasterState.Carried:
                        {
                            speed = JTC_carrySpeed;
                            break;
                        }

                    case JTC_CoasterState.FreeFall:
                        {
                            speed = Mathf.Sqrt(Mathf.Abs(JTC_maxHeight - JTC_coasters[0].position.y) * 9.8f * 2) * JTC_speedRate;
                            break;
                        }

                    case JTC_CoasterState.Deceleration:
                        {
                            speed = Mathf.Max(JTC_v - JTC_decelerationValue * Time.deltaTime, 0.1f);
                            break;
                        }
                }
                return Mathf.Max(speed, 0.1f);
            }

            void JTC_Move()
            {
                JTC_v = JTC_GetSpeedAcc();
                JTC_x += JTC_v * Time.deltaTime;
                var diff = JTC_distance - JTC_x;

                while (diff <= 0)
                {
                    JTC_x = -diff;

                    JTC_currentAnchorIdx = JTC_nextAnchorIdx;
                    JTC_CalcStep();

                    diff = JTC_distance - JTC_x;
                }
                JTC_UpdateCoasterPosition();

            }

            void JTC_CalcStep()
            {
                JTC_nextAnchorIdx = (JTC_currentAnchorIdx + 1) % JTC_anchors.Length;
                JTC_currentPos = JTC_subAnchorPoints[JTC_currentAnchorIdx];
                JTC_nextPos = JTC_subAnchorPoints[JTC_nextAnchorIdx];
                JTC_distance = JTC_CalcOrGetLength(JTC_currentAnchorIdx);

                JTC_UpdateState();
            }

            void JTC_UpdateState()
            {
                switch (JTC_coasterState)
                {
                    case JTC_CoasterState.Carried:
                        {
                            if (JTC_currentAnchorIdx == JTC_freeFallIdx)
                            {
                                JTC_coasterState = JTC_CoasterState.FreeFall;
                            }
                            return;
                        }

                    case JTC_CoasterState.FreeFall:
                        {
                            if (JTC_currentAnchorIdx == JTC_decelerateIdx)
                            {
                                JTC_coasterState = JTC_CoasterState.Deceleration;
                            }
                            return;
                        }

                    case JTC_CoasterState.Deceleration:
                        {
                            if (JTC_currentAnchorIdx == JTC_startIdx)
                            {
                                JTC_coasterState = JTC_CoasterState.Wait;

                                JTC_OnStateChangedToWait();
                            }
                            return;
                        }
                }
            }

            void JTC_OnStateChangedToWait()
            {
                CTMI_U_SetForJetCoaster(0);
            }

            float JTC_GetTime(int i, float x)
            {
                var step = 1 / (float)(JTC_pointLengthTTable[i].Length);
                for (int j = JTC_pointLengthTTable[i].Length - 1; j >= 0; j--)
                {
                    var l = JTC_pointLengthTTable[i][j];
                    if (l > x)
                    {
                        continue;
                    }
                    if (j == JTC_pointLengthTTable[i].Length - 1)
                    {
                        return 1;
                    }
                    var cDiff = x - l;
                    var diff = JTC_pointLengthTTable[i][j + 1] - l;
                    var r = cDiff / diff;
                    return (j + 1) * step + r * step;
                }

                return x / JTC_pointLengthTTable[i][0] * step;
            }

            void JTC_UpdateCoasterPosition()
            {
                float t = JTC_GetTime(JTC_currentAnchorIdx, JTC_x);
                var dir = JTC_CalcDir(JTC_currentAnchorIdx, t);

                JTC_coasters[0].position = JTC_CalcPosition(JTC_currentAnchorIdx, t);
                JTC_coasters[0].rotation = Quaternion.LookRotation(dir);

                var diff = JTC_x;
                var _JTC_currentAnchorIdx = JTC_currentAnchorIdx;
                var _JTC_nextAnchorIdx = JTC_nextAnchorIdx;
                var _JTC_preAnchorIdx = (JTC_currentAnchorIdx + JTC_anchors.Length - 1) % JTC_anchors.Length;

                for (int i = 1; i < JTC_coasters.Length; i++)
                {
                    var _JTC_distance = diff;
                    var _JTC_x = JTC_coasterDistance;
                    diff = _JTC_distance - _JTC_x;

                    while (diff <= 0)
                    {
                        _JTC_x = -diff;

                        _JTC_nextAnchorIdx = _JTC_currentAnchorIdx;
                        _JTC_currentAnchorIdx = _JTC_preAnchorIdx;
                        _JTC_preAnchorIdx = (_JTC_currentAnchorIdx + JTC_anchors.Length - 1) % JTC_anchors.Length;
                        _JTC_distance = JTC_CalcOrGetLength(_JTC_currentAnchorIdx);
                        diff = _JTC_distance - _JTC_x;
                    }

                    t = JTC_GetTime(_JTC_currentAnchorIdx, diff);
                    dir = JTC_CalcDir(_JTC_currentAnchorIdx, t);
                    JTC_coasters[i].position = JTC_CalcPosition(_JTC_currentAnchorIdx, t);
                    JTC_coasters[i].rotation = Quaternion.LookRotation(dir);
                }
            }

            float JTC_CalcOrGetLength(int i)
            {
                if (JTC_pointLengthList[i] <= 0)
                {
                    JTC_pointLengthList[i] = JTC_CalcLength(i);
                }

                return JTC_pointLengthList[i];
            }

            float JTC_CalcLength(int i)
            {
                int divCount = 10;
                float length = 0;
                Vector3 prePosition = Vector3.zero;
                JTC_pointLengthTTable[i] = new float[divCount];
                for (int j = 0; j <= divCount; j++)
                {
                    float t = j / (float)divCount;
                    var p = JTC_CalcPosition(i, t);
                    if (j != 0)
                    {
                        length += (p - prePosition).magnitude;
                        JTC_pointLengthTTable[i][j - 1] = length;
                    }
                    prePosition = p;
                }
                return length;
            }

            Vector3 JTC_CalcPosition(int i, float t)
            {
                i %= JTC_anchors.Length;
                int j = (i + 1) % JTC_anchors.Length;
                float t1 = 1 - t;
                return t1 * t1 * JTC_subAnchorPoints[i] + 2 * t1 * t * JTC_anchors[j].position + t * t * JTC_subAnchorPoints[j];
            }

            Vector3 JTC_CalcDir(int i, float t)
            {
                i %= JTC_anchors.Length;
                int j = (i + 1) % JTC_anchors.Length;
                return 2 * (t - 1) * JTC_subAnchorPoints[i] + 2 * (1 - 2 * t) * JTC_anchors[j].position + 2 * t * JTC_subAnchorPoints[j];
            }

            /// <summary>
            /// PIDF: PID Following
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            Transform PIDF_targetTransform;
            bool PIDF_isFollowing;
            float PIDF_Kp = 40, /*_Ki = 0,*/ PIDF_Kd = 8;
            Vector3 PIDF_error, PIDF_lastError, PIDF_p, /*_i,*/ PIDF_d, PIDF_pidValue;

            void PIDF_Initialize()
            {
                PIDF_Reset();
            }

            void PIDF_Update()
            {
                if (!PIDF_isFollowing || PIDF_targetTransform == null)
                {
                    return;
                }

                PIDF_CalcStep();
                if (Networking.LocalPlayer != null)
                {
                    Networking.LocalPlayer.SetVelocity(PIDF_pidValue);
                }
            }

            void PIDF_Start(Transform target)
            {
                PIDF_targetTransform = target;
                PIDF_isFollowing = true;
            }

            void PIDF_CalcStep()
            {
                PIDF_error = PIDF_targetTransform.position - _myPlayerPos;
                PIDF_p = PIDF_error;
                /*_i += _error;*/
                PIDF_d = PIDF_error - PIDF_lastError;
                PIDF_lastError = PIDF_error;
                PIDF_pidValue = PIDF_p * PIDF_Kp + /*_i * _Ki +*/ PIDF_d * PIDF_Kd;
            }

            void PIDF_Reset()
            {
                PIDF_isFollowing = false;
                PIDF_lastError = Vector3.zero;
                /*_i += Vector3.zero;*/
                if (Networking.LocalPlayer != null)
                {
                    Networking.LocalPlayer.SetVelocity(Vector3.zero);
                }
            }

            void PIDF_OnBoothExit()
            {
                if (PIDF_isFollowing)
                {
                    STT_OnLeaveDummySeat();
                    PIDF_Reset();
                }
            }

            /// <summary>
            /// WHL: Wheel
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const float WHL_SPEED = -8;

            [SerializeField] Transform WHL_wheel;
            [SerializeField] Transform[] WHL_Seats;
            [SerializeField] Transform WHL_gondolaRoot;

            Vector3 WHL_wheelAngles;
            bool[] WHL_isSeated;
            bool[] WHL_hasPic;
            Transform[] WHL_gondolas;

            void WHL_Initialize()
            {
                WHL_isSeated = new bool[WHL_Seats.Length];
                WHL_hasPic = new bool[WHL_Seats.Length];
                for (int i = 0; i < WHL_Seats.Length; i++)
                {
                    WHL_isSeated[i] = false;
                    WHL_hasPic[i] = false;
                }

                WHL_gondolas = new Transform[WHL_gondolaRoot.childCount];
                for (int i = 0; i < WHL_gondolaRoot.childCount; i++)
                {
                    WHL_gondolas[i] = WHL_gondolaRoot.GetChild(i);
                }
            }

            void WHL_Update()
            {
                WHL_wheelAngles = WHL_wheel.localEulerAngles;
                WHL_wheelAngles.y = (TM_fTotalSec % 360) * WHL_SPEED;
                WHL_wheel.localEulerAngles = WHL_wheelAngles;

                float gondolaAngle = WHL_wheelAngles.y + 90;
                for (int i = 0; i < WHL_gondolas.Length; i++)
                {
                    var gondola = WHL_gondolas[i];
                    var angles = gondola.localEulerAngles;
                    angles.y = gondolaAngle;
                    gondola.localEulerAngles = angles;
                }
            }

            /// <summary>
            /// TM: Time
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            float TM_fTotalSec, TM_fSec;
            int TM_min, TM_iSec, TM_preMin, TM_preISec;

            void TM_Initialize()
            {
                TM_UpdateTime();
            }

            void TM_Update()
            {
                if (TM_preMin != TM_min)
                {
                    TM_preMin = TM_min;
                    if (TM_min == 0)
                    {
                        TM_OnOurChanged();
                    }
                    TM_OnMinChanged();
                }

                if (TM_preISec != TM_iSec)
                {
                    TM_preISec = TM_iSec;
                    TM_OnSecChanged();
                }

                TM_UpdateTime();
            }

            void TM_OnBoothEnter()
            {
                TM_UpdateTime();
            }

            void TM_UpdateTime()
            {
                TM_fTotalSec = (float)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds % 3600);
                TM_min = (int)(TM_fTotalSec / 60);
                TM_fSec = TM_fTotalSec % 60;
                TM_iSec = (int)TM_fSec;
            }

            void TM_OnOurChanged()
            {
            }

            void TM_OnMinChanged()
            {
            }

            void TM_OnSecChanged()
            {
                JTC_C_OnSecChanged();
            }

            /// <summary>
            /// ASC: Avatar Scalling Control
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            const float ASC_AVATAR_SCALE_FOR_PLAY = 0.22f;
            const float ASC_AVATAR_SCALE_CHANGE_DISTANCE = 3.0f;

            float ASC_defaultAvatarScale;
            ASC_AvatarEyeHeightChangeState ASC_eyeHeightChangeState;

            void ASC_Initialize()
            {
                ASC_defaultAvatarScale = 0;
                ASC_eyeHeightChangeState = ASC_AvatarEyeHeightChangeState.OutOfBooth;
            }

            void ASC_Update()
            {
                switch (ASC_eyeHeightChangeState)
                {
                    case ASC_AvatarEyeHeightChangeState.OutOfBooth:
                        {
                            return;
                        }
                    case ASC_AvatarEyeHeightChangeState.InBoothNotChnaged:
                        {
                            if (ASC_IsInAvatarChangeRange())
                            {
                                ASC_eyeHeightChangeState = ASC_AvatarEyeHeightChangeState.Changed;
                                ASC_SetAvatarScaleForPlay();
                            }
                            return;
                        }
                    case ASC_AvatarEyeHeightChangeState.Changed:
                        {
                            return;
                        }
                }
            }

            bool ASC_IsInAvatarChangeRange()
            {
                return Mathf.Abs(_boothRoot.position.x - _myPlayerPos.x) <= ASC_AVATAR_SCALE_CHANGE_DISTANCE && Mathf.Abs(_boothRoot.position.z - _myPlayerPos.z) <= ASC_AVATAR_SCALE_CHANGE_DISTANCE;
            }

            void ASC_OnBoothEnter()
            {
                ASC_eyeHeightChangeState = ASC_AvatarEyeHeightChangeState.InBoothNotChnaged;
            }

            void ASC_OnBoothExit()
            {
                ASC_RestoreAvatarScale();
                ASC_eyeHeightChangeState = ASC_AvatarEyeHeightChangeState.OutOfBooth;
            }

            void ASC_SetAvatarScaleForPlay()
            {
                if (ASC_defaultAvatarScale <= 0 && Networking.LocalPlayer != null)
                {
                    ASC_defaultAvatarScale = Networking.LocalPlayer.GetAvatarEyeHeightAsMeters();
                }

                ASC_ChangeAvatarScale(ASC_AVATAR_SCALE_FOR_PLAY);
            }

            void ASC_RestoreAvatarScale()
            {
                if (Networking.LocalPlayer == null)
                {
                    return;
                }

                var currentScale = Networking.LocalPlayer.GetAvatarEyeHeightAsMeters();

                if (Mathf.Approximately(currentScale, ASC_AVATAR_SCALE_FOR_PLAY))
                {
                    ASC_ChangeAvatarScale(ASC_defaultAvatarScale);
                }
            }

            void ASC_ChangeAvatarScale(float scale)
            {
                if (scale <= 0 || Networking.LocalPlayer == null)
                {
                    return;
                }

                var currentScale = Networking.LocalPlayer.GetAvatarEyeHeightAsMeters();

                if (Mathf.Approximately(currentScale, scale))
                {
                    return;
                }


                Networking.LocalPlayer.SetAvatarEyeHeightByMeters(scale);
            }

            void ACT_Update()
            {
            }

            /// <summary>
            /// DBG: Debug
            /// 
            /// 
            /// 
            /// 
            /// 
            /// </summary>

            bool DBG_isDebugModeA;
            int DBG_debugModeAStep;
            int[] DBG_debugModeACodes;

            void DBG_Initialize()
            {
                DBG_debugModeACodes = new int[]
                {
                        (int)KeyCode.S,
                        (int)KeyCode.W,
                        (int)KeyCode.D,
                        (int)KeyCode.E,
                        (int)KeyCode.B,
                        (int)KeyCode.U,
                        (int)KeyCode.G,
                };
                DBG_isDebugModeA = false;
                DBG_debugModeAStep = 0;
            }

            void DBG_Update()
            {
                DBG_CheckCommands();
            }

            void DBG_CheckCommands()
            {
                if (Input.anyKeyDown)
                {
                    if (Input.GetKeyDown((KeyCode)DBG_debugModeACodes[DBG_debugModeAStep]))
                    {
                        DBG_debugModeAStep++;
                        if (DBG_debugModeAStep >= DBG_debugModeACodes.Length)
                        {
                            DBG_debugModeAStep = 0;
                            DBG_isDebugModeA = !DBG_isDebugModeA;
                        }
                    }
                    else
                    {
                        DBG_debugModeAStep = 0;
                    }
                }
            }
        }
    }
}
