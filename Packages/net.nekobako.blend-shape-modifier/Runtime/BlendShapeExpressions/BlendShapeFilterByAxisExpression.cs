using System;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable, BlendShapeExpression(BlendShapeExpressionType.FilterByAxis, "Filter By Axis")]
    internal class BlendShapeFilterByAxisExpression : IBlendShapeExpression
    {
        [SerializeField, NotKeyable]
        public Vector3 Position = Vector3.zero;

        [SerializeField, NotKeyable]
        public Vector3 Direction = Vector3.left;

        [SerializeField, NotKeyable]
        public float FalloffRange = 0.0f;

        [SerializeReference, NotKeyable]
        public IBlendShapeExpression Expression = new BlendShapeSampleExpression();

        public IBlendShapeExpression Clone()
        {
            return new BlendShapeFilterByAxisExpression
            {
                Position = Position,
                Direction = Direction,
                FalloffRange = FalloffRange,
                Expression = Expression.Clone(),
            };
        }

        public bool Equals(IBlendShapeExpression other)
        {
            return other is BlendShapeFilterByAxisExpression expression
                && Position.Equals(expression.Position)
                && Direction.Equals(expression.Direction)
                && FalloffRange.Equals(expression.FalloffRange)
                && Expression.Equals(expression.Expression);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IBlendShapeExpression);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Position);
            hash = HashCode.Combine(hash, Direction);
            hash = HashCode.Combine(hash, FalloffRange);
            hash = HashCode.Combine(hash, Expression);
            return hash;
        }
    }
}
