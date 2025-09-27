#if BSM_VRCSDK3_AVATARS

using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace net.nekobako.BlendShapeModifier.Runtime
{
    [HelpURL("https://blend-shape-modifier.nekobako.net")]
    internal class BlendShapeModifier : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        public SkinnedMeshRenderer Renderer = null;

        [SerializeReference]
        public List<BlendShape> Shapes = new();
    }
}

#endif
