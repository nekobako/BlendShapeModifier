using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeModifierPass : Pass<BlendShapeModifierPass>
    {
        public override string QualifiedName => "net.nekobako.blend-shape-modifier";
        public override string DisplayName => "Blend Shape Modifier";

        protected override void Execute(BuildContext context)
        {
            var modifiers = context.AvatarRootObject.GetComponentsInChildren<BlendShapeModifier>(true);

            foreach (var modifier in modifiers
                .Where(x => x.Renderer && x.Renderer.sharedMesh))
            {
                modifier.Renderer.sharedMesh = BlendShapeModifierProcessor.GenerateMesh(modifier.Renderer, modifier);

                BlendShapeModifierProcessor.ApplyWeights(modifier.Renderer, modifier);

                var asc = context.Extension<AnimatorServicesContext>();
                var map = new Dictionary<EditorCurveBinding, EditorCurveBinding>();
                foreach (var shape in modifier.Shapes)
                {
                    var source = EditorCurveBinding.SerializeReferenceCurve(
                        asc.ObjectPathRemapper.GetVirtualPathForObject(modifier.transform),
                        modifier.GetType(),
                        ManagedReferenceUtility.GetManagedReferenceIdForObject(modifier, shape),
                        nameof(BlendShape.Weight),
                        false, false);
                    var target = EditorCurveBinding.FloatCurve(
                        asc.ObjectPathRemapper.GetVirtualPathForObject(modifier.Renderer.transform),
                        modifier.Renderer.GetType(),
                        $"blendShape.{shape.Name}");
                    map[source] = target;
                }

                asc.AnimationIndex.EditClipsByBinding(map.Keys, clip =>
                {
                    foreach (var (source, target) in map)
                    {
                        var curve = clip.GetFloatCurve(source);
                        clip.SetFloatCurve(source, null);
                        clip.SetFloatCurve(target, curve);
                    }
                });
            }

            foreach (var modifier in modifiers)
            {
                Object.DestroyImmediate(modifier);
            }
        }
    }
}
