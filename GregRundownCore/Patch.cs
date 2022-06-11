using AK;
using AssetShards;
using CellMenu;
using Enemies;
using Feedback;
using FX_EffectSystem;
using GameData;
using Globals;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using Player;
using SNetwork;
using System;
using System.Collections;
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
        [HarmonyPatch(typeof(PLOC_Downed), nameof(PLOC_Downed.CommonEnter))]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.CheckExpeditionFailed))]
        public static void CheckExpeditionFailed(ref bool __result)
        {
            if (WardenObjectiveManager.ActiveWardenObjective(LG_LayerType.MainLayer).Type == eWardenObjectiveType.ClearAPath) __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WardenObjectiveManager._ExcecuteEvent_d__137), nameof(WardenObjectiveManager._ExcecuteEvent_d__137.MoveNext))]
        public static void ExecuteEvent_MoveNext(WardenObjectiveManager._ExcecuteEvent_d__137 __instance)
        {
            if (__instance.__1__state != -1) return;

            switch ((int)__instance.eData.Type)
            {
                case 100: L.Debug("Spleef mode: populating floor with greg");

                    if (SNet.IsMaster)
                    {
                        for (var i = 0; i < 30; i++)
                        {
                            EnemyAllocator.Current.SpawnEnemy(2001, PlayerManager.GetLocalPlayerAgent().CourseNode, Agents.AgentMode.Agressive, new(0, 144.1885f, 0), new(0, 0, 0, 0));
                        }
                    }

                    foreach (var player in PlayerManager.PlayerAgentsInLevel)
                    {
                        PlayerBackpackManager.GetBackpack(player.Owner).SpawnAndEquipItem(1000, InventorySlot.GearStandard).GearIDRange = new("{\"Ver\":1,\"Name\":\"SpleefBomb\",\"Packet\":{\"Comps\":{\"Length\":5,\"a\":{\"c\":2,\"v\":15},\"b\":{\"c\":3,\"v\":53},\"c\":{\"c\":4,\"v\":17},\"d\":{\"c\":5,\"v\":28}},\"MatTrans\":{\"tDecalA\":{\"scale\":0.1},\"tDecalB\":{\"scale\":0.1},\"tPattern\":{\"scale\":0.1}},\"publicName\":{\"data\":\"SpleefBomb\"}}}");
                        PlayerBackpackManager.GetBackpack(player.Owner).SpawnAndEquipItem(1001, InventorySlot.GearSpecial).GearIDRange = new("{\"Ver\":1,\"Name\":\"SpleefBomb\",\"Packet\":{\"Comps\":{\"Length\":5,\"a\":{\"c\":2,\"v\":15},\"b\":{\"c\":3,\"v\":53},\"c\":{\"c\":4,\"v\":17},\"d\":{\"c\":5,\"v\":28}},\"MatTrans\":{\"tDecalA\":{\"scale\":0.1},\"tDecalB\":{\"scale\":0.1},\"tPattern\":{\"scale\":0.1}},\"publicName\":{\"data\":\"SpleefBomb\"}}}");
                        PlayerBackpackManager.ForceSyncLocalInventory();
                        PlayerBackpackManager.GetBackpack(player.Owner).AmmoStorage.ConsumableAmmo.AmmoInPack = 999999;
                    }

                    var localplayer = PlayerManager.GetLocalPlayerAgent();
                    localplayer.PlayerData.crouchMoveSpeed *= 1.5f;
                    localplayer.PlayerData.walkMoveSpeed *= 1.5f;
                    localplayer.PlayerData.runMoveSpeed *= 1.5f;
                    localplayer.PlayerData.airMoveSpeed *= 1.5f;

                    SpleefManager.s_Barrier.m_DoShrink = true;
                    CoroutineHandler.Add(EquipRoutine());

                    break;

                default: return;
            }
        }

        public static IEnumerator EquipRoutine()
        {
            yield return new WaitForSeconds(1);
            PlayerManager.GetLocalPlayerAgent().Sync.WantsToWieldSlot(InventorySlot.GearStandard);
            SpleefManager.DisplayRoundCount();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GlowstickInstance), nameof(GlowstickInstance.OnCollisionEnter))]
        public static void GlowStickInstance_OnCollisionEnter(GlowstickInstance __instance, Collision col)
        {
            if (__instance.ItemDataBlock.persistentID != 1000) return;

            PlayerBackpackManager.GetBackpack(PlayerManager.GetLocalPlayerAgent().Owner).AmmoStorage.ConsumableAmmo.AmmoInPack = 999999;
            var comp = col.collider.GetComponent<CuttingTorchScreen>();

            if (comp != null)
            {
                col.collider.transform.parent.gameObject.SetActive(false);
                NetworkAPI.InvokeEvent("SpleefPlatBroken", int.Parse(comp.m_projectedScreen.name));

                CellSound.Post(AK.EVENTS.FRAGGRENADEEXPLODE, __instance.transform.position);
                var vfxExplosion = SpleefBombFX.AquireEffect();
                vfxExplosion.Play(null, __instance.transform.position, Quaternion.LookRotation(Vector3.up));

                if (SNet.IsMaster) __instance.ReplicationWrapper.Replicator.Despawn();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GlowstickInstance), nameof(GlowstickInstance.Update))]
        public static void GlowStickInstance_Update(GlowstickInstance __instance)
        {
            if (__instance.ItemDataBlock.persistentID != 1001 || __instance.m_state != eFadeState.One) return;

            PlayerBackpackManager.GetBackpack(PlayerManager.GetLocalPlayerAgent().Owner).AmmoStorage.ConsumableAmmo.AmmoInPack = 999999;

            if (Physics.Linecast(__instance.transform.position, __instance.transform.position + Vector3.down, out var hitInfo, LayerManager.MASK_WORLD) && hitInfo.collider.GetComponent<CuttingTorchScreen>() != null)
            {
                hitInfo.collider.transform.parent.gameObject.SetActive(false);
            }

            CellSound.Post(AK.EVENTS.FRAGGRENADEEXPLODE, __instance.transform.position);
            var vfxExplosion = SpleefBombFX.AquireEffect();
            vfxExplosion.Play(null, __instance.transform.position, Quaternion.LookRotation(Vector3.up));

            if (SNet.IsMaster) __instance.ReplicationWrapper.Replicator.Despawn();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThrowingWeapon), nameof(ThrowingWeapon.Throw))]
        public static void ThrowingWeapon_Throw(ThrowingWeapon __instance)
        {
            switch(__instance.ItemDataBlock.persistentID)
            {
                case 1000: __instance.m_gotoReadyTimer = Clock.Time + 0.25f; break;
                case 1001: __instance.m_gotoReadyTimer = Clock.Time; break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThrowingWeapon), nameof(ThrowingWeapon.OnWield))]
        public static void ThrowingWeapon_OnWield(ThrowingWeapon __instance)
        {
            switch (__instance.ItemDataBlock.persistentID)
            {
                case 1000: __instance.m_throwChargeTime = 0.35f; __instance.m_throwReleaseTime = 0.1f; break;
                case 1001: __instance.m_throwChargeTime = 0.2f; __instance.m_throwReleaseTime = 0.01f; break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PLOC_Fall), nameof(PLOC_Fall.Update))]
        public static bool PLOC_Fall_Update(PLOC_Fall __instance)
        {
            if (Clock.Time - __instance.m_enterTime > 10f && __instance.m_owner.TryWarpBack())
            {
                return false;
            }
            __instance.CheckExitConditions();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.UpdatePocketItemGUI))]
        public static bool UpdatePocketItemGUI(PlayerBackpackManager __instance) 
        {
            string first = "";
            string second = "";
            string third = "";
            string fourth = "";
            var leaderBoard = GameScoreManager.Current.m_LeaderBoard;

            for (int i = 0; i < leaderBoard.Count; i++)
            {
                switch (i) 
                {
                    case 0: first = $"<uppercase><color=green><u>1ST PLACE</u>//:<color=white> {leaderBoard[0].Player.NickName} - {leaderBoard[0].Score}</uppercase></color>\n"; break;
                    case 1: second = $"<uppercase><color=white><u>2ND PLACE</u>//:<color=white> {leaderBoard[1].Player.NickName} - {leaderBoard[1].Score}</uppercase></color>\n"; break;
                    case 2: third = $"<uppercase><color=white><u>3RD PLACE</u>//:<color=white> {leaderBoard[2].Player.NickName} - {leaderBoard[2].Score}</uppercase></color>\n"; break;
                    case 3: fourth = $"<uppercase><color=white><u>4TH PLACE</u>//:<color=white> {leaderBoard[3].Player.NickName} - {leaderBoard[3].Score}</uppercase></color>\n"; break;
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

        public static FX_Pool SpleefBombFX { get
            {
                if (s_SpleefBombFX == null) s_SpleefBombFX = FX_Manager.GetEffectPool(AssetShardManager.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/FX_Effects/FX_Tripmine.prefab", false));
                return s_SpleefBombFX;
            }
        }
        public static FX_Pool s_SpleefBombFX;

        public struct ScoreBoard
        {
            public int Score;
            public SNet_Player Player;
        }
    }
}
