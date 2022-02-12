using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace GregRundownCore
{
    class AutoRespawn : MonoBehaviour
    {
        public AutoRespawn(IntPtr value) : base(value)
        { }

        public void Awake()
        {
            m_Owner = GetComponent<PlayerAgent>();
            Patch.OnPlayerDowned += OnPlayerDowned;
            m_DownedText = GuiManager.InteractionLayer.m_message.m_headerText;
            m_DownedString = "<b>YOU ARE DOWNED, WAITING FOR TEAMMATE!</b>";
        }

        public void Update()
        {
            if (m_Owner.Locomotion.m_currentStateEnum != PlayerLocomotion.PLOC_State.Downed) return;

            if (m_RespawnTimer > 0) m_DownedText.SetText($"{m_DownedString}\nReviving in {Math.Max(0, Math.Truncate(m_RespawnTimer - Time.time))} seconds");
            else m_DownedText.SetText("Reviving");


            if (m_RespawnTimer < Time.time && m_RespawnTimer != 0)
            {
                m_RespawnTimer = 0;

                var data = new pSetHealthData();
                var value = new SFloat16();
                value.internalValue = 1;
                data.health = value;

                m_Owner.Locomotion.Downed.OnPlayerRevived(data);
            }
        }

        public void OnPlayerDowned()
        {
            m_RespawnTimer = Time.time + m_RespawnDelay;
            m_RespawnDelay += 30;
        }

        public float m_RespawnTimer = 0;
        public float m_RespawnDelay = 30;
        public TextMeshPro m_DownedText;
        public string m_DownedString;
        public PlayerAgent m_Owner;
    }
}
