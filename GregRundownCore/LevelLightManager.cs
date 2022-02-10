using LevelGeneration;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace GregRundownCore
{
    public class LevelLightManager : MonoBehaviour
    {
        public LevelLightManager(IntPtr value) : base(value)
        { }

        public void Awake()
        {
            Current = this;
            LG_Factory.add_OnFactoryBuildDone((Action)Setup);
            m_SongIndex = new System.Random().Next(1, 7);

            Patch.OnLevelCleanup += Cleanup;
            Patch.OnPlayerWarped += StartSequence;
        }

        public void Setup()
        {
            m_LevelLights.Clear();

            LG_Light light;
            LightAnimator animator;
            m_Fog = PlayerManager.Current.m_localPlayerAgentInLevel.FPSCamera.PrelitVolume;

            foreach(var obj in GameObject.FindObjectsOfTypeAll(UnhollowerRuntimeLib.Il2CppType.Of<LG_Light>()))
            {
                light = obj.TryCast<LG_Light>();
                m_LevelLights.Add(light);

                animator = light.gameObject.AddComponent<LightAnimator>();
                animator.m_Manager = this;
            }
        }

        public void StartSequence()
        {
            if (m_AnimateLights) return;

            m_AnimateLights = true;
            m_NextAnimTimer = Time.time + 10;
            a_PlayAnimation?.Invoke(LightAnimator.eLightAnimation.FadeIn, m_NextAnimDelay);

            var soundPlayer = PlayerManager.Current.m_localPlayerAgentInLevel.Sound;

            switch (m_SongIndex)
            {
                case 1: soundPlayer.Post(3409648182u); break;
                case 2: soundPlayer.Post(3409648181u); break;
                case 3: soundPlayer.Post(3409648180u); break;
                case 4: soundPlayer.Post(3409648179u); break;
                case 5: soundPlayer.Post(3409648178u); break;
                case 6: soundPlayer.Post(3409648177u); break;
            }

            m_SongIndex += 1;
            if (m_SongIndex > 6) m_SongIndex = 1;

            GuiManager.PlayerLayer.m_wardenObjective.m_itemsHeader.transform.FindChild("Text").GetComponent<TextMeshPro>().SetText("LEADERBOARD");
            PlayerManager.Current.m_localPlayerAgentInLevel.gameObject.AddComponent<AutoRespawn>();
        }

        public void Cleanup()
        {
            foreach(var light in m_LevelLights)
            {
                var animator = light.GetComponent<LightAnimator>();
                foreach (var routine in animator.m_LightCoroutines) routine.Stop();
                GameObject.Destroy(animator);
            }
            m_AnimateLights = false;
            m_LevelLights.Clear();
        }

        public void Update()
        {
            if (!m_AnimateLights) return;

            if (m_NextAnimTimer < Time.time)
            {
                m_NextAnimTimer = Time.time + (m_NextAnimDelay * 0.017f);
                a_PlayAnimation?.Invoke((LightAnimator.eLightAnimation)new System.Random().Next(9), m_NextAnimDelay);
            }
        }

        public void FixedUpdate()
        {
            if (!m_AnimateLights) return;

            m_EaseRGB_Slow = EaseRGB(1);
            m_EaseRGB_Fast = EaseRGB(4);
            m_Pulse_Slow = Pulse(1);
            m_Pulse_Fast = Pulse(4);

            m_Fog.m_fogColor = m_EaseRGB_Slow;
        }

        public Color EaseRGB(float frequency)
        {
            float r;
            float g;
            float b;

            r = MathF.Sin(Time.time * frequency) * 0.5f + 0.5f;
            g = MathF.Sin((Time.time + 2) * frequency) * 0.5f + 0.5f;
            b = MathF.Sin((Time.time + 4) * frequency) * 0.5f + 0.5f;

            return new Color(r, g, b, 1);
        }

        public float Pulse(float frequency)
        {
            return MathF.Sin(Time.time * frequency) * 0.5f + 0.5f;
        }


        public List<LG_Light> m_LevelLights = new();
        public bool m_AnimateLights = false;
        public float m_NextAnimTimer = 0;
        public float m_NextAnimDelay = 240;

        public Color m_EaseRGB_Slow = Color.white;
        public Color m_EaseRGB_Fast = Color.white;
        public float m_Pulse_Slow;
        public float m_Pulse_Fast;
        public PreLitVolume m_Fog;
        public int m_SongIndex;

        public static LevelLightManager Current;
        public static event Action<LightAnimator.eLightAnimation, float> a_PlayAnimation;
    }
}
