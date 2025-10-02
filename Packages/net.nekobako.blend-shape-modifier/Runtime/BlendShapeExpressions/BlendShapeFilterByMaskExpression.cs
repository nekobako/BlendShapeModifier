using System;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.FilterByMask, "Filter By Mask")]
    internal class BlendShapeFilterByMaskExpression : IBlendShapeExpression
    {
        [SerializeField, NotKeyable]
        public int Slot = 0;

        [SerializeField, NotKeyable]
        public Texture2D Mask = null;

        [SerializeReference, NotKeyable]
        public IBlendShapeExpression Expression = new BlendShapeSampleExpression();

        public IBlendShapeExpression Clone()
        {
            return new BlendShapeFilterByMaskExpression
            {
                Slot = Slot,
                Mask = Mask,
                Expression = Expression.Clone(),
            };
        }

        public bool Equals(IBlendShapeExpression other)
        {
            return other is BlendShapeFilterByMaskExpression expression
                && Slot.Equals(expression.Slot)
                && Mask.Equals(expression.Mask)
                && Expression.Equals(expression.Expression);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IBlendShapeExpression);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Slot);
            hash = HashCode.Combine(hash, Mask);
            hash = HashCode.Combine(hash, Expression);
            return hash;
        }
    }
}
