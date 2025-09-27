#if BSM_VRCSDK3_AVATARS

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using nadena.dev.ndmf.preview;
using Object = UnityEngine.Object;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFilterByMaskExpressionProcessor : BlendShapeExpressionProcessor<BlendShapeFilterByMaskExpression>
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(new BlendShapeFilterByMaskExpressionProcessor());
        }

        private BlendShapeFilterByMaskExpressionProcessor()
        {
        }

        protected override void OnProcess(BlendShapeFilterByMaskExpression expression, BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            Process(expression.Expression, context, results);

            var mask = expression.Mask;
            if (!mask)
            {
                return;
            }

            context.ComputeContext.Observe(mask, x => x.imageContentsHash);

            if (!mask.isReadable)
            {
                var rt = RenderTexture.GetTemporary(mask.width, mask.height);
                Graphics.Blit(mask, rt);

                mask = new(mask.width, mask.height)
                {
                    filterMode = mask.filterMode,
                    wrapModeU = mask.wrapModeU,
                    wrapModeV = mask.wrapModeV,
                };
                mask.ReadPixels(new(0.0f, 0.0f, mask.width, mask.height), 0, 0);
                mask.Apply();

                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);
            }

            var uv = context.Modifier.Renderer.sharedMesh.uv;
            foreach (var index in context.Modifier.Renderer.sharedMesh.GetIndices(Mathf.Min(expression.Slot, context.Modifier.Renderer.sharedMesh.subMeshCount - 1)).Distinct())
            {
                ref var result = ref results[index];
                var weight = mask.GetPixel((int)(uv[index].x * mask.width), (int)(uv[index].y * mask.height)).r;
                result.Position *= weight;
                result.Normal *= weight;
                result.Tangent *= weight;
            }

            if (mask != expression.Mask)
            {
                Object.DestroyImmediate(mask);
            }
        }
    }
}

#endif
