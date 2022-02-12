using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    class GlobalMusicManager : MonoBehaviour
    {
        public GlobalMusicManager(IntPtr value) : base(value) { }

        public void Awake()
        {
            m_SoundPlayer = new();
            LG_Factory.add_OnFactoryBuildStart((Action)Stop);
        }

        public void Play(string sound) 
        {
            if (sound == "play_Song_Menu") m_MenuThemePlaying = true;

            m_SoundPlayer.Post(sound); 
        }
        public void Stop() 
        {
            m_SoundPlayer.Post("stop_Song_All");
            m_MenuThemePlaying = false;
        }

        public CellSoundPlayer m_SoundPlayer;
        public bool m_MenuThemePlaying;
    }
}
