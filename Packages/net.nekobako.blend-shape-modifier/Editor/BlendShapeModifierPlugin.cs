using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using net.nekobako.BlendShapeModifier.Editor;

[assembly: ExportsPlugin(typeof(BlendShapeModifierPlugin))]

namespace net.nekobako.BlendShapeModifier.Editor
{
    internal class BlendShapeModifierPlugin : Plugin<BlendShapeModifierPlugin>
    {
        public override string QualifiedName => "net.nekobako.blend-shape-modifier";
        public override string DisplayName => "Blend Shape Modifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .WithRequiredExtension(typeof(AnimatorServicesContext), x => x
                    .Run(BlendShapeModifierPass.Instance)
                    .PreviewingWith(new BlendShapeModifierPreview()));
        }
    }
}
