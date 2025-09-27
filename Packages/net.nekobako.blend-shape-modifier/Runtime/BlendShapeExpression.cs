#if BSM_VRCSDK3_AVATARS

using System;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    internal enum BlendShapeExpressionType
    {
        Sample,
        Merge,
        FilterByAxis,
        FilterByMask,
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class BlendShapeExpressionAttribute : PropertyAttribute
    {
        public readonly BlendShapeExpressionType Type = 0;
        public readonly string Name = string.Empty;

        public BlendShapeExpressionAttribute(BlendShapeExpressionType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    internal interface IBlendShapeExpression
    {
    }
}

#endif
