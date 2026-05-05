using System;
using System.Linq;
using Unity.Collections;
using UnityEditor;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeMergeExpressionProcessor : BlendShapeExpressionProcessor<BlendShapeMergeExpression>
    {
        private readonly BlendShapeExpressionProcessor[] m_Processors = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(expression => new BlendShapeMergeExpressionProcessor(expression));
        }

        private BlendShapeMergeExpressionProcessor(BlendShapeMergeExpression expression) : base(expression)
        {
            m_Processors = expression.Expressions
                .Select(Create)
                .ToArray();
        }

        public override void Prepare(BlendShapeModifierProcessor.Context context)
        {
            foreach (var processor in m_Processors)
            {
                processor.Prepare(context);
            }
        }

        public override void Process(BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            using var blendShapeDeltas = new NativeArray<BlendShapeModifierProcessor.BlendShapeDelta>(results.Length, Allocator.Temp);
            var blendShapeDeltasSpan = blendShapeDeltas.AsSpan();

            foreach (var processor in m_Processors)
            {
                processor.Process(context, blendShapeDeltasSpan);

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
            foreach (var processor in m_Processors)
            {
                processor.Dispose();
            }
        }
    }
}
