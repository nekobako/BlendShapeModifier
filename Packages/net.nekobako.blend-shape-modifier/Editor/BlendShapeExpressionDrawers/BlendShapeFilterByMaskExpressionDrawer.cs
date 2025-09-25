#if BSM_VRCSDK3_AVATARS

using System;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeFilterByMaskExpressionDrawer : BlendShapeExpressionDrawer<BlendShapeFilterByMaskExpression>
    {
        public const string MaskTextureEditorToken = "net.nekobako.blend-shape-modifier.blend-shape-filter-by-mask-expression-drawer";
        private const float k_MaskButtonWidth = 50.0f;
        private const float k_MaskButtonSpacing = 2.0f;
        private static readonly Vector2Int s_DefaultMaskSize = new(1024, 1024);
        private static readonly Color s_DefaultMaskColor = Color.white;
        private static readonly Lazy<RectOffset> s_ExpressionPadding = new(() => new(21, 7, 5, 6));
        private static readonly Lazy<GUIStyle> s_HeaderStyle = new(() => new("RL Empty Header"));
        private static readonly Lazy<GUIStyle> s_ExpressionStyle = new(() => new("RL Background"));

        private readonly SerializedProperty m_SlotProperty = null;
        private readonly SerializedProperty m_MaskProperty = null;
        private readonly SerializedProperty m_ExpressionProperty = null;
        private BlendShapeExpressionDrawer m_ExpressionDrawer = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(x => new BlendShapeFilterByMaskExpressionDrawer(x));
        }

        private BlendShapeFilterByMaskExpressionDrawer(SerializedProperty property) : base(property)
        {
            m_SlotProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByMaskExpression.Slot));
            m_MaskProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByMaskExpression.Mask));
            m_ExpressionProperty = property.FindPropertyRelative(nameof(BlendShapeFilterByMaskExpression.Expression));
        }

        protected override void OnDrawInspectorGUI(Rect rect)
        {
            EditorGUI.BeginDisabledGroup(IsOpenMaskTextureEditor());

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_SlotProperty, true), true);
            var propertyContent = EditorGUI.BeginProperty(rect, GUIUtils.Text(m_SlotProperty.displayName), m_SlotProperty);

            var propertyRect = EditorGUI.PrefixLabel(rect, propertyContent);
            if (EditorGUI.DropdownButton(propertyRect, GUIUtils.Text($"Element {m_SlotProperty.intValue}"), FocusType.Keyboard))
            {
                var menu = new GenericMenu();

                if (m_SlotProperty.serializedObject.targetObject is BlendShapeModifier modifier && modifier.Renderer)
                {
                    for (var i = 0; i < modifier.Renderer.sharedMaterials.Length; i++)
                    {
                        var slot = i;
                        menu.AddItem(new($"Element {slot}"), m_SlotProperty.intValue == slot, () => SetSlot(slot));
                    }
                }

                menu.DropDown(propertyRect);

                void SetSlot(int slot)
                {
                    m_SlotProperty.serializedObject.Update();

                    m_SlotProperty.intValue = slot;

                    m_SlotProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_MaskProperty, true));
            EditorGUI.PropertyField(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax - k_MaskButtonWidth - k_MaskButtonSpacing, rect.yMax), m_MaskProperty, true);

            EditorGUI.EndDisabledGroup();

            DrawMaskButton(Rect.MinMaxRect(rect.xMax - k_MaskButtonWidth, rect.yMin, rect.xMax, rect.yMax));

            rect = GUIUtils.Line(rect, GetExpressionDrawer().CalcInspectorHeight() + s_ExpressionPadding.Value.vertical);
            GUI.Box(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMin + s_HeaderStyle.Value.fixedHeight), GUIContent.none, s_HeaderStyle.Value);
            GUI.Box(Rect.MinMaxRect(rect.xMin, rect.yMin + s_HeaderStyle.Value.fixedHeight, rect.xMax, rect.yMax), GUIContent.none, s_ExpressionStyle.Value);
            GetExpressionDrawer().DrawInspectorGUI(s_ExpressionPadding.Value.Remove(rect));
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_SlotProperty, true), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_MaskProperty, true));

            rect = GUIUtils.Line(rect, GetExpressionDrawer().CalcInspectorHeight() + s_ExpressionPadding.Value.vertical);

            return rect.yMax;
        }

        protected override void OnDrawSceneGUI()
        {
            GetExpressionDrawer().DrawSceneGUI();
        }

        protected override void OnDispose()
        {
            m_ExpressionDrawer?.Dispose();
            m_ExpressionDrawer = null;
        }

        private bool IsOpenMaskTextureEditor()
        {
#if BSM_MASK_TEXTURE_EDITOR
            return m_MaskProperty.objectReferenceValue is Texture2D mask
                && m_MaskProperty.serializedObject.targetObject is BlendShapeModifier modifier
                && MaskTextureEditor.Editor.Window.IsOpenFor(mask, modifier.Renderer, m_SlotProperty.intValue, MaskTextureEditorToken);
#else
            return false;
#endif
        }

        private void DrawMaskButton(Rect rect)
        {
            if (m_MaskProperty.objectReferenceValue is not Texture2D mask)
            {
                DrawCreateMaskButton(rect);
            }
            else
            {
                DrawEditMaskButton(rect, mask);
            }
        }

        private void DrawCreateMaskButton(Rect rect)
        {
            if (GUI.Button(rect, "Create"))
            {
#if BSM_MASK_TEXTURE_EDITOR
                var mask = MaskTextureEditor.Editor.Utility.CreateTexture(s_DefaultMaskSize, s_DefaultMaskColor);

                m_MaskProperty.serializedObject.Update();

                m_MaskProperty.objectReferenceValue = mask;

                m_MaskProperty.serializedObject.ApplyModifiedProperties();

                if (mask && m_MaskProperty.serializedObject.targetObject is BlendShapeModifier modifier)
                {
                    MaskTextureEditor.Editor.Window.TryOpen(mask, modifier.Renderer, m_SlotProperty.intValue, MaskTextureEditorToken);
                }
#else
                ShowMaskTextureEditorDialog();
#endif

                // Exit GUI to avoid "InvalidOperationException: Stack empty."
                GUIUtility.ExitGUI();
            }
        }

        private void DrawEditMaskButton(Rect rect, Texture2D mask)
        {
            EditorGUI.BeginChangeCheck();

            var open = GUI.Toggle(rect, IsOpenMaskTextureEditor(), "Edit", GUI.skin.button);

            if (EditorGUI.EndChangeCheck())
            {
#if BSM_MASK_TEXTURE_EDITOR
                if (open && m_MaskProperty.serializedObject.targetObject is BlendShapeModifier modifier)
                {
                    MaskTextureEditor.Editor.Window.TryOpen(mask, modifier.Renderer, m_SlotProperty.intValue, MaskTextureEditorToken);
                }
                else
                {
                    MaskTextureEditor.Editor.Window.TryClose();
                }
#else
                ShowMaskTextureEditorDialog();
#endif

                // Exit GUI to avoid "InvalidOperationException: Stack empty."
                GUIUtility.ExitGUI();
            }
        }

        private void ShowMaskTextureEditorDialog()
        {
            switch (EditorUtility.DisplayDialogComplex(
                "Info",
                "Mask Texture Editor is not installed.",
                "Open VCC/ALCOM",
                "Cancel",
                "Open GitHub"))
            {
                case 0:
                    Application.OpenURL("vcc://vpm/addRepo?url=https://vpm.nekobako.net/index.json");
                    break;
                case 2:
                    Application.OpenURL("https://github.com/nekobako/MaskTextureEditor");
                    break;
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

#endif
