#if BSM_VRCSDK3_AVATARS

using System;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.Sample, "Sample")]
    internal class BlendShapeSampleExpression : IBlendShapeExpression
    {
        [SerializeField]
        public string Name = string.Empty;

        [SerializeField]
        public float Weight = 100.0f;
    }
}

#endif
