#if BSM_VRCSDK3_AVATARS

using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    [CustomEditor(typeof(BlendShapeModifier))]
    internal class BlendShapeModifierEditor : UnityEditor.Editor
    {
        private static readonly Lazy<RectOffset> s_HeaderPadding = new(() => new(15, 1, 0, 0));

        private SerializedProperty m_RendererProperty = null;
        private SerializedProperty m_ShapesProperty = null;
        private ReorderableList m_ReorderableList = null;
        private BlendShapeDrawer m_ShapeDrawer = null;

        private void OnEnable()
        {
            m_RendererProperty = serializedObject.FindProperty(nameof(BlendShapeModifier.Renderer));
            m_ShapesProperty = serializedObject.FindProperty(nameof(BlendShapeModifier.Shapes));
            m_ReorderableList = new(serializedObject, m_ShapesProperty)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                onAddDropdownCallback = OnAddDropdown,
                onRemoveCallback = OnRemove,
                onSelectCallback = _ => GUIUtility.ExitGUI(),
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CL4EE.DrawLanguagePicker();

            GUIUtils.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(GUIUtils.TrText("preview"));
            BlendShapeModifierPreview.PreviewNode.IsEnabled.Value = GUILayout.Toolbar(BlendShapeModifierPreview.PreviewNode.IsEnabled.Value ? 1 : 0, new[] { CL4EE.Tr("disable-preview"), CL4EE.Tr("enable-preview") }) == 1;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_RendererProperty, GUIUtils.TrText("renderer"), true);

            GUIUtils.Space();
            m_ReorderableList.DoLayoutList();

            var selectedShapeIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedShapeIndex >= 0 && selectedShapeIndex < m_ShapesProperty.arraySize)
            {
                GUIUtils.Space();
                GetShapeDrawer(selectedShapeIndex).DrawInspectorGUI(EditorGUILayout.GetControlRect(false, GetShapeDrawer(selectedShapeIndex).CalcInspectorHeight()));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();

            var selectedShapeIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedShapeIndex >= 0 && selectedShapeIndex < m_ShapesProperty.arraySize)
            {
                GetShapeDrawer(selectedShapeIndex).DrawSceneGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            m_ShapeDrawer?.Dispose();
            m_ShapeDrawer = null;
        }

        private void DrawHeader(Rect rect)
        {
            rect = s_HeaderPadding.Value.Remove(rect);
            EditorGUI.LabelField(rect, CL4EE.Tr("shape"), m_ShapesProperty.arraySize > 0 ? CL4EE.Tr("shape-weight") : string.Empty);
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var shapeProperty = m_ShapesProperty.GetArrayElementAtIndex(index);
            var shapeNameProperty = shapeProperty.FindPropertyRelative(nameof(BlendShape.Name));
            var shapeWeightProperty = shapeProperty.FindPropertyRelative(nameof(BlendShape.Weight));
            var shapeFramesProperty = shapeProperty.FindPropertyRelative(nameof(BlendShape.Frames));

            var minWeight = 0.0f;
            var maxWeight = 0.0f;
            if (shapeFramesProperty.arraySize > 0)
            {
                var minFrameProperty = shapeFramesProperty.GetArrayElementAtIndex(0);
                var minFrameWeightProperty = minFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                minWeight = Mathf.Min(0.0f, minFrameWeightProperty.floatValue);

                var maxFrameProperty = shapeFramesProperty.GetArrayElementAtIndex(shapeFramesProperty.arraySize - 1);
                var maxFrameWeightProperty = maxFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                maxWeight = Mathf.Max(0.0f, maxFrameWeightProperty.floatValue);
            }

            rect.y = rect.center.y - EditorGUIUtility.singleLineHeight * 0.5f;
            rect.height = EditorGUIUtility.singleLineHeight;
            var propertyContent = EditorGUI.BeginProperty(rect, GUIUtils.Text(string.IsNullOrEmpty(shapeNameProperty.stringValue) ? " " : shapeNameProperty.stringValue), shapeWeightProperty);

            var propertyRect = EditorGUI.PrefixLabel(rect, propertyContent);
            GUIUtils.Slider(propertyRect, shapeWeightProperty, minWeight, maxWeight, float.MinValue, float.MaxValue, GUIContent.none);

            EditorGUI.EndProperty();
        }

        private void OnAddDropdown(Rect rect, ReorderableList list)
        {
            var dropdown = new GUIUtils.GenericDropdown(CL4EE.Tr("shape"));

            if (target is BlendShapeModifier modifier
                && modifier.Renderer
                && modifier.Renderer.sharedMesh)
            {
                var item = dropdown.AddItem(modifier.Renderer.name);
                for (var i = 0; i < modifier.Renderer.sharedMesh.blendShapeCount; i++)
                {
                    var name = modifier.Renderer.sharedMesh.GetBlendShapeName(i);
                    item.AddItem(name, () => AddShape(modifier.Renderer.sharedMesh, name));
                }
            }

            dropdown.AddItem(CL4EE.Tr("new-shape-name"), () => AddShape(null, CL4EE.Tr("new-shape-name")));

            dropdown.Show(rect);

            void AddShape(Mesh mesh, string name)
            {
                m_ShapesProperty.serializedObject.Update();

                var newShapeIndex = m_ShapesProperty.arraySize;
                m_ShapesProperty.InsertArrayElementAtIndex(newShapeIndex);

                var newShapeProperty = m_ShapesProperty.GetArrayElementAtIndex(newShapeIndex);
                newShapeProperty.managedReferenceValue = CreateShape(mesh, name);

                m_ReorderableList.Select(newShapeIndex);

                m_ShapesProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnRemove(ReorderableList list)
        {
            var selectedShapeIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedShapeIndex >= 0 && selectedShapeIndex < m_ShapesProperty.arraySize)
            {
                m_ShapesProperty.DeleteArrayElementAtIndex(selectedShapeIndex);
            }
            else
            {
                m_ShapesProperty.DeleteArrayElementAtIndex(m_ShapesProperty.arraySize - 1);
            }

            if (m_ShapesProperty.arraySize == 0)
            {
                m_ReorderableList.ClearSelection();
            }
            else if (selectedShapeIndex > m_ShapesProperty.arraySize - 1)
            {
                m_ReorderableList.Select(m_ShapesProperty.arraySize - 1);
            }
        }

        private BlendShape CreateShape(Mesh mesh, string name)
        {
            if (mesh)
            {
                var index = mesh.GetBlendShapeIndex(name);
                return new()
                {
                    Name = name,
                    Frames = Enumerable.Range(0, mesh.GetBlendShapeFrameCount(index))
                        .Select(x => new BlendShapeFrame
                        {
                            Weight = mesh.GetBlendShapeFrameWeight(index, x),
                            Expression = new BlendShapeSampleExpression
                            {
                                Name = name,
                                Weight = mesh.GetBlendShapeFrameWeight(index, x),
                            },
                        })
                        .ToList(),
                };
            }

            return new()
            {
                Name = name,
                Frames = new()
                {
                    new()
                    {
                        Weight = 100.0f,
                        Expression = new BlendShapeSampleExpression(),
                    },
                },
            };
        }

        private BlendShapeDrawer GetShapeDrawer(int index)
        {
            var shapeProperty = m_ShapesProperty.GetArrayElementAtIndex(index);
            if (m_ShapeDrawer != null && m_ShapeDrawer.IsValid(shapeProperty))
            {
                return m_ShapeDrawer;
            }

            m_ShapeDrawer?.Dispose();
            m_ShapeDrawer = new(shapeProperty);
            return m_ShapeDrawer;
        }
    }
}

#endif
