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
            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_NameProperty, GUIUtils.TrText("sample-expression-name"), true), true);
            EditorGUI.PropertyField(rect, m_NameProperty, GUIUtils.TrText("sample-expression-name"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, GUIUtils.TrText("sample-expression-weight"), true));
            var propertyContent = EditorGUI.BeginProperty(rect, GUIUtils.TrText("sample-expression-weight"), m_WeightProperty);

            var propertyRect = EditorGUI.PrefixLabel(rect, propertyContent);
            EditorGUI.DelayedFloatField(propertyRect, m_WeightProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_NameProperty, GUIUtils.TrText("sample-expression-name"), true), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, GUIUtils.TrText("sample-expression-weight"), true));

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
