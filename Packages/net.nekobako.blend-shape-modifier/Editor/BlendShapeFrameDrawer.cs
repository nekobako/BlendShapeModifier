#if BSM_VRCSDK3_AVATARS

using System;
using UnityEditor;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFrameDrawer : IDisposable
    {
        private static readonly Lazy<RectOffset> s_HeaderPadding = new(() => new(21, 7, 1, -1));
        private static readonly Lazy<RectOffset> s_ExpressionPadding = new(() => new(21, 7, 6, 6));
        private static readonly Lazy<GUIStyle> s_HeaderStyle = new(() => new("RL Header"));
        private static readonly Lazy<GUIStyle> s_ExpressionStyle = new(() => new("RL Background"));

        private readonly BlendShapeFrame m_Frame = null;
        private readonly SerializedProperty m_Property = null;
        private readonly SerializedProperty m_ExpressionProperty = null;
        private BlendShapeExpressionDrawer m_ExpressionDrawer = null;

        public BlendShapeFrameDrawer(SerializedProperty property)
        {
            m_Frame = property.managedReferenceValue as BlendShapeFrame;
            m_Property = property;
            m_ExpressionProperty = property.FindPropertyRelative(nameof(BlendShapeFrame.Expression));
        }

        public bool IsValid(SerializedProperty property)
        {
            return ReferenceEquals(m_Frame, property.managedReferenceValue) && SerializedProperty.EqualContents(m_Property, property);
        }

        public void DrawInspectorGUI(Rect rect)
        {
            rect = GUIUtils.Line(rect, true);
            GUI.Box(rect, GUIContent.none, s_HeaderStyle.Value);
            EditorGUI.LabelField(s_HeaderPadding.Value.Remove(rect), CL4EE.Tr("expression"));

            rect = GUIUtils.Line(rect, GetExpressionDrawer().CalcInspectorHeight() + s_ExpressionPadding.Value.vertical);
            GUI.Box(rect, GUIContent.none, s_ExpressionStyle.Value);
            GetExpressionDrawer().DrawInspectorGUI(s_ExpressionPadding.Value.Remove(rect));
        }

        public float CalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, true);

            rect = GUIUtils.Line(rect, GetExpressionDrawer().CalcInspectorHeight() + s_ExpressionPadding.Value.vertical);

            return rect.yMax;
        }

        public void DrawSceneGUI()
        {
            GetExpressionDrawer().DrawSceneGUI();
        }

        public void Dispose()
        {
            m_ExpressionDrawer?.Dispose();
            m_ExpressionDrawer = null;
        }

        private BlendShapeExpressionDrawer GetExpressionDrawer()
        {
            if (m_ExpressionDrawer != null && m_ExpressionDrawer.IsValid(m_ExpressionProperty))
            {
                return m_ExpressionDrawer;
            }

            m_ExpressionDrawer?.Dispose();
            m_ExpressionDrawer = BlendShapeExpressionDrawer.Create(m_ExpressionProperty);
            return m_ExpressionDrawer;
        }
    }
}

#endif
