#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.Merge, "Merge")]
    internal class BlendShapeMergeExpression : IBlendShapeExpression
    {
        [SerializeReference, NotKeyable]
        public List<IBlendShapeExpression> Expressions = new() { new BlendShapeSampleExpression(), new BlendShapeSampleExpression() };

        public IBlendShapeExpression Clone()
        {
            return new BlendShapeMergeExpression
            {
                Expressions = Expressions.ConvertAll(x => x.Clone()),
            };
        }

        public bool Equals(IBlendShapeExpression other)
        {
            return other is BlendShapeMergeExpression expression
                && Expressions.SequenceEqual(expression.Expressions);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IBlendShapeExpression);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = Expressions.Aggregate(hash, HashCode.Combine);
            return hash;
        }
    }
}

#endif
