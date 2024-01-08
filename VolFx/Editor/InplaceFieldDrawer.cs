using System.Linq;
using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(InplaceFieldAttribute))]
    public class InplaceFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = position;
            pos.height = 0f;

            var attr = ((InplaceFieldAttribute)attribute);
            foreach (var propPath in attr.PropertyPath)
            {
                var prop = property.FindPropertyRelative(propPath);
                pos.y      += pos.height;
                pos.height =  EditorGUI.GetPropertyHeight(prop, true);
                EditorGUI.PropertyField(pos, prop, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ((InplaceFieldAttribute)attribute).PropertyPath.Sum(n => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(n), true));
        }
    }
}