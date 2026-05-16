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
                .Select(x => RenderGroup.For(x.Key).WithData(x.ToArray(), Enumerable.SequenceEqual))
                .ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context)
        {
            var modifiers = group.GetData<BlendShapeModifier[]>();
            var node = new Node(pairs, modifiers, context);
            return Task.FromResult<IRenderFilterNode>(node);
        }

        private class Node : IRenderFilterNode
        {
            private const string k_MeshContextDescription = "BlendShapeModifierPreview.Node.MeshContext";
            private const string k_ShapesContextDescription = "BlendShapeModifierPreview.Node.ShapesContext";

            private readonly ComputeContext m_MeshContext = null;
            private ComputeContext m_ShapesContext = null;
            private readonly BlendShapeModifier[] m_Modifiers = null;
            private readonly Mesh m_Mesh = null;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Mesh | RenderAspects.Shapes;

            public Node(IEnumerable<(Renderer, Renderer)> pairs, BlendShapeModifier[] modifiers, ComputeContext context)
            {
                var (original, proxy) = pairs.Single();
                m_MeshContext = new(k_MeshContextDescription);
                m_ShapesContext = new(k_ShapesContextDescription);
                BlendShapeModifierProcessor.ProcessMesh(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, modifiers, m_MeshContext);
                BlendShapeModifierProcessor.ProcessShapes(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, modifiers, m_ShapesContext);

                m_Modifiers = modifiers;
                if (proxy is SkinnedMeshRenderer renderer)
                {
                    m_Mesh = renderer.sharedMesh;
                }

                m_MeshContext.Invalidates(context);
                m_ShapesContext.Invalidates(context);
            }

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context, RenderAspects aspects)
            {
                if (m_MeshContext.IsInvalidated ||
                    aspects.HasFlag(RenderAspects.Mesh))
                {
                    // Returning null here forcibly passes RenderAspects.Everything to Refresh() of downstream nodes
                    // return Task.FromResult<IRenderFilterNode>(null);

                    var node = new Node(pairs, m_Modifiers, context);
                    return Task.FromResult<IRenderFilterNode>(node);
                }

                WhatChanged = 0;

                if (m_ShapesContext.IsInvalidated)
                {
                    WhatChanged |= RenderAspects.Shapes;

                    var (original, proxy) = pairs.Single();
                    m_ShapesContext = new(k_ShapesContextDescription);
                    BlendShapeModifierProcessor.ProcessShapes(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, m_Modifiers, m_ShapesContext);
                }

                m_MeshContext.Invalidates(context);
                m_ShapesContext.Invalidates(context);

                return Task.FromResult<IRenderFilterNode>(this);
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (proxy is SkinnedMeshRenderer renderer)
                {
                    renderer.sharedMesh = m_Mesh;
                }
                BlendShapeModifierProcessor.ProcessShapes(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, m_Modifiers, ComputeContext.NullContext);
            }

            public void Dispose()
            {
                m_MeshContext.Invalidate();
                m_ShapesContext.Invalidate();

                Object.DestroyImmediate(m_Mesh);
            }
        }
    }
}
