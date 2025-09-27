#if BSM_VRCSDK3_AVATARS

using System;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.FilterByAxis, "Filter By Axis")]
    internal class BlendShapeFilterByAxisExpression : IBlendShapeExpression
    {
        [SerializeField]
        public Vector3 Position = Vector3.zero;

        [SerializeField]
        public Vector3 Direction = Vector3.left;

        [SerializeField]
        public float FalloffRange = 0.0f;

        [SerializeReference]
        public IBlendShapeExpression Expression = new BlendShapeSampleExpression();
    }
}

#endif
