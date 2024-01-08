using UnityEditor;
using UnityEngine;
using VolFx;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers.Editor
{
    [CustomPropertyDrawer(typeof(RangeFloat))]
    public class RangeFloatDrawer : PropertyDrawer
    {
        // =======================================================================
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var range = property.FindPropertyRelative(nameof(RangeFloat.Range));
            var value = property.FindPropertyRelative(nameof(RangeFloat.Value));
            
            value.floatValue = EditorGUI.Slider(position, label, value.floatValue, range.vector2Value.x, range.vector2Value.y);
        }
    }
}