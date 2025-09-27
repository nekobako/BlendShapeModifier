#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable]
    internal class BlendShape
    {
        [SerializeField]
        public string Name = string.Empty;

        [SerializeField]
        public float Weight = 0.0f;

        [SerializeReference]
        public List<BlendShapeFrame> Frames = new();
    }
}

#endif
