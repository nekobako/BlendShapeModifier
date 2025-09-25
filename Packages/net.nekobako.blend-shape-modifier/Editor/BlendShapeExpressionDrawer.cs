#if BSM_VRCSDK3_AVATARS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal abstract class BlendShapeExpressionDrawer : IDisposable
    {
        private static readonly Dictionary<Type, Func<SerializedProperty, BlendShapeExpressionDrawer>> s_Creators = new();
        private static readonly (Type type, string name)[] s_TypeNames = TypeCache.GetTypesDerivedFrom<IBlendShapeExpression>()
            .Select(x => (type: x, attribute: x.GetCustomAttribute<BlendShapeExpressionAttribute>()))
            .Where(x => x.type.IsClass && !x.type.IsAbstract && x.attribute != null)
            .OrderBy(x => x.attribute.Type)
            .Select(x => (x.type, x.attribute.Name))
            .ToArray();
        private static readonly Type[] s_Types = s_TypeNames
            .Select(x => x.type)
            .ToArray();
        private static readonly string[] s_Names = s_TypeNames
            .Select(x => x.name)
            .ToArray();

        private readonly IBlendShapeExpression m_Expression = null;
        private readonly SerializedProperty m_Property = null;

        protected static void Register<T>(Func<SerializedProperty, BlendShapeExpressionDrawer> creator) where T : IBlendShapeExpression
        {
            s_Creators[typeof(T)] = creator;
        }

        public static BlendShapeExpressionDrawer Create(SerializedProperty property)
        {
            return s_Creators[property.managedReferenceValue.GetType()].Invoke(property);
        }

        protected BlendShapeExpressionDrawer(SerializedProperty property)
        {
            m_Expression = property.managedReferenceValue as IBlendShapeExpression;
            m_Property = property;
        }

        public bool IsValid(SerializedProperty property)
        {
            return ReferenceEquals(m_Expression, property.managedReferenceValue) && SerializedProperty.EqualContents(m_Property, property);
        }

        public void DrawInspectorGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();

            rect = GUIUtils.Line(rect, true);
            EditorGUI.BeginProperty(rect, GUIContent.none, m_Property);

            var type = EditorGUI.Popup(rect, Array.IndexOf(s_Types, m_Property.managedReferenceValue.GetType()), s_Names);

            EditorGUI.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                m_Property.managedReferenceValue = Activator.CreateInstance(s_Types[type]);
                return;
            }

            rect = GUIUtils.Line(rect, OnCalcInspectorHeight());
            OnDrawInspectorGUI(rect);
        }

        public float CalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, true);

            rect = GUIUtils.Line(rect, OnCalcInspectorHeight());

            return rect.yMax;
        }

        public void DrawSceneGUI()
        {
            OnDrawSceneGUI();
        }

        public void Dispose()
        {
            OnDispose();
        }

        protected abstract void OnDrawInspectorGUI(Rect rect);
        protected abstract float OnCalcInspectorHeight();
        protected abstract void OnDrawSceneGUI();
        protected abstract void OnDispose();
    }

    internal abstract class BlendShapeExpressionDrawer<T> : BlendShapeExpressionDrawer where T : IBlendShapeExpression
    {
        protected static void Register(Func<SerializedProperty, BlendShapeExpressionDrawer<T>> creator)
        {
            Register<T>(creator);
        }

        protected BlendShapeExpressionDrawer(SerializedProperty property) : base(property)
        {
        }
    }
}

#endif
