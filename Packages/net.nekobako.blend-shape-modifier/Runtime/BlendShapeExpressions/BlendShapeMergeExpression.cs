#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.Merge, "Merge")]
    internal class BlendShapeMergeExpression : IBlendShapeExpression
    {
        [SerializeReference]
        public List<IBlendShapeExpression> Expressions = new() { new BlendShapeSampleExpression(), new BlendShapeSampleExpression() };
    }
}

#endif
