using System;
using System.Buffers;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFilterByAxisExpressionDrawer : BlendShapeExpressionDrawer<BlendShapeFilterByAxisExpression>
    {
        private const float k_ButtonSpacing = 2.0f;
        private const float k_DottedLineSize = 4.0f;
        private static readonly int[] s_LineIndices = { 0, 2, 4, 6, 0, 4, 2, 6, 1, 3, 5, 7, 1, 5, 3, 7 };
        private static readonly int[] s_DottedLineIndices = { 0, 1, 2, 3, 4, 5, 6, 7 };
        private static readonly Color s_HandleColor = new(0.0f, 0.0f, 0.0f, 0.4f);
        private static readonly Color s_ActiveHandleColor = new(0.0f, 0.0f, 0.0f, 0.8f);
        private static readonly Color s_InactiveHandleColor = new(0.0f, 0.0f, 0.0f, 0.2f);
        private static readonly Lazy<RectOffset> s_ExpressionPadding = new(() => new(21, 7, 5, 6));
        private static readonly Lazy<GUIStyle> s_HeaderStyle = new(() => new("RL Empty Header"));
        private static readonly Lazy<GUIStyle> s_ExpressionStyle = new(() => new("RL Background"));
        private static BlendShapeFilterByAxisExpressionDrawer s_EditingDrawer = null;

        private readonly SerializedProperty m_Property = null;
        private readonly SerializedProperty m_PositionProperty = null;
        private readonly SerializedProperty m_DirectionProperty = null;
        private readonly SerializedProperty m_FalloffRangeProperty = null;
        private readonly SerializedProperty m_ExpressionProperty = null;
        private BlendShapeExpressionDrawer m_ExpressionDrawer = null;
        private Quaternion m_Rotation = Quaternion.identity;
        private Vector3 m_Direction = Vector3.forward;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(x => new BlendShapeFilterByAxisExpressionDrawer(x));
        }

        private static bool IsEditing()
        {
            return s_EditingDrawer != null;
        }

        private static bool IsEditing(BlendShapeFilterByAxisExpressionDrawer drawer)
        {
            return s_EditingDrawer == drawer;
        }

        private static void SetEditing(BlendShapeFilterByAxisExpressionDrawer drawer)
        {
            s_EditingDrawer = drawer;

            Tools.hidden = drawer != null;
            SceneView.RepaintAll();
        }

        private BlendShapeFilterByAxisExpressionDrawer(SerializedProperty property) : base(property)
        {
            m_Property = property;
            m_PositionProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByAxisExpression.Position));
            m_DirectionProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByAxisExpression.Direction));
            m_FalloffRangeProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByAxisExpression.FalloffRange));
            m_ExpressionProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByAxisExpression.Expression));

            SceneView.RepaintAll();
        }

        protected override void OnDrawInspectorGUI(Rect rect)
        {
            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_PositionProperty, GUIUtils.TrText("filter-by-axis-expression-position"), true), true);
            EditorGUI.PropertyField(rect, m_PositionProperty, GUIUtils.TrText("filter-by-axis-expression-position"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_DirectionProperty, GUIUtils.TrText("filter-by-axis-expression-direction"), true));
            EditorGUI.PropertyField(rect, m_DirectionProperty, GUIUtils.TrText("filter-by-axis-expression-direction"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_FalloffRangeProperty, GUIUtils.TrText("filter-by-axis-expression-falloff-range"), true));
            EditorGUI.PropertyField(rect, m_FalloffRangeProperty, GUIUtils.TrText("filter-by-axis-expression-falloff-range"), true);

            rect = GUIUtils.Line(rect);
            DrawButtons(rect);

            rect = GUIUtils.Line(rect, GetExpressionDrawer().CalcInspectorHeight() + s_ExpressionPadding.Value.vertical);
            GUI.Box(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMin + s_HeaderStyle.Value.fixedHeight), GUIContent.none, s_HeaderStyle.Value);
            GUI.Box(Rect.MinMaxRect(rect.xMin, rect.yMin + s_HeaderStyle.Value.fixedHeight, rect.xMax, rect.yMax), GUIContent.none, s_ExpressionStyle.Value);
            GetExpressionDrawer().DrawInspectorGUI(s_ExpressionPadding.Value.Remove(rect));
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_PositionProperty, GUIUtils.TrText("filter-by-axis-expression-position"), true), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_DirectionProperty, GUIUtils.TrText("filter-by-axis-expression-direction"), true));

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_FalloffRangeProperty, GUIUtils.TrText("filter-by-axis-expression-falloff-range"), true));

            rect = GUIUtils.Line(rect);

            rect = GUIUtils.Line(rect, GetExpressionDrawer().CalcInspectorHeight() + s_ExpressionPadding.Value.vertical);

            return rect.yMax;
        }

        protected override void OnDrawSceneGUI()
        {
            if (m_Property.serializedObject.targetObject is BlendShapeModifier modifier && modifier.Renderer)
            {
                var position = modifier.Renderer.transform.TransformPoint(m_PositionProperty.vector3Value);
                var direction = modifier.Renderer.transform.TransformDirection(m_DirectionProperty.vector3Value);
                if (m_Direction != direction)
                {
                    m_Rotation = Quaternion.LookRotation(direction, modifier.Renderer.transform.up);
                    m_Direction = direction;
                }

                var handlesColor = Handles.color;
                Handles.color =
                    IsEditing(this) ? s_ActiveHandleColor :
                    IsEditing() ? s_InactiveHandleColor :
                    s_HandleColor;

                var size = HandleUtility.GetHandleSize(position);
                var offset = m_FalloffRangeProperty.floatValue * 0.5f;
                var points = ArrayPool<Vector3>.Shared.Rent(8);
                points[0] = position + m_Rotation * new Vector3(-size, -size, -offset);
                points[1] = position + m_Rotation * new Vector3(-size, -size, +offset);
                points[2] = position + m_Rotation * new Vector3(-size, +size, -offset);
                points[3] = position + m_Rotation * new Vector3(-size, +size, +offset);
                points[4] = position + m_Rotation * new Vector3(+size, -size, -offset);
                points[5] = position + m_Rotation * new Vector3(+size, -size, +offset);
                points[6] = position + m_Rotation * new Vector3(+size, +size, -offset);
                points[7] = position + m_Rotation * new Vector3(+size, +size, +offset);
                Handles.DrawLines(points, s_LineIndices);
                Handles.DrawDottedLines(points, s_DottedLineIndices, k_DottedLineSize);
                Handles.ArrowHandleCap(0, position, m_Rotation, size, EventType.Repaint);
                ArrayPool<Vector3>.Shared.Return(points);

                Handles.color = handlesColor;

                if (IsEditing(this))
                {
                    EditorGUI.BeginChangeCheck();

                    position = Handles.PositionHandle(position, m_Rotation);
                    m_Rotation = Handles.RotationHandle(m_Rotation, position);
                    direction = m_Rotation * Vector3.forward;
                    m_Direction = direction;

                    if (EditorGUI.EndChangeCheck())
                    {
                        m_PositionProperty.vector3Value = modifier.Renderer.transform.InverseTransformPoint(position);
                        m_DirectionProperty.vector3Value = modifier.Renderer.transform.InverseTransformDirection(direction);
                    }
                }
            }

            GetExpressionDrawer().DrawSceneGUI();
        }

        protected override void OnDispose()
        {
            m_ExpressionDrawer?.Dispose();
            m_ExpressionDrawer = null;

            if (IsEditing(this))
            {
                SetEditing(null);
            }

            SceneView.RepaintAll();
        }

        private void DrawButtons(Rect rect)
        {
            EditorGUI.BeginChangeCheck();

            var edit = GUI.Toggle(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.center.x - k_ButtonSpacing * 0.5f, rect.yMax), IsEditing(this), GUIUtils.TrText("filter-by-axis-expression-edit"), GUI.skin.button);

            if (EditorGUI.EndChangeCheck())
            {
                SetEditing(edit ? this : null);
            }

            if (GUI.Button(Rect.MinMaxRect(rect.center.x + k_ButtonSpacing * 0.5f, rect.yMin, rect.xMax, rect.yMax), GUIUtils.TrText("filter-by-axis-expression-flip")))
            {
                m_DirectionProperty.vector3Value *= -1.0f;
            }
        }

        private BlendShapeExpressionDrawer GetExpressionDrawer()
        {
            if (m_ExpressionDrawer != null && m_ExpressionDrawer.IsValid(m_ExpressionProperty))
            {
                return m_ExpressionDrawer;
            }

            m_ExpressionDrawer?.Dispose();
            m_ExpressionDrawer = Create(m_ExpressionProperty);
            return m_ExpressionDrawer;
        }
    }
}
