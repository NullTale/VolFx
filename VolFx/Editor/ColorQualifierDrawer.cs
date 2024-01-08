using UnityEditor;
using UnityEngine;
using VolFx;

namespace Buffers.Editor
{
    [CustomPropertyDrawer(typeof(ColorQualifier))]
    public class ColorQualifierDrawer : PropertyDrawer
    {
        public const int k_TexSize = 32;
        
        private static Texture2D _hue; 
        private static Texture2D _sat; 
        private static Texture2D _val;
        
        // =======================================================================
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var line = 0;
            var hue = property.FindPropertyRelative(nameof(ColorQualifier._hue));
            var sat = property.FindPropertyRelative(nameof(ColorQualifier._sat));
            var val = property.FindPropertyRelative(nameof(ColorQualifier._val));
            
            _slider(_fieldRect(line++), hue, _hue);
            _slider(_fieldRect(line++), sat, _sat);
            _slider(_fieldRect(line++), val, _val);

            _validateTex();
            
           // -----------------------------------------------------------------------
           void _slider(Rect pos, SerializedProperty prop, Texture2D bg)
           {
               var vec = prop.vector2Value;
               EditorGUI.DrawPreviewTexture(_fieldOnly(pos, EditorGUIUtility.standardVerticalSpacing), bg, null, ScaleMode.StretchToFill);
               
               //if (GUI.GetNameOfFocusedControl() != $"{property.name}_{prop.name}");
               //GUI.color = new Color(1, 1, 1, .7f);
                
               //GUI.SetNextControlName($"{property.name}_{prop.name}");
               //pos.y += pos.width * 0.3f;
               //pos.width *= 0.7f;
               EditorGUI.MinMaxSlider(pos, prop.displayName, ref vec.x, ref vec.y, 0f, 1f);
               GUI.color = Color.white;
               prop.vector2Value = vec;
           }
           
           Rect _fieldRect(int line)
           {
               return new Rect(position.x, position.y + line * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
           }
           
           Rect _fieldOnly(Rect rect, float yPadding)
           {
               var offset = EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
               return new Rect(rect.x + offset, rect.y + yPadding, rect.width - offset, EditorGUIUtility.singleLineHeight - yPadding * 2f);
           }
        }
        
        // =======================================================================
        private static void _validateTex()
        {
            //if (_hue != null && _sat != null && _val != null) return;
            
            _hue = new Texture2D(k_TexSize, 1, TextureFormat.RGBA32, false, true);
            _sat = new Texture2D(k_TexSize, 1, TextureFormat.RGBA32, false, true);
            _val = new Texture2D(k_TexSize, 1, TextureFormat.RGBA32, false, true);
            
            for (var n = 0; n < k_TexSize; n++)
            {
                _hue.SetPixel(n, 0, Color.HSVToRGB(n / (float)(k_TexSize - 1), 1f, 1f));
                _sat.SetPixel(n, 0, Color.HSVToRGB(0, n / (float)(k_TexSize - 1), 1f));
                _val.SetPixel(n, 0, Color.HSVToRGB(0, 0, n / (float)(k_TexSize - 1)));
            }
            
            _hue.Apply();
            _sat.Apply();
            _val.Apply();;
        }
    }
}