#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal abstract class BlendShapeExpressionProcessor
    {
        private static readonly Dictionary<Type, BlendShapeExpressionProcessor> s_Instances = new();

        protected static void Register<T>(BlendShapeExpressionProcessor instance) where T : IBlendShapeExpression
        {
            s_Instances[typeof(T)] = instance;
        }

        public static void Process(IBlendShapeExpression expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            results.Clear();
            s_Instances[expression.GetType()].OnProcess(expression, context, results);
        }

        protected abstract void OnProcess(IBlendShapeExpression expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results);
    }

    internal abstract class BlendShapeExpressionProcessor<T> : BlendShapeExpressionProcessor where T : IBlendShapeExpression
    {
        protected static void Register(BlendShapeExpressionProcessor<T> instance)
        {
            Register<T>(instance);
        }

        protected sealed override void OnProcess(IBlendShapeExpression expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            OnProcess((T)expression, context, results);
        }

        protected abstract void OnProcess(T expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results);
    }
}

#endif
