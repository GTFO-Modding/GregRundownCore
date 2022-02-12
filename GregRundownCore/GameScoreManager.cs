using GTFO.API;
using LevelGeneration;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    class GameScoreManager : MonoBehaviour
    {
        public GameScoreManager(IntPtr value) : base(value)
        { }

        public void Awake()
        {
            m_Crown = GameObject.Instantiate<GameObject>(AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/Crown.prefab").TryCast<GameObject>());
            m_Confetti = GameObject.Instantiate<ParticleSystem>(AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/Confetti.prefab").TryCast<GameObject>().GetComponent<ParticleSystem>());
            m_Crown.active = false;
            m_Confetti.gameObject.active = false;
            Current = this;
            Patch.CollectedSmallPickup += SyncUpdateScore;
            Patch.OnLevelCleanup += Cleanup;
        }

        public void Update()
        {
            if (m_DisplayTimer > Time.time)
            {
                if (!m_ObjectiveTimer.gameObject.active) m_ObjectiveTimer.gameObject.active = true;
                m_ObjectiveTimer.m_timerText.color = LevelLightManager.Current.m_EaseRGB_Fast;
            }

            else if (m_ObjectiveTimer.gameObject.active) m_ObjectiveTimer.gameObject.active = false;
        }

        public void SyncUpdateScore()
        {
            UpdateScore();
            NetworkAPI.InvokeEvent("SmallPickupCollected", 0);
        }

        public static void SyncRecieveUpdateScore(ulong x, byte y)
        {
            GameScoreManager.Current.UpdateScore();
        }

        public void UpdateScore()
        {
            if (m_AmbienceLightMem_Color == null) m_AmbienceLightMem_Color = PlayerManager.Current.m_localPlayerAgentInLevel.m_ambienceLight.color;
            if (m_AmbientPointMem_Scale == null) m_AmbientPointMem_Scale = PlayerManager.Current.m_localPlayerAgentInLevel.m_ambientPoint.m_lightScale;

            if (m_AmbientPointMem_Intensity == 0) m_AmbientPointMem_Intensity = PlayerManager.Current.m_localPlayerAgentInLevel.m_ambientPoint.m_intensity;
            if (m_AmbientPointMem_Range == 0) m_AmbientPointMem_Range = PlayerManager.Current.m_localPlayerAgentInLevel.m_ambientPoint.m_invRangeSqr;

            List<Patch.ScoreBoard> leaderboard = new();
            SNet_Player player;
            PlayerBackpack backpack;

            for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
            {
                player = SNet.Slots.SlottedPlayers[i];
                if (!player.IsInSlot) continue;

                backpack = PlayerBackpackManager.GetBackpack(player);

                var score = new Patch.ScoreBoard();
                score.Player = player;
                score.Score = backpack.CountPocketItem(WardenObjectiveManager.ActiveWardenObjective(LG_LayerType.MainLayer).Gather_ItemId);
                leaderboard.Add(score);
            }

            if (leaderboard.Count > 1)
            {
                leaderboard.Sort((s1, s2) => s1.Score.CompareTo(s2.Score));
                leaderboard.Reverse();
            }

            AssignCrown(PlayerManager.Current.GetPlayerAgentInSlot(leaderboard[0].Player.PlayerSlotIndex()));
        }
        
        public void AssignCrown(PlayerAgent player)
        {
            if (player == null) return;
            if (player == m_Leader) return;
            if (m_Leader == null) m_Leader = player;


            var enableDisplay = false;

            if (PlayerManager.Current.m_localPlayerAgentInLevel != player && PlayerManager.Current.m_localPlayerAgentInLevel == m_Leader)
            {
                m_ObjectiveTimer.m_timerText.SetText("LOST THE LEAD!");
                var localplayer = PlayerManager.Current.m_localPlayerAgentInLevel;
                localplayer.m_ambienceLight.color = m_AmbienceLightMem_Color;
                localplayer.m_ambientPoint.m_lightScale = m_AmbientPointMem_Scale;
                localplayer.m_ambientPoint.m_invRangeSqr = m_AmbientPointMem_Range;
                localplayer.m_ambientPoint.m_intensity = m_AmbientPointMem_Intensity;
                localplayer.m_ambientPoint.UpdateData();

                enableDisplay = true;
            }
            if (player == PlayerManager.Current.m_localPlayerAgentInLevel)
            {
                m_ObjectiveTimer.m_timerText.SetText("GAINED THE LEAD!");
                player.m_ambienceLight.color = new(1, 0.9f, 0.4f, 1);
                player.m_ambientPoint.SetRange(50);
                player.m_ambientPoint.m_intensity = 0.8f;
                player.Sound.Post(2763547111);
                enableDisplay = true;
            }

            m_Leader = player;

            //m_Confetti.gameObject.active = true;
            //m_Confetti.transform.position = player.Position;
            //m_Confetti.Play();

            m_Crown.active = true;
            m_Crown.transform.SetParent(player.transform.FindChild("PlayerCharacter_rig(Clone)/PlayerCharacter_rig/Root/Hip/Spine1/Spine2/Spine3/Neck/Head"));
            m_Crown.transform.localPosition = Vector3.zero;
            m_Crown.transform.localEulerAngles = new Vector3(0, 90, 270);
            m_ObjectiveTimer.m_titleText.SetText("");

            if (enableDisplay) m_DisplayTimer = Time.time + 5f;
        }
        
        public void Cleanup()
        {
            m_Crown.active = false;
            m_Confetti.gameObject.active = false;
            m_Score.Clear();
            m_Leader = null;
            m_DisplayTimer = 0;
        }

        public static GameScoreManager Current;
        public GameObject m_Crown;
        public ParticleSystem m_Confetti;
        public Dictionary<SNet_Player, int> m_Score = new();
        public PlayerAgent m_Leader;
        public float m_DisplayTimer;
        public PUI_ObjectiveTimer m_ObjectiveTimer = GuiManager.Current.m_playerLayer.m_objectiveTimer;

        public Color m_AmbienceLightMem_Color;
        public float m_AmbientPointMem_Range;
        public float m_AmbientPointMem_Intensity;
        public Vector3 m_AmbientPointMem_Scale;
    }
}
