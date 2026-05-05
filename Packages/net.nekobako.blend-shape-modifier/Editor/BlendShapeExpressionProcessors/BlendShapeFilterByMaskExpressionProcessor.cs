using System;
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

#if BSM_MASK_TEXTURE_EDITOR
            var editing = MaskTextureEditor.Editor.Window.ObserveTextureFor(context.ComputeContext, expression.Mask, context.OriginalRenderer, expression.Slot,
                BlendShapeFilterByMaskExpressionDrawer.MaskTextureEditorToken);
            if (editing)
            {
                mask = editing;
            }
#endif

            var temp = default(Texture2D);
            if (!mask.isReadable)
            {
                var rt = RenderTexture.GetTemporary(mask.width, mask.height);
                Graphics.Blit(mask, rt);

                mask = temp = new(mask.width, mask.height)
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

            var slot = Mathf.Min(expression.Slot, context.SubMeshCount - 1);
            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                if (context.SubMeshMasks.IsSet(context.VertexCount * slot + i))
                {
                    var uv = context.VertexUvs[i];
                    var weight = mask.GetPixel((int)(uv.x * mask.width), (int)(uv.y * mask.height)).r;
                    result.Position *= weight;
                    result.Normal *= weight;
                    result.Tangent *= weight;
                }
                else
                {
                    result.Position = Vector3.zero;
                    result.Normal = Vector3.zero;
                    result.Tangent = Vector3.zero;
                }
            }

            if (temp)
            {
                Object.DestroyImmediate(temp);
            }
        }
    }
}
