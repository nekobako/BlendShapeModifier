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
        private int m_Slot = 0;
        private Texture2D m_Mask = null;
        private Texture2D m_Temp = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(expression => new BlendShapeFilterByMaskExpressionProcessor(expression));
        }

        private BlendShapeFilterByMaskExpressionProcessor(BlendShapeFilterByMaskExpression expression) : base(expression)
        {
        }

        protected override void Prepare(BlendShapeModifierProcessor.Context context)
        {
            m_Mask = Expression.Mask;
            m_Slot = Mathf.Min(Expression.Slot, context.SubMeshCount - 1);

            if (!m_Mask)
            {
                return;
            }

            context.ComputeContext.Observe(m_Mask, x => x.imageContentsHash);

#if BSM_MASK_TEXTURE_EDITOR
            var editing = MaskTextureEditor.Editor.Window.ObserveTextureFor(context.ComputeContext, m_Mask, context.OriginalRenderer, m_Slot,
                BlendShapeFilterByMaskExpressionDrawer.MaskTextureEditorToken);
            if (editing)
            {
                m_Mask = editing;
            }
#endif

            if (m_Mask.isReadable)
            {
                return;
            }

            var rt = RenderTexture.GetTemporary(m_Mask.width, m_Mask.height);
            Graphics.Blit(m_Mask, rt);

            m_Mask = m_Temp = new(m_Mask.width, m_Mask.height)
            {
                filterMode = m_Mask.filterMode,
                wrapModeU = m_Mask.wrapModeU,
                wrapModeV = m_Mask.wrapModeV,
            };
            m_Mask.ReadPixels(new(0.0f, 0.0f, m_Mask.width, m_Mask.height), 0, 0);
            m_Mask.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
        }

        public override void Process(BlendShapeModifierProcessor.Context context, Span<BlendShapeModifierProcessor.BlendShapeDelta> results)
        {
            context.ExpressionProcessors[Expression.Expression].Process(context, results);

            if (!m_Mask)
            {
                return;
            }

            for (var i = 0; i < results.Length; i++)
            {
                ref var result = ref results[i];
                if (context.SubMeshMasks.IsSet(context.VertexCount * m_Slot + i))
                {
                    var uv = context.VertexUvs[i];
                    var weight = m_Mask.GetPixel((int)(uv.x * m_Mask.width), (int)(uv.y * m_Mask.height)).r;
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
        }

        public override void Dispose()
        {
            if (!m_Temp)
            {
                return;
            }

            Object.DestroyImmediate(m_Temp);
        }
    }
}
