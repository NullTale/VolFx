using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginProperty(position, label, property);

                var layerAttribute = attribute as LayerAttribute;
                if (layerAttribute != null)
                {
                    property.intValue = EditorGUI.LayerField(position, label, property.intValue);
                }
                else
                {
                    EditorGUI.LabelField(position, label, "Use LayerAttribute with LayerDrawer");
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label, "LayerDrawer supports only integer properties");
            }
        }
    }
}