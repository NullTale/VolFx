using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [CustomPropertyDrawer(typeof(SoCollection<>), true)]
    public class SoCollectionDrawer : PropertyDrawer
    {
        private ReorderableList m_List;

        // =======================================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var list = _getList(property);
           
                var headerRect = position;
                headerRect.height = EditorGUIUtility.singleLineHeight;
                property.isExpanded =  EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none, true);
                
                if (property.isExpanded == false)
                {
                    ReorderableList.defaultBehaviours.DrawHeaderBackground(headerRect);
                    headerRect.xMin     += 6f;
                    headerRect.xMax     -= 6f;
                    headerRect.height   -= 2f;
                    headerRect.y        += 1f;
                    list.drawHeaderCallback.Invoke(headerRect);
                }
                else
                {
                    list.DoList(position);
                    list.GetHeight();
                }
                
            }
            catch
            {
                // pass
            }
        }              
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                if (property.isExpanded == false)
                    return EditorGUIUtility.singleLineHeight;
                
                return _getList(property).GetHeight();
            }
            catch
            {
                return EditorGUI.GetPropertyHeight(property);
            }
        }
        
        // =======================================================================
        private ReorderableList _getList(SerializedProperty property)
        {
            if (m_List == null)
            {
                var propertyList   = property.FindPropertyRelative("m_List");
                var collectionType = propertyList.GetSerializedValue<object>().GetType().GetGenericArguments().First();
                var formatAtr      = fieldInfo.GetCustomAttribute<SocFormatAttribute>(true);

                // setup types, create list
                var types = new List<Type>();
                if (attribute is SocTypesAttribute attr)
                    types.AddRange(attr.Types);

                if (types.IsEmpty())
                    types.Add(collectionType);

                types.RemoveAll(n => n.IsAbstract);

                m_List = new ReorderableList(propertyList.serializedObject, propertyList, true, true, true, true);
                m_List.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.width -= 30;
                    rect.x += 30 * 0.5f;

                    var element = propertyList.GetArrayElementAtIndex(index);

                    var hasValue = element.objectReferenceValue != null;
                    var isInner = element.objectReferenceValue != null && element.serializedObject.GetNestedAssets().Contains(element.objectReferenceValue);

                    // ref
                    using (new UtilsEditor.DisablingScope(isInner))
                    {
                        EditorGUI.BeginChangeCheck();
                        var picked = EditorGUI.ObjectField(rect.WithHeight(EditorGUIUtility.singleLineHeight), new GUIContent(" "), element.objectReferenceValue, types.FirstOrDefault() ?? collectionType, false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (picked == null || _containtsName(picked.name) == false)
                                element.objectReferenceValue = picked;
                            else
                                Debug.Log($"Can't add object {picked} with duplicated name {picked.name}");
                        }
                    }


                    if (hasValue)
                        element.isExpanded = EditorGUI.Foldout(rect.WithHeight(EditorGUIUtility.singleLineHeight).WithWidth(5), element.isExpanded, GUIContent.none, toggleOnLabelClick: true);

                    // name
                    if (isInner)
                    {
                        var elementSO = element.objectReferenceValue as ScriptableObject;
                        if (elementSO != null)
                        {
                            // name must be unique
                            var newName = EditorGUI.DelayedTextField(rect.Label(), elementSO.name);
                            if (newName != elementSO.name && _containtsName(newName) == false)
                            {
                                elementSO.name = newName;
                                _delayedUpdate();

                                // -----------------------------------------------------------------------
                                async void _delayedUpdate()
                                {
                                    await Task.Yield();
                                    AssetDatabase.SaveAssets();
                                }
                            }

                            element.DrawObjectReference(rect);
                        }
                    }
                    else
                    {
                        using (new UtilsEditor.DisablingScope())
                            EditorGUI.TextField(rect.Label(), element.objectReferenceValue?.name ?? string.Empty);

                        element.DrawObjectReference(rect);
                    }

                    // ===================================
                    bool _containtsName(string name)
                    {
                        return propertyList.GetList().Where(n => n.objectReferenceValue != null).Any(n => n.objectReferenceValue.name == name);
                    }
                };
                m_List.onRemoveCallback = list =>
                {
                    var element = propertyList.GetArrayElementAtIndex(list.index);

                    var isNested = element.objectReferenceValue != null && element.serializedObject.GetNestedAssets().Contains(element.objectReferenceValue);
                    var hasValue = element.objectReferenceValue != null;
                    var obj      = element.objectReferenceValue;

                    var doNotRemove = Event.current.modifiers == EventModifiers.Shift || isNested == false || hasValue == false 
                                      || propertyList.GetList().Select(n => n.objectReferenceValue).Count(n => n == element.objectReferenceValue) > 1;

                    if (doNotRemove == false)
                    {
                        AssetDatabase.RemoveObjectFromAsset(obj);
                        Object.DestroyImmediate(element.objectReferenceValue);
                        element.objectReferenceValue = null;
#if !UNITY_2022_3_OR_NEWER
                        AssetDatabase.SaveAssets();
#endif
                    }

                    var index = list.index;
                    propertyList.DeleteArrayElementAtIndex(index);
                };
                m_List.onAddCallback = list =>
                {
                    // collect options
                    var options   = new List<KeyValuePair<string, Action>>();
                    var shiftHeld = Event.current.modifiers == EventModifiers.Shift;

                    // empty object option
                    if (shiftHeld)
                        options.Add(new KeyValuePair<string, Action>("Add Empty", () =>
                        {
                            propertyList.arraySize++;
                            propertyList.GetArrayElementAtIndex(propertyList.arraySize - 1).objectReferenceValue = null;
                            propertyList.serializedObject.ApplyModifiedProperties();
                        }));

                    // options from types
                    options.AddRange(types.Select(type => new KeyValuePair<string, Action>(type.Name, () => _createElementOfType(type))));

                    // if shift held add from existing and derived
                    if (shiftHeld)
                    {

                        var ofType = types.IsEmpty() ? TypeCache.GetTypesDerivedFrom(collectionType)
                                     .Where(type => type.IsAbstract == false && type.IsGenericTypeDefinition == false)
                                     .Except(new[] { collectionType })
                                     .ToArray() : types.ToArray();

                        var subAssets = _getAssets(ofType)
                                         .Except(_getElements(ofType))
                                         .Select(so => new KeyValuePair<string, Action>((string.IsNullOrEmpty(so.name) ? "_" : so.name), () => _addExistingElement(so)))
                                         .OrderBy(n => n.Key)
                                         .ToList();

                        if (subAssets.Count > 0)
                        {
                            if (options.IsEmpty() == false)
                                options.Add(new KeyValuePair<string, Action>(string.Empty, null));
                            options.AddRange(subAssets);
                        }
                        
                    }
                    if (shiftHeld || types.IsEmpty())
                    {
                        var derived = TypeCache.GetTypesDerivedFrom(collectionType)
                                     .Where(type => type.IsAbstract == false && type.IsGenericTypeDefinition == false && type.HasAttribute<SocIgnoreAttribute>() == false)
                                     .Except(new[] { collectionType })
                                     .Select(type => new KeyValuePair<string, Action>(type.Name, () => _createElementOfType(type)))
                                     .ToList();

                        if (derived.Count > 0)
                        {
                            if (options.IsEmpty() == false)
                                options.Add(new KeyValuePair<string, Action>(string.Empty, null));
                            options.AddRange(derived);
                        }
                    }
                    
                    // apply unique filter to the final result
                    var uniqueAtr = fieldInfo.GetCustomAttribute<SocUniqueAttribute>(true);
                    if (uniqueAtr != null)
                    {
                        for (var n = 0; n < list.count; n++)
                        {
                            var item = propertyList.GetArrayElementAtIndex(n).objectReferenceValue;
                            if (item == null)
                                continue;
                            
                            if (uniqueAtr._except.Contains(item.GetType()))
                                continue;
                            
                            var typeName = item.GetType().Name;
                            var toRemove = options.FirstOrDefault(n => n.Key == typeName);
                            options.Remove(toRemove);
                        }
                    }
                    
                    // check real options count
                    if (options.Count == 1)
                    {
                        options.First().Value.Invoke();
                    }
                    else
                    if (options.Count != 0)
                    {
                        var menu = new GenericMenu();
                        foreach (var option in options.OrderBy(n => n.Key))
                        {
                            if (string.IsNullOrEmpty(option.Key))
                                menu.AddSeparator(string.Empty);
                            else
                                menu.AddItem(new GUIContent(_format(option.Key)), false, option.Value.Invoke);
                        }

                        menu.ShowAsContext();
                    }

                    // ===================================
                    void _addExistingElement(object element)
                    {
                        propertyList.arraySize ++;
                        propertyList.GetArrayElementAtIndex(propertyList.arraySize - 1).objectReferenceValue = (Object)element;
                        propertyList.serializedObject.ApplyModifiedProperties();
                    }

                    IEnumerable<ScriptableObject> _getElements(Type[] ofType)
                    {
                        var result = new List<ScriptableObject>(propertyList.arraySize);

                        for (var n = 0; n < propertyList.arraySize; n++)
                        {
                            var el = propertyList.GetArrayElementAtIndex(n);
                            if (ofType.Contains(el.objectReferenceValue.GetType()) == false)
                                continue;

                            result.Add(el.objectReferenceValue as ScriptableObject);
                        }

                        return result;
                    }

                    IEnumerable<ScriptableObject> _getAssets(Type[] ofType)
                    {
                        return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(propertyList.serializedObject.targetObject))
                                            .Except(new []{propertyList.serializedObject.targetObject})
                                            .OfType<ScriptableObject>()
                                            .Where(n => ofType.Contains(n.GetType()))
                                            .ToList();
                    }
                };
                m_List.drawHeaderCallback = rect =>
                {
                    var style = EditorStyles.label;
                    if (Event.current.modifiers == EventModifiers.Shift)
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = Color.yellow;
                    }
                    EditorGUI.LabelField(rect, new GUIContent(property.displayName, "Hold shift to see all options"), style);
                    
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        property.isExpanded = !property.isExpanded;
                    }
                    
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Sort Alphabetic"), false, () =>
                        {
                            var objects = propertyList.GetList().Select(n => n.objectReferenceValue).OrderBy(n => n.name).ToList();
                            propertyList.SetList(objects);
                            propertyList.serializedObject.ApplyModifiedProperties();
                        });
                        
                        menu.AddItem(new GUIContent("Add All Unique"), false, () =>
                        {
                            var set = _collectionTypes();
                            var toCreate = types.IsEmpty() ? TypeCache.GetTypesDerivedFrom(collectionType)
                                     .Where(type => type.IsAbstract == false && type.IsGenericTypeDefinition == false && set.Contains(type) == false)
                                     .Except(new[] { collectionType })
                                     .ToList() : types.ToList();
                            
                            var uniqueAtr = fieldInfo.GetCustomAttribute<SocUniqueAttribute>(true);
                            if (uniqueAtr != null)
                            {
                                foreach (var type in uniqueAtr._except)
                                    toCreate.Remove(type);
                            }
                            
                            foreach (var type in toCreate)
                                _createElementOfType(type);

                            // -----------------------------------------------------------------------
                            HashSet<Type> _collectionTypes()
                            {
                                var result = new HashSet<Type>(propertyList.arraySize);

                                for (var n = 0; n < propertyList.arraySize; n++)
                                {
                                    var el = propertyList.GetArrayElementAtIndex(n);
                                    if (el.objectReferenceValue == null)
                                        continue;
                                    result.Add(el.objectReferenceValue?.GetType());
                                }

                                return result;
                            }
                        });
                        
                        menu.ShowAsContext();
                    }
                };
                m_List.elementHeightCallback = index =>
                {
                    if (propertyList.arraySize == 0)
                        return 0;

                    return propertyList.GetArrayElementAtIndex(index).GetObjectReferenceHeight();
                };
                
                // =======================================================================
                void _createElementOfType(object type)
                {
                    propertyList.arraySize ++;
                    
                    var element = ScriptableObject.CreateInstance((Type)type);
                    var varName = _format(element.GetType().Name);

                    // create
                    element.name = varName;
                    AssetDatabase.AddObjectToAsset(element, propertyList.serializedObject.targetObject);
#if !UNITY_2022_3_OR_NEWER
                    AssetDatabase.SaveAssets();
#endif
                    propertyList.GetArrayElementAtIndex(propertyList.arraySize - 1).objectReferenceValue = element;
                    element.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

                    propertyList.serializedObject.ApplyModifiedProperties();
                }
                
                string _format(string varName)
                {
                    if (formatAtr != null)
                    {
                        var regex = new Regex(formatAtr._regexClear);
                        varName = regex.Replace(varName, "");
                        varName = string.Format(varName, formatAtr._format);
                        if (formatAtr._nicify)
                            varName = ObjectNames.NicifyVariableName(varName);
                    }
                    else
                    {
                        varName = ObjectNames.NicifyVariableName(varName);
                    }
                    
                    return varName;
                }
            }

            return m_List;
        }
        
    }
}