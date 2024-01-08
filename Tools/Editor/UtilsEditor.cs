using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers.Editor
{
    internal static class UtilsEditor
    {
        public class DisablingScope : IDisposable
        {
            private static int s_DisableCounter = 0;

            private bool m_Disable;

            // -----------------------------------------------------------------------
            public void Dispose()
            {
                if (m_Disable && (-- s_DisableCounter <= 0))
                    GUI.enabled = true;
            }

            public DisablingScope(bool disable = true)
            {
                m_Disable = disable;
                if (m_Disable && (++ s_DisableCounter > 0))
                    GUI.enabled = false;
            }
        }
        
        // =======================================================================
        public static Rect Label(this Rect rect)
        {
            return new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        }
        
        public static void DrawObjectReference(this SerializedProperty element, Rect position)
        {
            DrawObjectReference(element.objectReferenceValue, element.isExpanded, position);
        }

        public static void DrawObjectReference(this Object obj, bool isExpanded, Rect position, bool decorativeBox = false, Predicate<SerializedProperty> filter = null)
        {
            if (obj == null)
                return;

            if (isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using var so = new SerializedObject(obj);

                    EditorGUI.BeginChangeCheck();

                    using (var iterator = so.GetIterator())
                    {
                        var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        if (iterator.NextVisible(true))
                        {
                            do
                            {
                                var childProperty = so.FindProperty(iterator.name);
                                if (filter != null && filter.Invoke(childProperty) == false)
                                    continue;

                                if (childProperty.name.Equals("m_Script", StringComparison.Ordinal))
                                    continue;

                                // if (NaughtyAttributes.Editor.PropertyUtility.IsVisible(childProperty) == false)
                                //     continue;

                                var childHeight = EditorGUI.GetPropertyHeight(childProperty);
                                var childRect = new Rect()
                                {
                                    x      = position.x,
                                    y      = position.y + yOffset,
                                    width  = position.width,
                                    height = childHeight
                                };

                                EditorGUI.PropertyField(childRect, iterator, true);
                                
                                yOffset += childHeight;
                            }
                            while (iterator.NextVisible(false));
                        }

                        if (decorativeBox)
			                GUI.Box(position.WithX(0f).IncY(EditorGUIUtility.singleLineHeight).IncWidth(100f).WithHeight(yOffset - EditorGUIUtility.singleLineHeight), GUIContent.none);
                    }

                    if (EditorGUI.EndChangeCheck())
                        so.ApplyModifiedProperties();
                }
            }
        }
        
        public static T GetSerializedValue<T>(this SerializedProperty property)
        {
            object   targetObject  = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');
     
            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();
     
            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                targetObject = targetObject.GetType()
                                           .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                                           .GetValue(targetObject);
            }
     
            if (targetObject != null && targetObject.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());
     
                return ((IList<T>) targetObject)[propertyIndex];
            }
            
            return (T) targetObject;
        }
        
        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            return collection.Count == 0;
        }
        
        public static IEnumerable<Object> GetNestedAssets(this SerializedObject so)
        {
            return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(so.targetObject))
                                .Except(new []{so.targetObject});
        }
        
        public static Rect WithWidth(this Rect rect, float width)
        {
            return new Rect(rect.xMin, rect.yMin, width, rect.height);
        }
        
        public static Rect WithHeight(this Rect rect, float height)
        {
            return new Rect(rect.xMin, rect.yMin, rect.width, height);
        }
        
        public static Rect WithX(this Rect rect, float xMin)
        {
            return new Rect(xMin, rect.yMin, rect.width, rect.height);
        }
        
        public static Rect WithY(this Rect rect, float yMin)
        {
            return new Rect(rect.xMin, yMin, rect.width, rect.height);
        }
        
        public static Rect IncX(this Rect rect, float addX )
        {
            return new Rect(rect.xMin + addX, rect.yMin, rect.width, rect.height);
        }
        
        public static Rect IncY(this Rect rect, float addY)
        {
            return new Rect(rect.xMin, rect.yMin + addY, rect.width, rect.height);
        }
        
        public static Rect IncWidth(this Rect rect, float addWidth)
        {
            return new Rect(rect.x, rect.y, rect.width + addWidth, rect.height);
        }
        
        public static Rect IncWidth(this Rect rect, float addWidth, Vector2 pivot)
        {
            return new Rect(rect.x + pivot.x * addWidth, rect.y, rect.width + addWidth, rect.height);
        }
        
        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return Attribute.GetCustomAttribute(type, typeof(T)) != null;
        }

        public static IEnumerable<SerializedProperty> GetList(this SerializedProperty property)
        {
            for (var n = 0; n < property.arraySize; n++)
            {
                yield return property.GetArrayElementAtIndex(n);
            }
        }
        
        public static void SetList<T>(this SerializedProperty property, IEnumerable<T> list)
        {
            var index = 0;
            foreach (var n in list)
            {
                var item = property.GetArrayElementAtIndex(index);
                switch (n)
                {
                    case int i:
                        item.intValue = i;
                        break;

                    case Object obj:
                        item.objectReferenceValue = obj;
                        break;

                    case string str:
                        item.stringValue = str;
                        break;
                }

                index ++;
            }
        }
        public static float GetObjectReferenceHeight(this SerializedProperty element)
        {
            return GetObjectReferenceHeight(element.objectReferenceValue, element.isExpanded);
        }

        public static float GetObjectReferenceHeight(this Object obj, bool isExpanded, Predicate<SerializedProperty> filter = null)
        {
            if (obj == null)
                return EditorGUIUtility.singleLineHeight;

            if (isExpanded)
            {
                using var so          = new SerializedObject(obj);
                var       totalHeight = EditorGUIUtility.singleLineHeight;

                using (var iterator = so.GetIterator())
                {
                    if (iterator.NextVisible(true))
                    {
                        do
                        {
                            var childProperty = so.FindProperty(iterator.name);
                            
                            if (childProperty.name.Equals("m_Script", System.StringComparison.Ordinal))
                                continue;
                            
                            if (filter != null && filter.Invoke(childProperty) == false)
                                continue;
                            
							// if (NaughtyAttributes.Editor.PropertyUtility.IsVisible(childProperty) == false)
                            //    continue;

                            totalHeight += EditorGUI.GetPropertyHeight(childProperty);
                        }
                        while (iterator.NextVisible(false));
                    }
                }

                totalHeight += EditorGUIUtility.standardVerticalSpacing;
                return totalHeight;
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}