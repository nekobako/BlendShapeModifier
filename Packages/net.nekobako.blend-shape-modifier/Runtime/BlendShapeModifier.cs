using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using nadena.dev.ndmf;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [HelpURL("https://blend-shape-modifier.nekobako.net")]
    internal class BlendShapeModifier : MonoBehaviour, INDMFEditorOnly
    {
        [SerializeField, NotKeyable]
        public SkinnedMeshRenderer Renderer = null;

        [SerializeReference, NotKeyable]
        public List<BlendShape> Shapes = new();
    }
}
