using System;
using System.Collections.Generic;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal abstract class BlendShapeExpressionProcessor : IDisposable
    {
        private static readonly Dictionary<Type, Func<IBlendShapeExpression, BlendShapeExpressionProcessor>> s_Creators = new();

        protected static void Register<T>(Func<T, BlendShapeExpressionProcessor> creator) where T : IBlendShapeExpression
        {
            s_Creators[typeof(T)] = expression => creator((T)expression);
        }

        public static BlendShapeExpressionProcessor Create(IBlendShapeExpression expression, BlendShapeModifierProcessor.Context context)
        {
            var instance = s_Creators[expression.GetType()].Invoke(expression);
            instance.Prepare(context);
            return instance;
        }

        protected abstract void Prepare(BlendShapeModifierProcessor.Context context);
        public abstract void Process(BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results);
        public abstract void Dispose();
    }

    internal abstract class BlendShapeExpressionProcessor<T> : BlendShapeExpressionProcessor where T : IBlendShapeExpression
    {
        protected readonly T Expression = default;

        protected static void Register(Func<T, BlendShapeExpressionProcessor<T>> creator)
        {
            Register<T>(creator);
        }

        protected BlendShapeExpressionProcessor(T expression)
        {
            Expression = expression;
        }
    }
}
