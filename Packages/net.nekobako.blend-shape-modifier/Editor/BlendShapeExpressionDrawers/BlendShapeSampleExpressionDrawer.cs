#if BSM_VRCSDK3_AVATARS

using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeSampleExpressionDrawer : BlendShapeExpressionDrawer<BlendShapeSampleExpression>
    {
        private readonly SerializedProperty m_NameProperty = null;
        private readonly SerializedProperty m_WeightProperty = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(x => new BlendShapeSampleExpressionDrawer(x));
        }

        private BlendShapeSampleExpressionDrawer(SerializedProperty property) : base(property)
        {
            m_NameProperty = property.FindPropertyRelative(nameof(BlendShapeSampleExpression.Name));
            m_WeightProperty = property.FindPropertyRelative(nameof(BlendShapeSampleExpression.Weight));
        }

        protected override void OnDrawInspectorGUI(Rect rect)
        {
            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_NameProperty, true), true);
            EditorGUI.PropertyField(rect, m_NameProperty, true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, true));
            var propertyContent = EditorGUI.BeginProperty(rect, GUIUtils.Text(m_WeightProperty.displayName), m_WeightProperty);

            var propertyRect = EditorGUI.PrefixLabel(rect, propertyContent);
            EditorGUI.DelayedFloatField(propertyRect, m_WeightProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_NameProperty, true), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, true));

            return rect.yMax;
        }

        protected override void OnDrawSceneGUI()
        {
        }

        protected override void OnDispose()
        {
        }
    }
}

#endif
