using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(CurveValue))]
    public class CurveValueDraver : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var curve = property.FindPropertyRelative("_curve");
            EditorGUI.BeginChangeCheck();
            EditorGUI.CurveField(position, curve, Color.green, new Rect(0, 0, 1, 1), label);
            if (EditorGUI.EndChangeCheck())
            {
                var pixels = property.FindPropertyRelative("_pixels");
                var val    = curve.animationCurveValue;
                for (var n = 0; n < GradientValue.k_Width; n++)
                {
                    var c = val.Evaluate(n / (float)(GradientValue.k_Width - 1));
                    pixels.GetArrayElementAtIndex(n).colorValue = new Color(c, c, c, c);
                }
            }
        }
    }
}