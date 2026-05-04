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
                .Select(x => RenderGroup.For(x.Renderer).WithData(x))
                .ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context)
        {
            var modifier = group.GetData<BlendShapeModifier>();
            var node = new Node(pairs, modifier, context);
            return Task.FromResult<IRenderFilterNode>(node);
        }

        private class Node : IRenderFilterNode
        {
            private const string k_MeshContextDescription = "BlendShapeModifierPreview.Node.MeshContext";
            private const string k_ShapesContextDescription = "BlendShapeModifierPreview.Node.ShapesContext";

            private readonly BlendShapeModifier m_Modifier = null;
            private readonly Mesh m_Mesh = null;
            private readonly ComputeContext m_MeshContext = null;
            private ComputeContext m_ShapesContext = null;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Mesh | RenderAspects.Shapes;

            public Node(IEnumerable<(Renderer, Renderer)> pairs, BlendShapeModifier modifier, ComputeContext context)
            {
                var (original, proxy) = pairs.Single();
                m_MeshContext = new(k_MeshContextDescription);
                m_ShapesContext = new(k_ShapesContextDescription);
                BlendShapeModifierProcessor.ProcessMesh(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, modifier, m_MeshContext);
                BlendShapeModifierProcessor.ProcessShapes(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, modifier, m_ShapesContext);

                m_Modifier = modifier;
                if (proxy is SkinnedMeshRenderer renderer)
                {
                    m_Mesh = renderer.sharedMesh;
                }

                m_MeshContext.Invalidates(context);
                m_ShapesContext.Invalidates(context);
            }

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context, RenderAspects aspects)
            {
                if (aspects.HasFlag(RenderAspects.Mesh) || m_MeshContext.IsInvalidated)
                {
                    // Returning null here forcibly passes RenderAspects.Everything to Refresh() of downstream nodes
                    // return Task.FromResult<IRenderFilterNode>(null);

                    var node = new Node(pairs, m_Modifier, context);
                    return Task.FromResult<IRenderFilterNode>(node);
                }

                WhatChanged = 0;

                if (m_ShapesContext.IsInvalidated)
                {
                    WhatChanged |= RenderAspects.Shapes;

                    var (original, proxy) = pairs.Single();
                    m_ShapesContext = new(k_ShapesContextDescription);
                    BlendShapeModifierProcessor.ProcessShapes(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, m_Modifier, m_ShapesContext);
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
                BlendShapeModifierProcessor.ProcessShapes(original as SkinnedMeshRenderer, proxy as SkinnedMeshRenderer, m_Modifier, ComputeContext.NullContext);
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
