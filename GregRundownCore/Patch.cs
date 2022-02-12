﻿using AK;
using CellMenu;
using Enemies;
using Globals;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Video;

namespace GregRundownCore
{
    [HarmonyPatch]
    class Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.OnLocalPlayerSolvedObjectiveItem))]
        public static void OnLocalPlayerSolvedObjectiveItem()
        {
            if (WardenObjectiveManager.ActiveWardenObjective(LG_LayerType.MainLayer).Type != eWardenObjectiveType.GatherSmallItems) return;
            CollectedSmallPickup?.Invoke();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GS_AfterLevel), nameof(GS_AfterLevel.Enter))]
        public static void Enter()
        {
            OnLevelCleanup?.Invoke();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.TryWarpTo))]
        public static void TryWarpTo()
        {
            OnPlayerWarped?.Invoke();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SurvivalWave), nameof(SurvivalWave.TryPlaySound))]
        public static void TryPlaySound(EnemyGroup group, SurvivalWave __instance)
        {
            PlayerManager.Current.m_localPlayerAgentInLevel.Sound.Post(1078008235);
            NetworkAPI.InvokeEvent("GregSpawned", 0);
        }
        public static void SyncRecieveApplause(ulong x, byte y)
        {
            PlayerManager.Current.m_localPlayerAgentInLevel.Sound.Post(1078008235);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PLOC_Downed), nameof(PLOC_Downed.Enter))]
        public static void DownedEnter()
        {
            PlayerManager.Current.m_localPlayerAgentInLevel.Sound.Post(384416095);
            OnPlayerDowned?.Invoke();
            NetworkAPI.InvokeEvent("PlayerDowned", 0);
        }
        public static void SyncRecieveGasp(ulong x, byte y)
        {
            PlayerManager.Current.m_localPlayerAgentInLevel.Sound.Post(384416095);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerStamina), nameof(PlayerStamina.Setup))]
        public static void SetupStamina(PlayerStamina __instance)
        {
            __instance.Enabled = false;
            __instance.enabled = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.UpdatePocketItemGUI))]
        public static bool UpdatePocketItemGUI(PlayerBackpackManager __instance) 
        {
            List<ScoreBoard> leaderboard = new();
            string first = "";
            string second = "";
            string third = "";
            string fourth = "";

            SNet_Player player;
            PlayerBackpack backpack;

            for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
            {
                player = SNet.Slots.SlottedPlayers[i];
                if (!player.IsInSlot) continue;

                backpack = PlayerBackpackManager.GetBackpack(player);

                var score = new ScoreBoard();
                score.Player = player;
                score.Score = backpack.CountPocketItem(WardenObjectiveManager.ActiveWardenObjective(LG_LayerType.MainLayer).Gather_ItemId);
                leaderboard.Add(score);
            }

            if (leaderboard.Count > 1)
            {
                leaderboard.Sort((s1, s2) => s1.Score.CompareTo(s2.Score));
                leaderboard.Reverse();
            }

            for (int i = 0; i < leaderboard.Count; i++)
            {
                switch (i) 
                {
                    case 0: first = $"<uppercase><color=green><u>1ST PLACE</u>//:<color=white> {leaderboard[0].Player.NickName} - {leaderboard[0].Score}</uppercase></color>\n"; break;
                    case 1: second = $"<uppercase><color=white><u>2ND PLACE</u>//:<color=white> {leaderboard[1].Player.NickName} - {leaderboard[1].Score}</uppercase></color>\n"; break;
                    case 2: third = $"<uppercase><color=white><u>3RD PLACE</u>//:<color=white> {leaderboard[2].Player.NickName} - {leaderboard[2].Score}</uppercase></color>\n"; break;
                    case 3: fourth = $"<uppercase><color=white><u>4TH PLACE</u>//:<color=white> {leaderboard[3].Player.NickName} - {leaderboard[3].Score}</uppercase></color>\n"; break;
                }
            }

            string result = first + second + third + fourth;

            GuiManager.PlayerLayer.UpdateObjectiveItems(result);
            GuiManager.MainMenuLayer.PageObjectives.UpdateObjectiveItems(result);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageExpeditionSuccess), nameof(CM_PageExpeditionSuccess.OnEnable))]
        public static void OnEnableExpeditionSuccess(CM_PageExpeditionSuccess __instance)
        {
            if (!__instance.m_isSetup) return;

            int winnerIndex = GameScoreManager.Current.m_Leader.Owner.PlayerSlotIndex();
            __instance.m_playerReports[winnerIndex].m_name.color = Color.green;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.Setup))]
        public static void CM_PageRundown_New_Setup(CM_PageRundown_New __instance)
        {
            __instance.m_buttonConnect.OnBtnPressCallback = null;
            __instance.m_buttonConnect.add_OnBtnPressCallback((Action<int>)((_) => GregsHouse.GregManagers.GetComponent<GlobalMusicManager>().Play("play_MenuOK")));
            __instance.m_buttonConnect.add_OnBtnPressCallback((Action<int>)((_) => GregsHouse.GregManagers.GetComponent<GlobalMusicManager>().Play("play_Song_Menu")));
            __instance.m_buttonConnect.add_OnBtnPressCallback((Action<int>)((_) => __instance.SetRundownFullyRevealed()));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.SetRundownFullyRevealed))]
        public static void SetRundownFullyRevealed(CM_PageRundown_New __instance)
        {
            CustomRundownPage.Setup(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GS_Lobby), nameof(GS_Lobby.Enter))]
        public static void GS_Lobby_Enter()
        {
            var music = GregsHouse.GregManagers.GetComponent<GlobalMusicManager>();
            if (!music.m_MenuThemePlaying) music.Play("play_Song_Menu");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CM_PageIntro), nameof(CM_PageIntro.SetPageActive))]
        public static bool SetPageActive(CM_PageIntro __instance, bool active)
        {
            __instance.m_isActive = active;
            __instance.gameObject.active = true;

            if (active)
            {
                if (Global.SkipIntro)
                {
                    __instance.m_step = CM_IntroStep.WaitForNetwork;
                    return false;
                }
                MusicManager.Machine.Sound.Post(EVENTS.START_SCREEN_ENTER, true);
                if (Global.ShowStartupScreen)
                {
                    //__instance.PrepareForStartupScreen();
                    return false;
                }
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageIntro), nameof(CM_PageIntro.OnSkip))]
        public static void OnSkip()
        {
            CM_PageBase.PostSound(1837763790);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
        public static void CM_PageRundown_New_OnEnable()
        {
            MainMenuGuiLayer.Current.PageIntro.gameObject.active = false;
        }


        public static event Action CollectedSmallPickup;
        public static event Action OnLevelCleanup;
        public static event Action OnPlayerWarped;
        public static event Action OnPlayerDowned;

        public struct ScoreBoard
        {
            public int Score;
            public SNet_Player Player;
        }
    }
}
