#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal static class BlendShapeModifierProcessor
    {
        public readonly ref struct Context
        {
            public BlendShapeModifier Modifier { get; init; }
            public ReadOnlySpan<BlendShape> BlendShapes { get; init; }
            public ReadOnlySpan<BlendShapeFrame> BlendShapeFrames { get; init; }
            public ReadOnlySpan<BlendShapeDelta> BlendShapeDeltas { get; init; }
        }

        public struct BlendShape
        {
            public FixedString128Bytes Name;
            public int FrameIndex;
            public int FrameCount;
        }

        public struct BlendShapeFrame
        {
            public float Weight;
            public int DeltaIndex;
            public int DeltaCount;
        }

        public struct BlendShapeDelta
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Tangent;
        }

        public static void Process(BlendShapeModifier modifier)
        {
            if (!modifier.Renderer || !modifier.Renderer.sharedMesh)
            {
                return;
            }

            var mesh = Object.Instantiate(modifier.Renderer.sharedMesh);

            using var blendShapes = new NativeArray<BlendShape>(mesh.blendShapeCount + modifier.Shapes.Count, Allocator.Temp);

            var blendShapesSpan = blendShapes.AsSpan();
            var blendShapeFrameIndex = 0;
            for (var i = 0; i < blendShapesSpan.Length; i++)
            {
                ref var blendShape = ref blendShapesSpan[i];
                blendShape.Name = i < mesh.blendShapeCount ? mesh.GetBlendShapeName(i) : modifier.Shapes[i - mesh.blendShapeCount].Name;
                blendShape.FrameIndex = blendShapeFrameIndex;
                blendShape.FrameCount = i < mesh.blendShapeCount ? mesh.GetBlendShapeFrameCount(i) : modifier.Shapes[i - mesh.blendShapeCount].Frames.Count;
                blendShapeFrameIndex += blendShape.FrameCount;
            }

            using var blendShapeFrames = new NativeArray<BlendShapeFrame>(blendShapeFrameIndex, Allocator.Temp);

            var blendShapeFramesSpan = blendShapeFrames.AsSpan();
            var blendShapeDeltaIndex = 0;
            for (var i = 0; i < blendShapesSpan.Length; i++)
            {
                ref var blendShape = ref blendShapesSpan[i];
                for (var j = 0; j < blendShape.FrameCount; j++)
                {
                    ref var blendShapeFrame = ref blendShapeFramesSpan[blendShape.FrameIndex + j];
                    blendShapeFrame.Weight = i < mesh.blendShapeCount ? mesh.GetBlendShapeFrameWeight(i, j) : modifier.Shapes[i - mesh.blendShapeCount].Frames[j].Weight;
                    blendShapeFrame.DeltaIndex = blendShapeDeltaIndex;
                    blendShapeFrame.DeltaCount = mesh.vertexCount;
                    blendShapeDeltaIndex += blendShapeFrame.DeltaCount;
                }
            }

            using var blendShapeDeltas = new NativeArray<BlendShapeDelta>(blendShapeDeltaIndex, Allocator.Temp);

            var blendShapeDeltasSpan = blendShapeDeltas.AsSpan();
            var deltaPositionBuffer = new Vector3[mesh.vertexCount];
            var deltaNormalBuffer = new Vector3[mesh.vertexCount];
            var deltaTangentBuffer = new Vector3[mesh.vertexCount];
            for (var i = 0; i < blendShapesSpan.Length; i++)
            {
                ref var blendShape = ref blendShapesSpan[i];
                for (var j = 0; j < blendShape.FrameCount; j++)
                {
                    ref var blendShapeFrame = ref blendShapeFramesSpan[blendShape.FrameIndex + j];
                    if (i < mesh.blendShapeCount)
                    {
                        mesh.GetBlendShapeFrameVertices(i, j, deltaPositionBuffer, deltaNormalBuffer, deltaTangentBuffer);
                        for (var k = 0; k < blendShapeFrame.DeltaCount; k++)
                        {
                            ref var blendShapeDelta = ref blendShapeDeltasSpan[blendShapeFrame.DeltaIndex + k];
                            blendShapeDelta.Position = deltaPositionBuffer[k];
                            blendShapeDelta.Normal = deltaNormalBuffer[k];
                            blendShapeDelta.Tangent = deltaTangentBuffer[k];
                        }
                    }
                    else
                    {
                        BlendShapeExpressionProcessor.Process(
                            modifier.Shapes[i - mesh.blendShapeCount].Frames[j].Expression,
                            new()
                            {
                                Modifier = modifier,
                                BlendShapes = blendShapesSpan[..i],
                                BlendShapeFrames = blendShapeFramesSpan[..blendShapesSpan[i].FrameIndex],
                                BlendShapeDeltas = blendShapeDeltasSpan[..blendShapeFramesSpan[blendShapesSpan[i].FrameIndex].DeltaIndex],
                            },
                            blendShapeDeltasSpan[blendShapeFrame.DeltaIndex..(blendShapeFrame.DeltaIndex + blendShapeFrame.DeltaCount)]);
                    }
                }
            }

            var blendShapeNames = new List<string>();
            var blendShapesByName = new Dictionary<string, BlendShape>();
            for (var i = 0; i < blendShapesSpan.Length; i++)
            {
                ref var blendShape = ref blendShapesSpan[i];
                if (blendShapesByName.TryAdd(blendShape.Name.Value, blendShape))
                {
                    blendShapeNames.Add(blendShape.Name.Value);
                }
                else
                {
                    blendShapesByName[blendShape.Name.Value] = blendShape;
                }
            }
            blendShapesSpan = blendShapeNames
                .Select(x => blendShapesByName[x])
                .ToArray();

            mesh.ClearBlendShapes();

            for (var i = 0; i < blendShapesSpan.Length; i++)
            {
                ref var blendShape = ref blendShapesSpan[i];
                var name = blendShape.Name.Value;
                var nextMinWeight = float.MinValue;
                for (var j = 0; j < blendShape.FrameCount; j++)
                {
                    ref var blendShapeFrame = ref blendShapeFramesSpan[blendShape.FrameIndex + j];
                    var weight = Mathf.Max(blendShapeFrame.Weight, nextMinWeight);
                    nextMinWeight = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(weight) + 1);
                    for (var k = 0; k < blendShapeFrame.DeltaCount; k++)
                    {
                        ref var blendShapeDelta = ref blendShapeDeltasSpan[blendShapeFrame.DeltaIndex + k];
                        deltaPositionBuffer[k] = blendShapeDelta.Position;
                        deltaNormalBuffer[k] = blendShapeDelta.Normal;
                        deltaTangentBuffer[k] = blendShapeDelta.Tangent;
                    }
                    mesh.AddBlendShapeFrame(name, weight, deltaPositionBuffer, deltaNormalBuffer, deltaTangentBuffer);
                }
            }

            modifier.Renderer.sharedMesh = mesh;

            foreach (var shape in modifier.Shapes)
            {
                var index = mesh.GetBlendShapeIndex(shape.Name);
                if (index >= 0 && index < mesh.blendShapeCount)
                {
                    modifier.Renderer.SetBlendShapeWeight(index, shape.Weight);
                }
            }
        }
    }
}

#endif
