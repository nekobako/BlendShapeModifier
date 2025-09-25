#if BSM_VRCSDK3_AVATARS

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    internal static class GUIUtils
    {
        private static readonly GUIContent s_Content = new();
        private static readonly MethodInfo s_Slider = typeof(EditorGUI).GetMethod(nameof(EditorGUI.Slider), BindingFlags.NonPublic | BindingFlags.Static, null, new[]
        {
            typeof(Rect),
            typeof(SerializedProperty),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(GUIContent),
        }, null);
        private static readonly FieldInfo s_FloatFieldFormatString = typeof(EditorGUI).GetField("kFloatFieldFormatString", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo s_DragNumberValueMethod = typeof(EditorGUI).GetMethod("DragNumberValue", BindingFlags.NonPublic | BindingFlags.Static, null, new[]
        {
            typeof(Rect),
            typeof(int),
            typeof(bool),
            typeof(double).MakeByRefType(),
            typeof(long).MakeByRefType(),
            typeof(double),
        }, null);

        public static GUIContent Text(string text)
        {
            s_Content.text = text;
            s_Content.image = null;
            s_Content.tooltip = string.Empty;
            return s_Content;
        }

        public static void Space()
        {
            Space(EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight);
        }

        public static void Space(float height)
        {
            GUILayout.Space(height);
        }

        public static Rect Line(Rect rect, bool init = false)
        {
            return Line(rect, EditorGUIUtility.singleLineHeight, init);
        }

        public static Rect Line(Rect rect, float height, bool init = false)
        {
            if (!init)
            {
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
            rect.height = height;
            return rect;
        }

        public static int GetSelectedIndex(this ReorderableList list)
        {
            return list.selectedIndices.DefaultIfEmpty(-1).First();
        }

        public static void Slider(Rect rect, SerializedProperty property, float sliderMin, float sliderMax, float fieldMin, float fieldMax, GUIContent content)
        {
            s_Slider.Invoke(null, new object[] { rect, property, sliderMin, sliderMax, fieldMin, fieldMax, content });
        }

        public static string FloatFieldFormatString
        {
            get => s_FloatFieldFormatString.GetValue(null) as string;
            set => s_FloatFieldFormatString.SetValue(null, value);
        }

        public static float DragFloatLabel(Rect rect, string label, float value, float sensitivity, GUIStyle style)
        {
            var parameters = new object[] { rect, GUIUtility.GetControlID(FocusType.Passive), true, (double)value, 0L, sensitivity };
            EditorGUI.LabelField(rect, label, style);
            s_DragNumberValueMethod.Invoke(null, parameters);
            return (float)(double)parameters[3];
        }

        public class GenericDropdown : AdvancedDropdown
        {
            private static readonly Vector2 s_MinSize = new(200.0f, 200.0f);

            private readonly Item m_Root = null;

            public GenericDropdown(string title) : base(new())
            {
                m_Root = new(title);
                minimumSize = s_MinSize;
            }

            public Item AddItem(string name, Action onSelect = null)
            {
                return m_Root.AddItem(name, onSelect);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                return m_Root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is Item selected)
                {
                    selected.OnSelect();
                }
            }

            public class Item : AdvancedDropdownItem
            {
                private readonly Action m_OnSelect = null;

                public Item(string name, Action onSelect = null) : base(name)
                {
                    m_OnSelect = onSelect;
                }

                public Item AddItem(string name, Action onSelect = null)
                {
                    var item = new Item(name, onSelect);
                    AddChild(item);
                    return item;
                }

                public void OnSelect()
                {
                    m_OnSelect?.Invoke();
                }
            }
        }
    }
}

#endif
