using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers.Editor
{
    [CustomPropertyDrawer(typeof(CurveRangeAttribute))]
    public class CurveRangePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var curveRangeAttribute = (CurveRangeAttribute)attribute;
            var curveRanges = new Rect(
                curveRangeAttribute.Min.x,
                curveRangeAttribute.Min.y,
                curveRangeAttribute.Max.x - curveRangeAttribute.Min.x,
                curveRangeAttribute.Max.y - curveRangeAttribute.Min.y);

            EditorGUI.CurveField(
                rect,
                property,
                Color.green,
                curveRanges,
                label);

            EditorGUI.EndProperty();
        }
    }
}