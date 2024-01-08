using System;
using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(VolFx.SourceOptions))]
    public class SourceOptionsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var mode = (VolFx.SourceOptions.Source)property.FindPropertyRelative(nameof(VolFx._source._source)).intValue;
            return mode switch
            {
                VolFx.SourceOptions.Source.Camera    => 1,
                VolFx.SourceOptions.Source.GlobalTex => 3,
                VolFx.SourceOptions.Source.RenderTex => 3,
                VolFx.SourceOptions.Source.LayerMask => 2 + ((VolFx.SourceOptions.MaskOutput)property.FindPropertyRelative(nameof(VolFx._source._output)).intValue == VolFx.SourceOptions.MaskOutput.Texture ? 2 : 0),
                VolFx.SourceOptions.Source.Buffer    => 3,
                _                                    => throw new ArgumentOutOfRangeException()
            } * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mode  = property.FindPropertyRelative(nameof(VolFx._source._source));
            var tex   = property.FindPropertyRelative(nameof(VolFx._source._sourceTex));
            var rt    = property.FindPropertyRelative(nameof(VolFx._source._renderTex));
            var buf   = property.FindPropertyRelative(nameof(VolFx._source._buffer));
            var cam   = property.FindPropertyRelative(nameof(VolFx._source._screenOutput));
            
            var output    = property.FindPropertyRelative(nameof(VolFx._source._output));
            var outputTex = property.FindPropertyRelative(nameof(VolFx._source._outputTex));
            
            var line = 0;
            EditorGUI.PropertyField(_fieldRect(line ++), mode, label, true);
            EditorGUI.indentLevel ++;
            
            switch ((VolFx.SourceOptions.Source)mode.intValue)
            {
                case VolFx.SourceOptions.Source.Camera:
                    break;
                case VolFx.SourceOptions.Source.GlobalTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), tex, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                case VolFx.SourceOptions.Source.RenderTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), rt, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                case VolFx.SourceOptions.Source.LayerMask:
                    EditorGUI.PropertyField(_fieldRect(line ++), output, true);
                    if (((VolFx.SourceOptions.MaskOutput)output.intValue) == VolFx.SourceOptions.MaskOutput.Texture)
                        EditorGUI.PropertyField(_fieldRect(line ++), outputTex, true);
                    if (((VolFx.SourceOptions.MaskOutput)output.intValue) != VolFx.SourceOptions.MaskOutput.Camera)
                        EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                case VolFx.SourceOptions.Source.Buffer:
                    EditorGUI.PropertyField(_fieldRect(line ++), buf, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.indentLevel --;
            
            // -----------------------------------------------------------------------
            Rect _fieldRect(int line)
            {
                return new Rect(position.x, position.y + line * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            }
        }
    }
}