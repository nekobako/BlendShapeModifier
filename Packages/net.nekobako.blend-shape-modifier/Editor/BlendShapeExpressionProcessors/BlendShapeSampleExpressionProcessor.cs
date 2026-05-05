using System;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeSampleExpressionProcessor : BlendShapeExpressionProcessor<BlendShapeSampleExpression>
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(expression => new BlendShapeSampleExpressionProcessor(expression));
        }

        private BlendShapeSampleExpressionProcessor(BlendShapeSampleExpression expression) : base(expression)
        {
        }

        protected override void Prepare(BlendShapeModifierProcessor.Context context)
        {
        }

        public override void Process(BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            for (var i = context.BlendShapes.Length - 1; i >= 0; i--)
            {
                var blendShape = context.BlendShapes[i];
                if (blendShape.Name.Value != Expression.Name)
                {
                    continue;
                }

                if (blendShape.FrameCount == 0)
                {
                    return;
                }

                var minFrame = context.BlendShapeFrames[blendShape.FrameIndex + 0];
                if (Expression.Weight <= minFrame.Weight)
                {
                    if (blendShape.FrameCount == 1 || minFrame.Weight > 0.0f)
                    {
                        LerpOneFrame(minFrame, Expression.Weight, context, results);
                    }
                    else
                    {
                        var nextMinFrame = context.BlendShapeFrames[blendShape.FrameIndex + 1];
                        LerpTwoFrame(minFrame, nextMinFrame, Expression.Weight, context, results);
                    }
                    return;
                }

                var maxFrame = context.BlendShapeFrames[blendShape.FrameIndex + blendShape.FrameCount - 1];
                if (Expression.Weight >= maxFrame.Weight)
                {
                    if (blendShape.FrameCount == 1 || maxFrame.Weight < 0.0f)
                    {
                        LerpOneFrame(maxFrame, Expression.Weight, context, results);
                    }
                    else
                    {
                        var prevMaxFrame = context.BlendShapeFrames[blendShape.FrameIndex + blendShape.FrameCount - 2];
                        LerpTwoFrame(prevMaxFrame, maxFrame, Expression.Weight, context, results);
                    }
                    return;
                }

                for (var j = 0; j < context.BlendShapeFrames.Length - 1; j++)
                {
                    var prevMaxFrame = context.BlendShapeFrames[blendShape.FrameIndex + j + 0];
                    var nextMaxFrame = context.BlendShapeFrames[blendShape.FrameIndex + j + 1];
                    if (Expression.Weight >= prevMaxFrame.Weight && Expression.Weight <= nextMaxFrame.Weight)
                    {
                        LerpTwoFrame(prevMaxFrame, nextMaxFrame, Expression.Weight, context, results);
                        return;
                    }
                }
            }
        }

        public override void Dispose()
        {
        }

        private void LerpOneFrame(
            BlendShapeModifierProcessor.BlendShapeFrame frame,
            float weight,
            BlendShapeModifierProcessor.Context context,
            Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                var delta = context.BlendShapeDeltas[frame.DeltaIndex + i];
                var t = weight / frame.Weight;
                result.Position = Vector3.LerpUnclamped(Vector3.zero, delta.Position, t);
                result.Normal = Vector3.LerpUnclamped(Vector3.zero, delta.Normal, t);
                result.Tangent = Vector3.LerpUnclamped(Vector3.zero, delta.Tangent, t);
            }
        }

        private void LerpTwoFrame(
            BlendShapeModifierProcessor.BlendShapeFrame frameA,
            BlendShapeModifierProcessor.BlendShapeFrame frameB,
            float weight,
            BlendShapeModifierProcessor.Context context,
            Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                var deltaA = context.BlendShapeDeltas[frameA.DeltaIndex + i];
                var deltaB = context.BlendShapeDeltas[frameB.DeltaIndex + i];
                var t = (weight - frameA.Weight) / (frameB.Weight - frameA.Weight);
                result.Position = Vector3.LerpUnclamped(deltaA.Position, deltaB.Position, t);
                result.Normal = Vector3.LerpUnclamped(deltaA.Normal, deltaB.Normal, t);
                result.Tangent = Vector3.LerpUnclamped(deltaA.Tangent, deltaB.Tangent, t);
            }
        }
    }
}
