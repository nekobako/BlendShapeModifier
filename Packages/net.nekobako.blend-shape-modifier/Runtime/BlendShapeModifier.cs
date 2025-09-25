#if BSM_VRCSDK3_AVATARS

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [HelpURL("https://blend-shape-modifier.nekobako.net")]
    internal class BlendShapeModifier : MonoBehaviour, IEditorOnly
    {
        [SerializeField, NotKeyable]
        public SkinnedMeshRenderer Renderer = null;

        [SerializeReference, NotKeyable]
        public List<BlendShape> Shapes = new();
    }
}

#endif
