using System;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFilterByAxisExpressionProcessor : BlendShapeExpressionProcessor<BlendShapeFilterByAxisExpression>
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(new BlendShapeFilterByAxisExpressionProcessor());
        }

        private BlendShapeFilterByAxisExpressionProcessor()
        {
        }

        protected override void OnProcess(BlendShapeFilterByAxisExpression expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            Process(expression.Expression, context, results);

            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                var distance = Vector3.Dot(context.VertexPositions[i] - expression.Position, expression.Direction.normalized);
                var weight = InverseLerp(-expression.FalloffRange * 0.5f, expression.FalloffRange * 0.5f, distance);
                result.Position *= weight;
                result.Normal *= weight;
                result.Tangent *= weight;
            }
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
