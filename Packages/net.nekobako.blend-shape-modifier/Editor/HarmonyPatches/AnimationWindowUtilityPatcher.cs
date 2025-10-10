using UnityEditor;
using UnityEngine.Serialization;
using HarmonyLib;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal static class AnimationWindowUtilityPatcher
    {
        private const string k_PatchId = "net.nekobako.blend-shape-modifier.animation-window-utility-patcher";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var harmony = new Harmony(k_PatchId);

            harmony.Patch(AccessTools.Method("UnityEditorInternal.AnimationWindowUtility:GetNicePropertyDisplayName", new[]
            {
                typeof(EditorCurveBinding),
                typeof(SerializedObject),
            }), postfix: new(typeof(AnimationWindowUtilityPatcher), nameof(GetNicePropertyDisplayName_Postfix)));

            AssemblyReloadEvents.beforeAssemblyReload += () => harmony.UnpatchAll(k_PatchId);
        }

        private static void GetNicePropertyDisplayName_Postfix(EditorCurveBinding curveBinding, SerializedObject so, ref string __result)
        {
            if (so != null
                && curveBinding.isSerializeReferenceCurve
                && long.TryParse(curveBinding.propertyName[(curveBinding.propertyName.IndexOf('[') + 1)..curveBinding.propertyName.IndexOf(']')], out var id)
                && ManagedReferenceUtility.GetManagedReference(so.targetObject, id) is BlendShape shape)
            {
                __result = $"{__result} ({shape.Name})";
            }
        }
    }
}
