using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalDrawer : PropertyDrawer
    {
        private const float k_ToggleWidth = 18;

        // =======================================================================
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");
            return EditorGUI.GetPropertyHeight(valueProperty);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty   = property.FindPropertyRelative("value");
            var enabledProperty = property.FindPropertyRelative("enabled");

            position.width -= k_ToggleWidth;
            using (new EditorGUI.DisabledGroupScope(!enabledProperty.boolValue))
            {
                // hardcore fix!#@ (for some reason PropertyField working with LayerMask in new unity version)
                if (valueProperty.propertyType == SerializedPropertyType.LayerMask)
                    valueProperty.intValue =  InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(EditorGUI.MaskField(position, label, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(valueProperty.intValue), InternalEditorUtility.layers));
                else
                    EditorGUI.PropertyField(position, valueProperty, label, true);
            }

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var togglePos = new Rect(position.x + position.width + EditorGUIUtility.standardVerticalSpacing, position.y, k_ToggleWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(togglePos, enabledProperty, GUIContent.none);

            EditorGUI.indentLevel = indent;
        }
    }
}