#if BSM_VRCSDK3_AVATARS

using System;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable]
    internal class BlendShapeFrame
    {
        [SerializeField]
        public float Weight = 0.0f;

        [SerializeReference]
        public IBlendShapeExpression Expression = new BlendShapeSampleExpression();
    }
}

#endif
