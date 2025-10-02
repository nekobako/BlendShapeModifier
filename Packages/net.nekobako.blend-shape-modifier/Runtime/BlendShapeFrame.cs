using System;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [Serializable]
    internal class BlendShapeFrame : IEquatable<BlendShapeFrame>
    {
        [SerializeField, NotKeyable]
        public float Weight = 0.0f;

        [SerializeReference, NotKeyable]
        public IBlendShapeExpression Expression = new BlendShapeSampleExpression();

        public BlendShapeFrame Clone()
        {
            return new()
            {
                Weight = Weight,
                Expression = Expression.Clone(),
            };
        }

        public bool Equals(BlendShapeFrame other)
        {
            return other is not null
                && Weight.Equals(other.Weight)
                && Expression.Equals(other.Expression);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlendShapeFrame);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Weight);
            hash = HashCode.Combine(hash, Expression);
            return hash;
        }
    }
}
