using System;
using UnityEditor;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    [CustomPropertyDrawer(typeof(BlendShapeNameAttribute))]
    internal class BlendShapeNameDrawer : PropertyDrawer
    {
        private static readonly Lazy<GUIStyle> s_DropDownStyle = new(() => new("TextFieldDropDown"));
        private static readonly Lazy<GUIStyle> s_DropDownTextStyle = new(() => new("TextFieldDropDownText"));

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent content)
        {
            content = EditorGUI.BeginProperty(rect, content, property);

            property.stringValue = EditorGUI.DelayedTextField(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax - s_DropDownStyle.Value.fixedWidth, rect.yMax), content, property.stringValue, s_DropDownTextStyle.Value);

            if (EditorGUI.DropdownButton(Rect.MinMaxRect(rect.xMax - s_DropDownStyle.Value.fixedWidth, rect.yMin, rect.xMax, rect.yMax), GUIContent.none, FocusType.Keyboard, s_DropDownStyle.Value))
            {
                var dropdown = new GUIUtils.GenericDropdown(CL4EE.Tr("shape"));

                if (property.serializedObject.targetObject is BlendShapeModifier modifier
                    && modifier.Renderer
                    && modifier.Renderer.sharedMesh)
                {
                    var item = dropdown.AddItem(modifier.Renderer.name);
                    for (var i = 0; i < modifier.Renderer.sharedMesh.blendShapeCount; i++)
                    {
                        var name = modifier.Renderer.sharedMesh.GetBlendShapeName(i);
                        item.AddItem(name, () => SetShapeName(name));
                    }
                }

                dropdown.Show(rect);

                void SetShapeName(string name)
                {
                    property.serializedObject.Update();

                    property.stringValue = name;

                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent content)
        {
            return EditorGUI.GetPropertyHeight(property, content, true);
        }
    }
}
