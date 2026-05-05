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
            Register(new BlendShapeMergeExpressionProcessor());
        }

        private BlendShapeMergeExpressionProcessor()
        {
        }

        protected override void OnProcess(BlendShapeMergeExpression expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            using var blendShapeDeltas = new NativeArray<BlendShapeModifierProcessor.BlendShapeDelta>(results.Length, Allocator.Temp);
            var blendShapeDeltasSpan = blendShapeDeltas.AsSpan();

            foreach (var target in expression.Expressions)
            {
                Process(target, context, blendShapeDeltasSpan);

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
    }
}
