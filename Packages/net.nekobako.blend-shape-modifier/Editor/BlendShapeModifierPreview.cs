using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeModifierPreview : IRenderFilter
    {
        public static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Blend Shape Modifier", "net.nekobako.blend-shape-modifier", false);

        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(PreviewNode.IsEnabled);
        }

        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return PreviewNode;
        }

        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            return context.GetComponentsByType<BlendShapeModifier>()
                .Where(x => context.Observe(x, y => y.Renderer) && context.Observe(x, y => y.Renderer.sharedMesh))
                .Select(x => RenderGroup.For(x.Renderer).WithData(x))
                .ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context)
        {
            var modifier = group.GetData<BlendShapeModifier>();
            var node = new Node(modifier);
            return node.Refresh(pairs, context, 0);
        }

        private class Node : IRenderFilterNode
        {
            private readonly BlendShapeModifier m_Modifier = null;
            private readonly Mesh m_Mesh = null;
            private ComputeContext m_ShapesContext = null;
            private ComputeContext m_WeightsContext = null;

            public RenderAspects WhatChanged { get; private set; }

            public Node(BlendShapeModifier modifier)
            {
                m_Modifier = modifier;
                m_ShapesContext = new("BlendShapeModifierPreview.Shapes");
                m_WeightsContext = new("BlendShapeModifierPreview.Weights");
                m_Mesh = BlendShapeModifierProcessor.GenerateMesh(modifier, m_ShapesContext);
            }

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context, RenderAspects aspects)
            {
                if ((aspects & RenderAspects.Mesh) != 0)
                {
                    return Task.FromResult<IRenderFilterNode>(null);
                }

                m_ShapesContext.Invalidates(context);
                m_ShapesContext.Observe(m_Modifier, x => x.Shapes.Select(y => y.Clone(0.0f)).ToImmutableList(), Enumerable.SequenceEqual);
                if (m_ShapesContext.IsInvalidated)
                {
                    WhatChanged = RenderAspects.Mesh;
                    m_ShapesContext = new("BlendShapeModifierPreview.Shapes");
                    return Task.FromResult<IRenderFilterNode>(null);
                }

                m_WeightsContext.Invalidates(context);
                m_WeightsContext.Observe(m_Modifier, x => x.Shapes.Select(y => y.Weight).ToImmutableList(), Enumerable.SequenceEqual);
                if (m_WeightsContext.IsInvalidated)
                {
                    WhatChanged = RenderAspects.Shapes;
                    m_WeightsContext = new("BlendShapeModifierPreview.Weights");
                    return Task.FromResult<IRenderFilterNode>(this);
                }

                WhatChanged = 0;
                return Task.FromResult<IRenderFilterNode>(this);
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (proxy is not SkinnedMeshRenderer renderer)
                {
                    return;
                }

                renderer.sharedMesh = m_Mesh;

                BlendShapeModifierProcessor.ApplyWeights(m_Modifier, renderer);
            }

            public void Dispose()
            {
                Object.DestroyImmediate(m_Mesh);
            }
        }
    }
}
