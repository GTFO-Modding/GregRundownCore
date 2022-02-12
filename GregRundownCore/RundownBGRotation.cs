using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    class RundownBGRotation : MonoBehaviour
    {
        public RundownBGRotation(IntPtr value) : base(value) { }

        public void Update()
        {
            gameObject.transform.Rotate(new(0, 0.05f, 0));
        }
    }
}
