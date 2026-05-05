using System;
using Unity.Collections;
using UnityEditor;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeMergeExpressionProcessor : BlendShapeExpressionProcessor<BlendShapeMergeExpression>
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(expression => new BlendShapeMergeExpressionProcessor(expression));
        }

        private BlendShapeMergeExpressionProcessor(BlendShapeMergeExpression expression) : base(expression)
        {
        }

        protected override void Prepare(BlendShapeModifierProcessor.Context context)
        {
        }

        public override void Process(BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            using var blendShapeDeltas = new NativeArray<BlendShapeModifierProcessor.BlendShapeDelta>(results.Length, Allocator.Temp);
            var blendShapeDeltasSpan = blendShapeDeltas.AsSpan();

            foreach (var expression in Expression.Expressions)
            {
                context.ExpressionProcessors[expression].Process(context, blendShapeDeltasSpan);

                for (var i = 0; i < results.Length; i++)
                {
                    ref var result = ref results[i];
                    ref var delta = ref blendShapeDeltasSpan[i];
                    result.Position += delta.Position;
                    result.Normal += delta.Normal;
                    result.Tangent += delta.Tangent;
                }

                blendShapeDeltasSpan.Clear();
            }
        }

        public override void Dispose()
        {
        }
    }
}
