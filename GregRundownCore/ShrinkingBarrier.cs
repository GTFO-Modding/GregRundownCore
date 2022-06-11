using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    class ShrinkingBarrier : MonoBehaviour
    {
        public ShrinkingBarrier(IntPtr value) : base(value) { }

        public void Update() 
        {
            if (!m_DoShrink) return;

            transform.localScale += m_ShrinkVector * (Time.deltaTime * 60);
            transform.localEulerAngles += m_RotVector * (Time.deltaTime * 60);
        }

        public void Reset()
        {
            transform.localScale = new(13000, 13000, 3156.962f);
            transform.localEulerAngles = new(-90, 0, 0);
        }

        public bool m_DoShrink;
        public Vector3 m_ShrinkVector = new(-2.5f, -2.5f, 0);
        public Vector3 m_RotVector = new(0, 0, 0.05f);
    }
}
