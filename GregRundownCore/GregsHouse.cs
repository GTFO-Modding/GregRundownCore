using AssetShards;
using BepInEx;
using BepInEx.IL2CPP;
using GTFO.API;
using HarmonyLib;
using MTFO.Managers;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace GregRundownCore
{
    [BepInPlugin("com.mccad00.GregCore", "GregCore", "1.0.0")]
    public class GregsHouse : BasePlugin
    {
        // The method that gets called when BepInEx tries to load our plugin
        public override void Load()
        {
            m_Harmony = new Harmony("com.mccad00.GregCore");
            m_Harmony.PatchAll();

            ClassInjector.RegisterTypeInIl2Cpp<GameScoreManager>();
            ClassInjector.RegisterTypeInIl2Cpp<LevelLightManager>();
            ClassInjector.RegisterTypeInIl2Cpp<LightAnimator>();
            ClassInjector.RegisterTypeInIl2Cpp<AutoRespawn>();
            ClassInjector.RegisterTypeInIl2Cpp<GlobalMusicManager>();
            ClassInjector.RegisterTypeInIl2Cpp<RundownBGRotation>();
            CoroutineHandler.Init();

            AssetShardManager.add_OnStartupAssetsLoaded((Action)OnStartupAssetsLoaded);
        }

        public void OnStartupAssetsLoaded()
        {
            GregManagers = new();
            GregManagers.name = "GregRundown_Manager";
            GregManagers.AddComponent<LevelLightManager>();
            GregManagers.AddComponent<GameScoreManager>();
            GregManagers.AddComponent<GlobalMusicManager>();

            NetworkAPI.RegisterEvent<byte>("SmallPickupCollected", GameScoreManager.SyncRecieveUpdateScore);
            NetworkAPI.RegisterEvent<byte>("GregSpawned", Patch.SyncRecieveApplause);
            NetworkAPI.RegisterEvent<byte>("PlayerDowned", Patch.SyncRecieveGasp);

            L.Error(LoadBNK(File.ReadAllBytes(@$"{ConfigManager.CustomPath}\GregRundownAudio.json"), out var bnkID));
            L.Error(bnkID);
        }

        public static bool LoadBNK(byte[] bytes, out uint bnkID)
        {
            try
            {
                var size = (uint)bytes.Length;
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var ptr = handle.AddrOfPinnedObject();

                #region Latex's Ass Fix
                if ((ptr.ToInt64() & 15L) != 0L) // gay gay check (I guess) [Re-aligns the bytes by 16 so last 4 bits are 0]
                {
                    byte[] array = new byte[(long)bytes.Length + 16L];
                    IntPtr newPtr = GCHandle.Alloc(array, GCHandleType.Pinned).AddrOfPinnedObject();
                    int offset = 0;
                    if ((newPtr.ToInt64() & 15L) != 0L)
                    {
                        long realignedPointerLoc = (newPtr.ToInt64() + 15L) & -16L; // re-aligns pointer location
                        offset = (int)(realignedPointerLoc - newPtr.ToInt64()); // updates offset
                        newPtr = new IntPtr(realignedPointerLoc); // create new pointer with re-aligned
                    }
                    Array.Copy(bytes, 0, array, offset, bytes.Length);
                    ptr = newPtr;
                    handle.Free(); // free original handle because we have a new one
                }
                #endregion

                return AkSoundEngine.LoadBank(ptr, size, out bnkID) == AKRESULT.AK_Success;
            }
            catch (Exception)
            {
                bnkID = 0;
                return false;
            }
        }

        private Harmony m_Harmony;
        public static GameObject GregManagers { get; set; }
    }
}
