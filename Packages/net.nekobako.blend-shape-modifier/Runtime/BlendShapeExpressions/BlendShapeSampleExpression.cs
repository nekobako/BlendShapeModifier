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

        public IBlendShapeExpression Clone()
        {
            return new BlendShapeSampleExpression
            {
                Name = Name,
                Weight = Weight,
            };
        }

        public bool Equals(IBlendShapeExpression other)
        {
            return other is BlendShapeSampleExpression expression
                && Name.Equals(expression.Name)
                && Weight.Equals(expression.Weight);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IBlendShapeExpression);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Name);
            hash = HashCode.Combine(hash, Weight);
            return hash;
        }
    }
}

#endif
