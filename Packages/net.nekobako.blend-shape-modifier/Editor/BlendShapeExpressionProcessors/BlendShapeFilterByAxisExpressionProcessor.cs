#if BSM_VRCSDK3_AVATARS

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

            var mesh = new Mesh();
            context.Modifier.Renderer.BakeMesh(mesh, true);

            var vertices = mesh.vertices;
            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                var weight = InverseLerp(-expression.FalloffRange * 0.5f, expression.FalloffRange * 0.5f, Vector3.Dot(vertices[i] - expression.Position, expression.Direction.normalized));
                result.Position *= weight;
                result.Normal *= weight;
                result.Tangent *= weight;
            }

            Object.DestroyImmediate(mesh);
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

#endif
