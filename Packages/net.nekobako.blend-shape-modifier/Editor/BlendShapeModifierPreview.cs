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
                .Where(x => context.Observe(x, y => y.Renderer) && context.Observe(x.Renderer, y => y.sharedMesh))
                .GroupBy(x => x.Renderer)
                .Select(x => RenderGroup.For(x.Key).WithData(x.ToArray()))
                .ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context)
        {
            var (original, proxy) = pairs.Single();
            var modifiers = group.GetData<BlendShapeModifier[]>();
            var node = new Node(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, modifiers, context);
            return Task.FromResult<IRenderFilterNode>(node);
        }

        private class Node : IRenderFilterNode
        {
            private readonly BlendShapeModifier[] m_Modifiers = null;
            private readonly Mesh m_Mesh = null;
            private ComputeContext m_MeshContext = null;
            private ComputeContext m_ShapesContext = null;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Mesh;

            public Node(SkinnedMeshRenderer original, SkinnedMeshRenderer proxy, BlendShapeModifier[] modifiers, ComputeContext context)
            {
                m_Modifiers = modifiers;
                CreateContexts(context);

                BlendShapeModifierProcessor.Process(original, proxy, modifiers, m_MeshContext);
                m_Mesh = proxy.sharedMesh;
            }

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context, RenderAspects aspects)
            {
                if (aspects.HasFlag(RenderAspects.Mesh) || m_MeshContext.IsInvalidated)
                {
                    // Returning null here forcibly passes RenderAspects.Everything to Refresh() of downstream nodes
                    // return Task.FromResult<IRenderFilterNode>(null);
                    var (original, proxy) = pairs.Single();
                    var node = new Node(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, m_Modifiers, context);
                    return Task.FromResult<IRenderFilterNode>(node);
                }

                WhatChanged = m_ShapesContext.IsInvalidated ? RenderAspects.Shapes : 0;

                InvalidateContexts();
                CreateContexts(context);

                return Task.FromResult<IRenderFilterNode>(this);
            }

            private void CreateContexts(ComputeContext context)
            {
                m_MeshContext = new("BlendShapeModifierPreview.Node.MeshContext");
                m_MeshContext.Invalidates(context);

                m_ShapesContext = new("BlendShapeModifierPreview.Node.ShapesContext");
                m_ShapesContext.Invalidates(context);

                foreach (var modifier in m_Modifiers)
                {
                    m_MeshContext.Observe(modifier, x => x.Shapes.Select(y => y.Clone(0.0f)).ToArray(), Enumerable.SequenceEqual);
                    m_ShapesContext.Observe(modifier, x => x.Shapes.Select(y => y.Weight).ToArray(), Enumerable.SequenceEqual);
                }
            }

            private void InvalidateContexts()
            {
                m_MeshContext.Invalidate();
                m_ShapesContext.Invalidate();
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (proxy is not SkinnedMeshRenderer renderer)
                {
                    return;
                }

                renderer.sharedMesh = m_Mesh;
                BlendShapeModifierProcessor.ApplyWeights(renderer, m_Modifiers);
            }

            public void Dispose()
            {
                InvalidateContexts();

                Object.DestroyImmediate(m_Mesh);
            }
        }
    }
}
