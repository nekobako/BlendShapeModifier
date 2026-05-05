using System;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFilterByAxisExpressionProcessor : BlendShapeExpressionProcessor<BlendShapeFilterByAxisExpression>
    {
        private readonly BlendShapeExpressionProcessor m_Processor = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(expression => new BlendShapeFilterByAxisExpressionProcessor(expression));
        }

        private BlendShapeFilterByAxisExpressionProcessor(BlendShapeFilterByAxisExpression expression) : base(expression)
        {
            m_Processor = Create(expression.Expression);
        }

        public override void Prepare(BlendShapeModifierProcessor.Context context)
        {
            m_Processor.Prepare(context);
        }

        public override void Process(BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            m_Processor.Process(context, results);

            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                var distance = Vector3.Dot(context.VertexPositions[i] - Expression.Position, Expression.Direction.normalized);
                var weight = InverseLerp(-Expression.FalloffRange * 0.5f, Expression.FalloffRange * 0.5f, distance);
                result.Position *= weight;
                result.Normal *= weight;
                result.Tangent *= weight;
            }
        }

        public override void Dispose()
        {
            m_Processor.Dispose();
        }

        private float InverseLerp(float a, float b, float value)
        {
            var min = Mathf.Min(a, b);
            var max = Mathf.Max(a, b);
            return
                value < min ? 0.0f :
                value > max ? 1.0f :
                Mathf.InverseLerp(min, max, value);
        }
    }
}
