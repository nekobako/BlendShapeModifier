#if BSM_VRCSDK3_AVATARS

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeMergeExpressionDrawer : BlendShapeExpressionDrawer<BlendShapeMergeExpression>
    {
        private readonly SerializedProperty m_ExpressionsProperty = null;
        private readonly ReorderableList m_ReorderableList = null;
        private readonly Dictionary<string, BlendShapeExpressionDrawer> m_ExpressionDrawers = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(x => new BlendShapeMergeExpressionDrawer(x));
        }

        private BlendShapeMergeExpressionDrawer(SerializedProperty property) : base(property)
        {
            m_ExpressionsProperty = property.FindPropertyRelative(nameof(BlendShapeMergeExpression.Expressions));
            m_ReorderableList = new(m_ExpressionsProperty.serializedObject, m_ExpressionsProperty, true, false, true, true)
            {
                headerHeight = 0.0f,
                drawElementCallback = DrawElement,
                elementHeightCallback = ElementHeight,
                onAddCallback = OnAdd,
                onRemoveCallback = OnRemove,
                onSelectCallback = _ => GUIUtility.ExitGUI(),
            };
            m_ExpressionDrawers = new();
        }

        protected override void OnDrawInspectorGUI(Rect rect)
        {
            rect = GUIUtils.Line(rect, m_ReorderableList.GetHeight(), true);
            m_ReorderableList.DoList(rect);
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, m_ReorderableList.GetHeight(), true);

            return rect.yMax;
        }

        protected override void OnDrawSceneGUI()
        {
            for (var i = 0; i < m_ExpressionsProperty.arraySize; i++)
            {
                GetExpressionDrawer(i).DrawSceneGUI();
            }
        }

        protected override void OnDispose()
        {
            foreach (var drawer in m_ExpressionDrawers.Values)
            {
                drawer.Dispose();
            }
            m_ExpressionDrawers.Clear();
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            rect.y = rect.center.y - GetExpressionDrawer(index).CalcInspectorHeight() * 0.5f;
            rect.height = GetExpressionDrawer(index).CalcInspectorHeight();
            GetExpressionDrawer(index).DrawInspectorGUI(rect);
        }

        private float ElementHeight(int index)
        {
            return GetExpressionDrawer(index).CalcInspectorHeight() + EditorGUIUtility.standardVerticalSpacing;
        }

        private void OnAdd(ReorderableList list)
        {
            var newExpressionIndex = m_ExpressionsProperty.arraySize;
            m_ExpressionsProperty.InsertArrayElementAtIndex(newExpressionIndex);

            var newExpressionProperty = m_ExpressionsProperty.GetArrayElementAtIndex(newExpressionIndex);
            newExpressionProperty.managedReferenceValue = CreateExpression();

            m_ReorderableList.Select(newExpressionIndex);
        }

        private void OnRemove(ReorderableList list)
        {
            var selectedExpressionIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedExpressionIndex >= 0 && selectedExpressionIndex < m_ExpressionsProperty.arraySize)
            {
                m_ExpressionsProperty.DeleteArrayElementAtIndex(selectedExpressionIndex);
            }
            else
            {
                m_ExpressionsProperty.DeleteArrayElementAtIndex(m_ExpressionsProperty.arraySize - 1);
            }

            if (m_ExpressionsProperty.arraySize == 0)
            {
                m_ReorderableList.ClearSelection();
            }
            else if (selectedExpressionIndex > m_ExpressionsProperty.arraySize - 1)
            {
                m_ReorderableList.Select(m_ExpressionsProperty.arraySize - 1);
            }
        }

        private IBlendShapeExpression CreateExpression()
        {
            return new BlendShapeSampleExpression();
        }

        private BlendShapeExpressionDrawer GetExpressionDrawer(int index)
        {
            var expressionProperty = m_ExpressionsProperty.GetArrayElementAtIndex(index);
            if (m_ExpressionDrawers.TryGetValue(expressionProperty.propertyPath, out var drawer) && drawer.IsValid(expressionProperty))
            {
                return drawer;
            }

            drawer?.Dispose();
            drawer = Create(expressionProperty);
            return m_ExpressionDrawers[expressionProperty.propertyPath] = drawer;
        }
    }
}

#endif
