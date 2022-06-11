using LevelGeneration;
using Player;
using SNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GTFO.API;
using GTFO.API.Wrappers;
using GameData;

namespace GregRundownCore
{
    class SpleefManager
    {
        public static void Setup()
        {
            LG_Factory.add_OnFactoryBuildDone((Action)SetupSpleef);
            Patch.OnPlayerDowned += OnPlayerDead;
        }

        public static void SetupSpleef()
        {
            if (RundownManager.ActiveExpedition.LevelLayoutData != 2012) return;
            s_BreakableProps.Clear();
            s_RoundCounter = 0;
            s_RoundOver = false;

            var exitScanAlign = LG_LevelBuilder.Current.m_currentFloor.allZones[0].m_areas[0].transform.FindChild("ExpeditionExitScanAlign").gameObject;
            exitScanAlign.transform.localPosition = Vector3.one * -10000;
            exitScanAlign.SetActive(false);
              
            var tile = LG_LevelBuilder.Current.m_currentFloor.m_dimensions[1].GetStartTile();
            var platform = tile.m_geoRoot.m_areas[0].transform.FindChild("EnvProps/Arena/Platform");
            s_Barrier = tile.m_geoRoot.transform.FindChild("storm").gameObject.AddComponent<ShrinkingBarrier>();

            for (var i = 0; i < platform.childCount; i++)
            {
                var plat = platform.GetChild(i).gameObject;

                s_BreakableProps.Add(plat.transform.FindChild("prop").gameObject);
                var comp = plat.transform.FindChild("prop/c_BreakablePlat").gameObject.AddComponent<CuttingTorchScreen>();
                comp.m_projectedScreen = new(i.ToString());
            }
        }

        public static void OnPlayerDead()
        {
            if (s_RoundOver == true) return;
            List<PlayerAgent> alivePlayers = new();

            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                if (player.Locomotion.m_currentStateEnum != PlayerLocomotion.PLOC_State.Downed) alivePlayers.Add(player);
            }

            if (alivePlayers.Count == 1)
            {
                GameScoreManager.IncrementScore(alivePlayers[0]);
                CoroutineHandler.Add(ResetArena());
            }
            else if (alivePlayers.Count == 0) CoroutineHandler.Add(ResetArena());

        }

        public static void OnPlatformBreakPacket(ulong x, int index)
        {
            s_BreakableProps[index].SetActive(false);
        }

        public static void DisplayRoundCount()
        {
            GameScoreManager.Current.m_ObjectiveTimer.m_timerText.SetText($"[ROUND {s_RoundCounter + 1}]\nDROP PRISONERS INTO THE PIT. LAST MAN STANDING WINS!");
            GameScoreManager.Current.m_ObjectiveTimer.m_titleText.SetText("");
            GameScoreManager.Current.m_DisplayTimer = Time.time + 5;
        }

        public static IEnumerator ResetArena()
        {
            s_RoundOver = true;
            GameScoreManager.Current.UpdateScore();

            var localPlayer = PlayerManager.GetLocalPlayerAgent();
            var playerBackpack = PlayerBackpackManager.GetBackpack(localPlayer.Owner);

            s_RoundCounter++;
            if (s_RoundCounter >= s_RoundLimit)
            {
                playerBackpack.TryClearSlot(InventorySlot.GearStandard);
                playerBackpack.TryClearSlot(InventorySlot.GearSpecial);
            }

            yield return new WaitForSeconds(1);
            PlayerManager.GetLocalPlayerAgent().Sound.Post(1078008235);

            yield return new WaitForSeconds(5);
            s_RoundOver = false;

            if (s_RoundCounter >= s_RoundLimit)
            {
                WardenObjectiveManager.Current.AttemptInteract(new pWardenObjectiveInteraction
                {
                    type = eWardenObjectiveInteractionType.SolveWardenObjectiveItem
                });
                WardenObjectiveManager.OnWinConditionSolved(LG_LayerType.MainLayer);
            }

            else 
            {
                foreach (var prop in s_BreakableProps) prop.SetActive(true);
                s_Barrier.Reset();
                localPlayer.TryWarpTo(eDimensionIndex.Dimension_1, new((float)(s_Rand.NextDouble() - 0.5f) * 55, 172.5619f, (float)(s_Rand.NextDouble() - 0.5f) * 55), localPlayer.TargetLookDir);
                DisplayRoundCount();
            }
        }

        public static List<GameObject> s_BreakableProps = new();
        public static ShrinkingBarrier s_Barrier;
        public static System.Random s_Rand = new();
        public static int s_RoundCounter = 0;
        public static int s_RoundLimit = 10;
        public static bool s_RoundOver = false;
    }
}
