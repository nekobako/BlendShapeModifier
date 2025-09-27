#if BSM_VRCSDK3_AVATARS

using System;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.FilterByMask, "Filter By Mask")]
    internal class BlendShapeFilterByMaskExpression : IBlendShapeExpression
    {
        [SerializeField]
        public int Slot = 0;

        [SerializeField]
        public Texture2D Mask = null;

        [SerializeReference]
        public IBlendShapeExpression Expression = new BlendShapeSampleExpression();
    }
}

#endif
