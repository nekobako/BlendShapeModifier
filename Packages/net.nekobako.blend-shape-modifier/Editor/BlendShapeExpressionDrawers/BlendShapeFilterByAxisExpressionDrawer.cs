using System;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFilterByAxisExpressionDrawer : BlendShapeExpressionDrawer<BlendShapeFilterByAxisExpression>
    {
        private const float k_ButtonSpacing = 2.0f;
        private const float k_HandleSize = 2.0f;
        private const float k_HandleThickness = 2.0f;
        private static readonly Color s_HandleColor = new(1.0f, 1.0f, 1.0f, 0.4f);
        private static readonly Color s_ActiveHandleColor = new(1.0f, 1.0f, 1.0f, 0.8f);
        private static readonly Color s_InactiveHandleColor = new(1.0f, 1.0f, 1.0f, 0.2f);
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
        private Quaternion m_HandleRotation = Quaternion.identity;

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

                var handlesColor = Handles.color;
                Handles.color =
                    IsEditing(this) ? s_ActiveHandleColor :
                    IsEditing() ? s_InactiveHandleColor :
                    s_HandleColor;

                var size = HandleUtility.GetHandleSize(position) * k_HandleSize;
                var vector = modifier.Renderer.transform.TransformVector(m_DirectionProperty.vector3Value);
                Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(vector, modifier.Renderer.transform.up), size, EventType.Repaint);

                var normal = modifier.Renderer.transform.worldToLocalMatrix.transpose.MultiplyVector(m_DirectionProperty.vector3Value).normalized;
                if (m_FalloffRangeProperty.floatValue == 0.0f)
                {
                    Handles.DrawWireDisc(position, normal, size, k_HandleThickness);
                }
                else
                {
                    var offset = modifier.Renderer.transform.TransformVector(m_DirectionProperty.vector3Value.normalized * (m_FalloffRangeProperty.floatValue * 0.5f));
                    Handles.DrawWireDisc(position + offset, normal, size, k_HandleThickness);
                    Handles.DrawWireDisc(position - offset, normal, size, k_HandleThickness);
                }

                Handles.color = handlesColor;

                if (IsEditing(this))
                {
                    EditorGUI.BeginChangeCheck();

                    m_HandleRotation = Tools.pivotRotation switch
                    {
                        PivotRotation.Local when direction.normalized != m_HandleRotation * Vector3.forward => Quaternion.LookRotation(direction, modifier.Renderer.transform.up),
                        PivotRotation.Global when GUIUtility.hotControl == 0 => Quaternion.identity,
                        _ => m_HandleRotation
                    };

                    var rotation = Handles.RotationHandle(m_HandleRotation, position);
                    position = Handles.PositionHandle(position, rotation);
                    direction = rotation * Quaternion.Inverse(m_HandleRotation) * direction.normalized;

                    m_HandleRotation = rotation;

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
