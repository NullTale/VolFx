using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(GradientValue))]
    public class GradientValueDraver : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var grad = property.FindPropertyRelative("_grad");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, grad, label);
            if (EditorGUI.EndChangeCheck())
            {
                var pixels = property.FindPropertyRelative("_pixels");
                var val = _getGradient(grad);
                for (var n = 0; n < GradientValue.k_Width; n++)
                    pixels.GetArrayElementAtIndex(n).colorValue = val.Evaluate(n / (float)(GradientValue.k_Width - 1));
            }

            // =======================================================================
            Gradient _getGradient(SerializedProperty gradientProperty)
            {
#if UNITY_2022_1_OR_NEWER
                return grad.gradientValue;
#else
                System.Reflection.PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue",
                                                                                                     System.Reflection.BindingFlags.Public |
                                                                                                     System.Reflection.BindingFlags.NonPublic |
                                                                                                     System.Reflection.BindingFlags.Instance);
                
                return propertyInfo.GetValue(gradientProperty, null) as Gradient;
#endif
            }
        }
    }
}