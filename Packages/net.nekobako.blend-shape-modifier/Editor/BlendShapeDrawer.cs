#if BSM_VRCSDK3_AVATARS

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BlendShapeModifier.Editor
{
    using Runtime;

    internal class BlendShapeDrawer : IDisposable
    {
        private const float k_FramesHeight = 42.0f;
        private const float k_SnapRange = 10.0f;
        private const float k_MinMaxWeightLabelWidth = 40.0f;
        private const float k_MinMaxWeightFieldWidth = 50.0f;
        private const string k_MinMaxWeightFormat = "g4";
        private const float k_MenuButtonWidth = 16.0f;
        private const float k_MenuButtonHeight = 16.0f;
        private static readonly Color s_FrameBackgroundColor = new(0.0f, 0.4f, 0.8f, 0.2f);
        private static readonly Color s_FrameBorderColor = new(1.0f, 1.0f, 1.0f, 0.2f);
        private static readonly Color s_FrameLineColor = new(1.0f, 1.0f, 1.0f, 0.0f);
        private static readonly Color s_SelectedFrameBackgroundColor = new(0.0f, 0.4f, 0.8f, 0.6f);
        private static readonly Color s_SelectedFrameBorderColor = new(1.0f, 1.0f, 1.0f, 0.6f);
        private static readonly Color s_SelectedFrameLineColor = new(1.0f, 1.0f, 1.0f, 0.6f);
        private static readonly Color s_WeightHandleColor = new(1.0f, 1.0f, 1.0f, 1.0f);
        private static readonly Lazy<RectOffset> s_FramesPadding = new(() => new(2, 2, 2, 2));
        private static readonly Lazy<RectOffset> s_HeaderPadding = new(() => new(15, 1, 0, 0));
        private static readonly Lazy<GUIStyle> s_FramesStyle = new(() => new(EditorStyles.helpBox));
        private static readonly Lazy<GUIStyle> s_WeightHandleStyle = new(() => new("MeBlendPosition"));
        private static readonly Lazy<GUIStyle> s_MinMaxWeightLabelStyle = new(() => new(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });
        private static readonly Lazy<GUIStyle> s_MinMaxWeightFieldStyle = new(() => new(EditorStyles.numberField)
        {
            alignment = TextAnchor.MiddleCenter,
        });
        private static readonly Lazy<GUIStyle> s_MenuButtonStyle = new(() => new("PaneOptions"));

        private readonly BlendShape m_Shape = null;
        private readonly SerializedProperty m_Property = null;
        private readonly SerializedProperty m_NameProperty = null;
        private readonly SerializedProperty m_WeightProperty = null;
        private readonly SerializedProperty m_FramesProperty = null;
        private readonly ReorderableList m_ReorderableList = null;
        private BlendShapeFrameDrawer m_FrameDrawer = null;
        private int m_DragUndoGroup = 0;
        private bool m_IsDragFrame = false;
        private float m_DragWeight = 0.0f;
        private float m_DragMinWeight = 0.0f;
        private float m_DragMaxWeight = 0.0f;

        public BlendShapeDrawer(SerializedProperty property)
        {
            m_Shape = property.managedReferenceValue as BlendShape;
            m_Property = property;
            m_NameProperty = property.FindPropertyRelative(nameof(BlendShape.Name));
            m_WeightProperty = property.FindPropertyRelative(nameof(BlendShape.Weight));
            m_FramesProperty = property.FindPropertyRelative(nameof(BlendShape.Frames));
            m_ReorderableList = new(m_FramesProperty.serializedObject, m_FramesProperty)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                onAddCallback = OnAdd,
                onRemoveCallback = OnRemove,
                onReorderCallback = OnReorder,
                onSelectCallback = _ => GUIUtility.ExitGUI(),
            };
        }

        public bool IsValid(SerializedProperty property)
        {
            return ReferenceEquals(m_Shape, property.managedReferenceValue) && SerializedProperty.EqualContents(m_Property, property);
        }

        public void DrawInspectorGUI(Rect rect)
        {
            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_NameProperty, GUIUtils.TrText("shape-name"), true), true);
            EditorGUI.PropertyField(rect, m_NameProperty, GUIUtils.TrText("shape-name"), true);

            rect = GUIUtils.Line(rect, k_FramesHeight);
            DrawFrames(rect);

            rect = GUIUtils.Line(rect);
            DrawMinMaxWeight(rect);

            rect = GUIUtils.Line(rect);
            rect = GUIUtils.Line(rect, m_ReorderableList.GetHeight());
            m_ReorderableList.DoList(rect);

            var selectedFrameIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedFrameIndex >= 0 && selectedFrameIndex < m_FramesProperty.arraySize)
            {
                rect = GUIUtils.Line(rect);
                rect = GUIUtils.Line(rect, GetFrameDrawer(selectedFrameIndex).CalcInspectorHeight());
                GetFrameDrawer(selectedFrameIndex).DrawInspectorGUI(rect);
            }
        }

        public float CalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_NameProperty, GUIUtils.TrText("shape-name"), true), true);

            rect = GUIUtils.Line(rect, k_FramesHeight);

            rect = GUIUtils.Line(rect);

            rect = GUIUtils.Line(rect);
            rect = GUIUtils.Line(rect, m_ReorderableList.GetHeight());

            var selectedFrameIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedFrameIndex >= 0 && selectedFrameIndex < m_FramesProperty.arraySize)
            {
                rect = GUIUtils.Line(rect);
                rect = GUIUtils.Line(rect, GetFrameDrawer(selectedFrameIndex).CalcInspectorHeight());
            }

            return rect.yMax;
        }

        public void DrawSceneGUI()
        {
            var selectedFrameIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedFrameIndex >= 0 && selectedFrameIndex < m_FramesProperty.arraySize)
            {
                GetFrameDrawer(selectedFrameIndex).DrawSceneGUI();
            }
        }

        public void Dispose()
        {
            m_FrameDrawer?.Dispose();
            m_FrameDrawer = null;
        }

        private void DrawFrames(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, s_FramesStyle.Value);

            rect = s_FramesPadding.Value.Remove(rect);

            var frameWeights = new float[m_FramesProperty.arraySize];
            for (var i = 0; i < m_FramesProperty.arraySize; i++)
            {
                var frameProperty = m_FramesProperty.GetArrayElementAtIndex(i);
                var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                frameWeights[i] = frameWeightProperty.floatValue;
            }

            var minWeight = Mathf.Min(0.0f, Mathf.Min(frameWeights));
            var maxWeight = Mathf.Max(0.0f, Mathf.Max(frameWeights));
            var framePositions = new float[m_FramesProperty.arraySize];
            for (var i = 0; i < m_FramesProperty.arraySize; i++)
            {
                framePositions[i] = Mathf.Lerp(rect.xMin, rect.xMax, Mathf.InverseLerp(minWeight, maxWeight, frameWeights[i]));
            }

            var weightPosition = Mathf.Lerp(rect.xMin, rect.xMax, Mathf.InverseLerp(minWeight, maxWeight, m_WeightProperty.floatValue));
            var weightHandleRect = new Rect(
                weightPosition - s_WeightHandleStyle.Value.fixedWidth * 0.5f, rect.y,
                s_WeightHandleStyle.Value.fixedWidth, s_WeightHandleStyle.Value.fixedHeight);

            var selectedFrameIndex = m_ReorderableList.GetSelectedIndex();

            var control = GUIUtility.GetControlID(FocusType.Passive);
            switch (Event.current.GetTypeForControl(control))
            {
                case EventType.Repaint:
                {
                    var guiColor = GUI.color;
                    var handlesColor = Handles.color;

                    for (var i = 0; i < m_FramesProperty.arraySize; i++)
                    {
                        var prevPosition = i > 0 ? framePositions[i - 1] : rect.xMin;
                        var nextPosition = i < m_FramesProperty.arraySize - 1 ? framePositions[i + 1] : rect.xMax;
                        var points = new Vector3[]
                        {
                            new(prevPosition, rect.yMax),
                            new(framePositions[i], rect.yMin),
                            new(nextPosition, rect.yMax),
                        };

                        Handles.color = i == selectedFrameIndex ? s_SelectedFrameBackgroundColor : s_FrameBackgroundColor;
                        Handles.DrawAAConvexPolygon(points);

                        Handles.color = i == selectedFrameIndex ? s_SelectedFrameBorderColor : s_FrameBorderColor;
                        Handles.DrawAAPolyLine(points);

                        Handles.color = i == selectedFrameIndex ? s_SelectedFrameLineColor : s_FrameLineColor;
                        Handles.DrawLine(new(framePositions[i], rect.yMax), new(framePositions[i], rect.yMin));
                    }

                    if (m_WeightProperty.floatValue >= minWeight && m_WeightProperty.floatValue <= maxWeight)
                    {
                        GUI.color = s_WeightHandleColor;
                        s_WeightHandleStyle.Value.Draw(weightHandleRect, false, false, false, false);
                    }

                    GUI.color = guiColor;
                    Handles.color = handlesColor;
                    break;
                }
                case EventType.MouseDown:
                {
                    if (weightHandleRect.Contains(Event.current.mousePosition)
                        && m_WeightProperty.floatValue >= minWeight
                        && m_WeightProperty.floatValue <= maxWeight)
                    {
                        m_DragUndoGroup = Undo.GetCurrentGroup();
                        m_IsDragFrame = false;
                        m_DragWeight = m_WeightProperty.floatValue;
                        m_DragMinWeight = minWeight;
                        m_DragMaxWeight = maxWeight;

                        Event.current.Use();
                        GUIUtility.hotControl = control;
                        GUIUtility.keyboardControl = control;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    else if (rect.Contains(Event.current.mousePosition))
                    {
                        m_DragUndoGroup = Undo.GetCurrentGroup();
                        m_IsDragFrame = false;
                        m_DragWeight = Mathf.Lerp(minWeight, maxWeight, Mathf.InverseLerp(rect.xMin, rect.xMax, Event.current.mousePosition.x));
                        m_DragMinWeight = minWeight;
                        m_DragMaxWeight = maxWeight;

                        var nearestFrameDeltaPosition = Mathf.Infinity;
                        var nearestFrameIndex = -1;
                        for (var i = 0; i < m_FramesProperty.arraySize; i++)
                        {
                            var deltaPosition = Mathf.Abs(Event.current.mousePosition.x - framePositions[i]);
                            if (deltaPosition < nearestFrameDeltaPosition)
                            {
                                nearestFrameDeltaPosition = deltaPosition;
                                nearestFrameIndex = i;
                            }
                        }

                        if (Event.current.button == 0 && nearestFrameIndex >= 0)
                        {
                            m_IsDragFrame = true;
                            m_DragWeight = frameWeights[nearestFrameIndex];

                            m_ReorderableList.Select(nearestFrameIndex);
                        }
                        else
                        {
                            m_WeightProperty.floatValue = Mathf.Clamp(m_DragWeight, minWeight, maxWeight);
                        }

                        Event.current.Use();
                        GUIUtility.hotControl = control;
                        GUIUtility.keyboardControl = control;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                }
                case EventType.MouseDrag when GUIUtility.hotControl == control:
                {
                    m_DragWeight += (m_DragMaxWeight - m_DragMinWeight) * Event.current.delta.x / rect.width;

                    if (m_IsDragFrame)
                    {
                        var selectedFrameProperty = m_FramesProperty.GetArrayElementAtIndex(selectedFrameIndex);
                        var selectedFrameWeightProperty = selectedFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                        selectedFrameWeightProperty.floatValue = m_DragWeight;

                        var snapWeightRange = (maxWeight - minWeight) * k_SnapRange / rect.width;
                        if (selectedFrameIndex > 0
                            && m_DragWeight > frameWeights[selectedFrameIndex - 1] - snapWeightRange
                            && m_DragWeight < frameWeights[selectedFrameIndex - 1] + snapWeightRange)
                        {
                            selectedFrameWeightProperty.floatValue = frameWeights[selectedFrameIndex - 1];
                        }
                        if (selectedFrameIndex < m_FramesProperty.arraySize - 1
                            && m_DragWeight > frameWeights[selectedFrameIndex + 1] - snapWeightRange
                            && m_DragWeight < frameWeights[selectedFrameIndex + 1] + snapWeightRange)
                        {
                            selectedFrameWeightProperty.floatValue = frameWeights[selectedFrameIndex + 1];
                        }

                        SortFrames();
                        ClampWeight();
                    }
                    else
                    {
                        m_WeightProperty.floatValue = Mathf.Clamp(m_DragWeight, minWeight, maxWeight);

                        var nearestFrameDeltaWeight = Mathf.Infinity;
                        var nearestFrameIndex = -1;
                        for (var i = 0; i < m_FramesProperty.arraySize; i++)
                        {
                            var deltaWeight = Mathf.Abs(m_DragWeight - frameWeights[i]);
                            if (deltaWeight < nearestFrameDeltaWeight)
                            {
                                nearestFrameDeltaWeight = deltaWeight;
                                nearestFrameIndex = i;
                            }
                        }

                        var snapWeightRange = (maxWeight - minWeight) * k_SnapRange / rect.width;
                        if (nearestFrameIndex >= 0
                            && m_DragWeight > frameWeights[nearestFrameIndex] - snapWeightRange
                            && m_DragWeight < frameWeights[nearestFrameIndex] + snapWeightRange)
                        {
                            m_WeightProperty.floatValue = frameWeights[nearestFrameIndex];
                        }
                    }

                    Event.current.Use();
                    GUIUtility.hotControl = control;
                    GUIUtility.keyboardControl = control;
                    EditorGUIUtility.SetWantsMouseJumping(1);
                    break;
                }
                case EventType.MouseUp when GUIUtility.hotControl == control:
                {
                    Undo.CollapseUndoOperations(m_DragUndoGroup);

                    m_DragUndoGroup = 0;
                    m_IsDragFrame = false;
                    m_DragWeight = 0.0f;
                    m_DragMinWeight = 0.0f;
                    m_DragMaxWeight = 0.0f;

                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                }
            }
        }

        private void DrawMinMaxWeight(Rect rect)
        {
            var minWeight = 0.0f;
            var maxWeight = 0.0f;
            if (m_FramesProperty.arraySize > 0)
            {
                var minFrameProperty = m_FramesProperty.GetArrayElementAtIndex(0);
                var minFrameWeightProperty = minFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                minWeight = Mathf.Min(0.0f, minFrameWeightProperty.floatValue);

                var maxFrameProperty = m_FramesProperty.GetArrayElementAtIndex(m_FramesProperty.arraySize - 1);
                var maxFrameWeightProperty = maxFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                maxWeight = Mathf.Max(0.0f, maxFrameWeightProperty.floatValue);
            }

            EditorGUI.BeginChangeCheck();

            var floatFieldFormatString = GUIUtils.FloatFieldFormatString;
            GUIUtils.FloatFieldFormatString = k_MinMaxWeightFormat;

            var newMinWeight = EditorGUI.DelayedFloatField(
                new(rect.xMin + k_MinMaxWeightFieldWidth * 0.0f, rect.y, k_MinMaxWeightFieldWidth, rect.height),
                minWeight, s_MinMaxWeightFieldStyle.Value);
            var newMaxWeight = EditorGUI.DelayedFloatField(
                new(rect.xMax - k_MinMaxWeightFieldWidth * 1.0f, rect.y, k_MinMaxWeightFieldWidth, rect.height),
                maxWeight, s_MinMaxWeightFieldStyle.Value);

            newMinWeight = GUIUtils.DragFloatLabel(
                new(rect.xMin + k_MinMaxWeightFieldWidth + k_MinMaxWeightLabelWidth * 0.0f, rect.y, k_MinMaxWeightLabelWidth, rect.height),
                CL4EE.Tr("min-frame-weight"), newMinWeight, (maxWeight - minWeight) / rect.width, s_MinMaxWeightLabelStyle.Value);
            newMaxWeight = GUIUtils.DragFloatLabel(
                new(rect.xMax - k_MinMaxWeightFieldWidth - k_MinMaxWeightLabelWidth * 1.0f, rect.y, k_MinMaxWeightLabelWidth, rect.height),
                CL4EE.Tr("max-frame-weight"), newMaxWeight, (maxWeight - minWeight) / rect.width, s_MinMaxWeightLabelStyle.Value);

            GUIUtils.FloatFieldFormatString = floatFieldFormatString;

            if (EditorGUI.EndChangeCheck() && m_FramesProperty.arraySize > 0)
            {
                var minFrameProperty = m_FramesProperty.GetArrayElementAtIndex(0);
                var minFrameWeightProperty = minFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                newMinWeight = minFrameWeightProperty.floatValue <= 0.0f ? Mathf.Min(0.0f, newMinWeight) : 0.0f;

                var maxFrameProperty = m_FramesProperty.GetArrayElementAtIndex(m_FramesProperty.arraySize - 1);
                var maxFrameWeightProperty = maxFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                newMaxWeight = maxFrameWeightProperty.floatValue >= 0.0f ? Mathf.Max(0.0f, newMaxWeight) : 0.0f;

                for (var i = 0; i < m_FramesProperty.arraySize; i++)
                {
                    var frameProperty = m_FramesProperty.GetArrayElementAtIndex(i);
                    var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                    frameWeightProperty.floatValue = Mathf.Lerp(newMinWeight, newMaxWeight, Mathf.InverseLerp(minWeight, maxWeight, frameWeightProperty.floatValue));
                }

                if (minWeight == 0.0f && maxWeight == 0.0f)
                {
                    minFrameWeightProperty.floatValue = newMinWeight < 0.0f ? newMinWeight : minFrameWeightProperty.floatValue;
                    maxFrameWeightProperty.floatValue = newMaxWeight > 0.0f ? newMaxWeight : maxFrameWeightProperty.floatValue;
                }

                ClampWeight();
            }
        }

        private void DrawHeader(Rect rect)
        {
            rect = s_HeaderPadding.Value.Remove(rect);
            EditorGUI.LabelField(rect, CL4EE.Tr("frame"), m_FramesProperty.arraySize > 0 ? CL4EE.Tr("frame-weight") : string.Empty);

            rect = new(rect.xMax - k_MenuButtonWidth, rect.center.y - k_MenuButtonHeight * 0.5f, k_MenuButtonWidth, k_MenuButtonHeight);
            if (m_FramesProperty.arraySize > 0 && EditorGUI.DropdownButton(rect, GUIContent.none, FocusType.Passive, s_MenuButtonStyle.Value))
            {
                var menu = new GenericMenu();

                if (m_FramesProperty.arraySize > 1)
                {
                    var minFrameProperty = m_FramesProperty.GetArrayElementAtIndex(0);
                    var minFrameWeightProperty = minFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                    var minWeight = Mathf.Min(0.0f, minFrameWeightProperty.floatValue);
                    var minIndex = minFrameWeightProperty.floatValue >= 0.0f ? -1 : 0;

                    var maxFrameProperty = m_FramesProperty.GetArrayElementAtIndex(m_FramesProperty.arraySize - 1);
                    var maxFrameWeightProperty = maxFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                    var maxWeight = Mathf.Max(0.0f, maxFrameWeightProperty.floatValue);
                    var maxIndex = maxFrameWeightProperty.floatValue <= 0.0f ? m_FramesProperty.arraySize : m_FramesProperty.arraySize - 1;

                    menu.AddItem(new(CL4EE.Tr("distribute-frames")), false, () => DistributeFrames(minWeight, maxWeight, minIndex, maxIndex));
                }
                else
                {
                    menu.AddDisabledItem(new(CL4EE.Tr("distribute-frames")));
                }

                menu.AddItem(new(CL4EE.Tr("normalize-frames")), false, () => DistributeFrames(0.0f, 100.0f, -1, m_FramesProperty.arraySize - 1));

                menu.DropDown(rect);

                void DistributeFrames(float minWeight, float maxWeight, int minIndex, int maxIndex)
                {
                    m_FramesProperty.serializedObject.Update();

                    for (var i = 0; i < m_FramesProperty.arraySize; i++)
                    {
                        var frameProperty = m_FramesProperty.GetArrayElementAtIndex(i);
                        var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                        frameWeightProperty.floatValue = Mathf.Lerp(minWeight, maxWeight, Mathf.InverseLerp(minIndex, maxIndex, i));
                    }

                    m_FramesProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var frameProperty = m_FramesProperty.GetArrayElementAtIndex(index);
            var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));

            EditorGUI.BeginChangeCheck();

            rect.y = rect.center.y - EditorGUIUtility.singleLineHeight * 0.5f;
            rect.height = EditorGUIUtility.singleLineHeight;
            var propertyContent = EditorGUI.BeginProperty(rect, GUIUtils.Text($"#{index}"), frameWeightProperty);

            var propertyRect = EditorGUI.PrefixLabel(rect, propertyContent);
            EditorGUI.DelayedFloatField(propertyRect, frameWeightProperty, GUIContent.none);

            EditorGUI.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                SortFrames();
                ClampWeight();
            }
        }

        private void OnAdd(ReorderableList list)
        {
            var newFrameIndex = m_FramesProperty.arraySize;
            m_FramesProperty.InsertArrayElementAtIndex(newFrameIndex);

            var newFrameProperty = m_FramesProperty.GetArrayElementAtIndex(newFrameIndex);
            newFrameProperty.managedReferenceValue = CreateFrame();

            m_ReorderableList.Select(newFrameIndex);

            SortFrames();
        }

        private void OnRemove(ReorderableList list)
        {
            var selectedFrameIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedFrameIndex >= 0 && selectedFrameIndex < m_FramesProperty.arraySize)
            {
                m_FramesProperty.DeleteArrayElementAtIndex(selectedFrameIndex);
            }
            else
            {
                m_FramesProperty.DeleteArrayElementAtIndex(m_FramesProperty.arraySize - 1);
            }

            if (m_FramesProperty.arraySize == 0)
            {
                m_ReorderableList.ClearSelection();
            }
            else if (selectedFrameIndex > m_FramesProperty.arraySize - 1)
            {
                m_ReorderableList.Select(m_FramesProperty.arraySize - 1);
            }

            ClampWeight();
        }

        private void OnReorder(ReorderableList list)
        {
            var frameWeights = new float[m_FramesProperty.arraySize];
            for (var i = 0; i < m_FramesProperty.arraySize; i++)
            {
                var frameProperty = m_FramesProperty.GetArrayElementAtIndex(i);
                var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                frameWeights[i] = frameWeightProperty.floatValue;
            }

            Array.Sort(frameWeights);

            for (var i = 0; i < m_FramesProperty.arraySize; i++)
            {
                var frameProperty = m_FramesProperty.GetArrayElementAtIndex(i);
                var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                frameWeightProperty.floatValue = frameWeights[i];
            }
        }

        private BlendShapeFrame CreateFrame()
        {
            var frameWeight = 100.0f;
            if (m_FramesProperty.arraySize > 1)
            {
                var prevFrameProperty = m_FramesProperty.GetArrayElementAtIndex(m_FramesProperty.arraySize - 2);
                var prevFrameWeightProperty = prevFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                if (m_FramesProperty.arraySize > 2 && prevFrameWeightProperty.floatValue >= 0.0f)
                {
                    var prevPrevFrameProperty = m_FramesProperty.GetArrayElementAtIndex(m_FramesProperty.arraySize - 3);
                    var prevPrevFrameWeightProperty = prevPrevFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                    frameWeight = (prevFrameWeightProperty.floatValue + prevPrevFrameWeightProperty.floatValue) * 0.5f;
                }
                else
                {
                    frameWeight = prevFrameWeightProperty.floatValue * 0.5f;
                }
            }

            return new()
            {
                Weight = frameWeight,
                Expression = new BlendShapeSampleExpression(),
            };
        }

        private void SortFrames()
        {
            for (var i = 0; i < m_FramesProperty.arraySize; i++)
            {
                var minFrameWeight = Mathf.Infinity;
                var minFrameIndex = -1;
                for (var j = i; j < m_FramesProperty.arraySize; j++)
                {
                    var frameProperty = m_FramesProperty.GetArrayElementAtIndex(j);
                    var frameWeightProperty = frameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                    if (frameWeightProperty.floatValue < minFrameWeight)
                    {
                        minFrameWeight = frameWeightProperty.floatValue;
                        minFrameIndex = j;
                    }
                }

                if (minFrameIndex > i)
                {
                    // Workaround for the issue where calling SerializedProperty.MoveArrayElement() multiple times
                    // on a SerializeReference array or list during a single drag operation breaks the Undo stack
                    Undo.IncrementCurrentGroup();

                    m_FramesProperty.MoveArrayElement(minFrameIndex, i);

                    var selectedFrameIndex = m_ReorderableList.GetSelectedIndex();
                    if (selectedFrameIndex >= i && selectedFrameIndex < minFrameIndex)
                    {
                        m_ReorderableList.Select(selectedFrameIndex + 1);
                    }
                    else if (selectedFrameIndex == minFrameIndex)
                    {
                        m_ReorderableList.Select(i);
                    }
                }
            }
        }

        private void ClampWeight()
        {
            var minWeight = 0.0f;
            var maxWeight = 0.0f;
            if (m_FramesProperty.arraySize > 0)
            {
                var minFrameProperty = m_FramesProperty.GetArrayElementAtIndex(0);
                var minFrameWeightProperty = minFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                minWeight = Mathf.Min(0.0f, minFrameWeightProperty.floatValue);

                var maxFrameProperty = m_FramesProperty.GetArrayElementAtIndex(m_FramesProperty.arraySize - 1);
                var maxFrameWeightProperty = maxFrameProperty.FindPropertyRelative(nameof(BlendShapeFrame.Weight));
                maxWeight = Mathf.Max(0.0f, maxFrameWeightProperty.floatValue);
            }

            m_WeightProperty.floatValue = Mathf.Clamp(m_WeightProperty.floatValue, minWeight, maxWeight);
        }

        private BlendShapeFrameDrawer GetFrameDrawer(int index)
        {
            var frameProperty = m_FramesProperty.GetArrayElementAtIndex(index);
            if (m_FrameDrawer != null && m_FrameDrawer.IsValid(frameProperty))
            {
                return m_FrameDrawer;
            }

            m_FrameDrawer?.Dispose();
            m_FrameDrawer = new(frameProperty);
            return m_FrameDrawer;
        }
    }
}

#endif
