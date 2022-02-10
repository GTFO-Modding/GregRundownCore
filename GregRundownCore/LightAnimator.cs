using LevelGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    public class LightAnimator : MonoBehaviour
    {
        public LightAnimator(IntPtr value) : base(value) { }

        public void Awake() 
        {
            LevelLightManager.a_PlayAnimation += RecieveLightAnimation;

            m_Light = GetComponent<LG_Light>();
            m_OriginalColor = m_Light.m_color;
            m_OriginalIntensity = m_Light.m_intensity;
        }
        public void Update() { }

        public void RecieveLightAnimation(eLightAnimation animation, float duration)
        {
            if (animation == eLightAnimation.Kill)
            {
                foreach (var coroutine in m_LightCoroutines) coroutine.Stop();
                m_LightCoroutines.Clear();
                m_Light.ChangeColor(m_OriginalColor);
                m_Light.ChangeIntensity(m_OriginalIntensity);
            }

            if (m_IntroSequenceComplete) animation = (eLightAnimation)(((int)animation + (int)m_Light.m_category) % 8);
            else m_IntroSequenceComplete = true;

            m_Animation = animation;
            m_Reset = true;
            m_DoSwitchColor = true;
            if (!m_AnimatorActive)
            {
                m_LightCoroutines.Add(CoroutineHandler.Add(Animate()));
                m_AnimatorActive = true;
            }
        }

        public void RandomizeColor()
        {
            m_Light.ChangeColor(new Color((float)new System.Random().NextDouble(), (float)new System.Random().NextDouble(), (float)new System.Random().NextDouble(), 1));
        }
        public void OnDestroy()
        {
            LevelLightManager.a_PlayAnimation -= RecieveLightAnimation;
        }

        public IEnumerator Animate()
        {
            while (1 == 1)
            {
                if (!m_Reset)
                {
                    switch (m_Animation)
                    {
                        case eLightAnimation.FadeIn: FadeIn(0.001f); break;
                        case eLightAnimation.FadeOut: FadeIn(-0.001f); break;

                        case eLightAnimation.Strobe: Strobe(); break;

                        case eLightAnimation.PulseSlow: m_Light.ChangeIntensity(m_Manager.m_Pulse_Slow); break;
                        case eLightAnimation.PulseFast: m_Light.ChangeIntensity(m_Manager.m_Pulse_Fast); break;

                        case eLightAnimation.EaseRGBSlow: m_Light.ChangeColor(m_Manager.m_EaseRGB_Slow); break;
                        case eLightAnimation.EaseRGBFast: m_Light.ChangeColor(m_Manager.m_EaseRGB_Fast); break;

                        case eLightAnimation.SwitchRGBSlow: SwitchRGB(5); break;
                        case eLightAnimation.SwitchRGBFast: SwitchRGB(10);  break;
                    }
                }
                else
                {
                    m_Light.ChangeColor(m_OriginalColor);
                    m_Light.ChangeIntensity(m_OriginalIntensity);
                    m_Reset = false;
                }
                yield return null;
            }

        }
        public void FadeIn(float rateOfChange)
        {
            if (m_DoSwitchColor)
            {
                m_Light.ChangeIntensity(0);
                m_DoSwitchColor = false;
            }
            else m_Light.ChangeIntensity(m_Light.m_intensity + rateOfChange);
        }

        public void Strobe()
        {
            if (m_Light.m_emitterMesh != null && Convert.ToInt32(Time.time * 10) % 2 == 1) m_Light.ChangeIntensity(m_OriginalIntensity * 0.3f);
            else m_Light.ChangeIntensity(m_OriginalIntensity);
        }

        public void Pulse(float frequency)
        {
            m_Light.ChangeIntensity(MathF.Sin(Time.time * frequency) * (m_OriginalIntensity * 0.5f) + (m_OriginalIntensity * 0.5f));
        }
        public void EaseRGB(float frequency)
        {
            float r;
            float g;
            float b;

            r = MathF.Sin(Time.time * frequency) * 0.5f + 0.5f;
            g = MathF.Sin((Time.time + 2) * frequency) * 0.5f + 0.5f;
            b = MathF.Sin((Time.time + 4) * frequency) * 0.5f + 0.5f;

            m_Light.ChangeColor(new Color(r, g, b, 1));
        }

        public void SwitchRGB(float timeMulti)
        {
            if (Convert.ToInt32(Time.time * timeMulti) % 2 == 1)
            {
                if (m_DoSwitchColor)
                {
                    RandomizeColor();
                    m_DoSwitchColor = false;
                }
            }
            else m_DoSwitchColor = true;
        }

        public LevelLightManager m_Manager;
        public LG_Light m_Light;
        public bool m_IntroSequenceComplete;
        public bool m_DoSwitchColor;
        public Color m_OriginalColor;
        public float m_OriginalIntensity;
        public eLightAnimation m_Animation;
        public bool m_Reset;
        public bool m_AnimatorActive;
        public List<CoroutineHandler.IRoutine> m_LightCoroutines = new();
        public enum eLightAnimation
        {
            FadeIn = 0,
            FadeOut = 1,
            Strobe = 2,
            PulseSlow = 3,
            PulseFast = 4,
            EaseRGBSlow = 5,
            EaseRGBFast = 6,
            SwitchRGBSlow = 7,
            SwitchRGBFast = 8,
            Kill = 999
        }
    }
}
