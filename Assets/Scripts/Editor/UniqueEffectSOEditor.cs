using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

namespace DungeonMaster.Equipment.Editor
{
    /// <summary>
    /// TriggeredRule의 리스트를 인스펙터에서 편리하게 편집하기 위한 PropertyDrawer입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(TriggeredRule))]
    public class TriggeredRuleDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> _reorderableLists = new Dictionary<string, ReorderableList>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var triggerProp = property.FindPropertyRelative("Trigger");
            EditorGUILayout.PropertyField(triggerProp);

            // Conditions List
            DrawReorderableList(property, "Conditions", typeof(EffectCondition));

            // Actions List
            DrawReorderableList(property, "Actions", typeof(EffectAction));

            EditorGUI.EndProperty();
        }
        
        private void DrawReorderableList(SerializedProperty parentProperty, string listPropertyName, Type baseType)
        {
            string listKey = parentProperty.propertyPath + listPropertyName;
            
            if (!_reorderableLists.TryGetValue(listKey, out ReorderableList list))
            {
                var listProp = parentProperty.FindPropertyRelative(listPropertyName);
                list = new ReorderableList(parentProperty.serializedObject, listProp, true, true, true, true)
                {
                    drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, $"{listPropertyName} (규칙)"),
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        var element = listProp.GetArrayElementAtIndex(index);
                        rect.y += 2;
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element, true)), element, true);
                    },
                    elementHeightCallback = (index) =>
                    {
                        var element = listProp.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(element, true) + 4;
                    },
                    onAddDropdownCallback = (buttonRect, l) =>
                    {
                        var menu = new GenericMenu();
                        var types = GetInheritedTypes(baseType);
                        foreach (var type in types)
                        {
                            menu.AddItem(new GUIContent(type.Name), false, () => {
                                var newListProp = l.serializedProperty;
                                newListProp.arraySize++;
                                var newElement = newListProp.GetArrayElementAtIndex(newListProp.arraySize - 1);
                                newElement.managedReferenceValue = Activator.CreateInstance(type);
                                newListProp.serializedObject.ApplyModifiedProperties();
                            });
                        }
                        menu.ShowAsContext();
                    }
                };
                _reorderableLists[listKey] = list;
            }

            list.DoLayoutList();
        }

        private IEnumerable<Type> GetInheritedTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract && type != baseType);
        }
        
        // This is needed to make the drawer expand to fit the ReorderableLists
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;
    }
    
    /// <summary>
    /// UniqueEffectSO의 인스펙터를 커스터마이징하여, TriggeredRule 리스트를 더 보기 좋게 만듭니다.
    /// </summary>
    [CustomEditor(typeof(UniqueEffectSO))]
    public class UniqueEffectSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _rulesProperty;
        private ReorderableList _rulesList;

        private void OnEnable()
        {
            _rulesProperty = serializedObject.FindProperty("Rules");
            _rulesList = new ReorderableList(serializedObject, _rulesProperty, true, true, true, true)
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "트리거 규칙 목록"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = _rulesProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, true);
                },
                elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(_rulesProperty.GetArrayElementAtIndex(index), true)
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // "Rules" 속성을 제외한 모든 속성을 자동으로 그립니다. m_Script는 인스펙터에 표시할 필요 없는 스크립트 참조입니다.
            DrawPropertiesExcluding(serializedObject, "m_Script", "Rules");

            EditorGUILayout.Space();
            
            // "Rules" 속성은 ReorderableList를 사용하여 커스텀 UI로 그립니다.
            _rulesList.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
} 