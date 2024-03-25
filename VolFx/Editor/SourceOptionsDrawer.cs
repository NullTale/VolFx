using System;
using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(VolFxProc.SourceOptions))]
    public class SourceOptionsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var mode = (VolFxProc.SourceOptions.Source)property.FindPropertyRelative(nameof(VolFxProc._source._source)).intValue;
            return mode switch
            {
                VolFxProc.SourceOptions.Source.Camera    => 1,
                VolFxProc.SourceOptions.Source.GlobalTex => 3,
                VolFxProc.SourceOptions.Source.RenderTex => 3,
                VolFxProc.SourceOptions.Source.LayerMask => 3 + ((VolFxProc.SourceOptions.MaskOutput)property.FindPropertyRelative(nameof(VolFxProc._source._output)).intValue == VolFxProc.SourceOptions.MaskOutput.Texture ? 2 : 0),
                VolFxProc.SourceOptions.Source.Buffer    => 3,
                _                                    => throw new ArgumentOutOfRangeException()
            } * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mode  = property.FindPropertyRelative(nameof(VolFxProc._source._source));
            var tex   = property.FindPropertyRelative(nameof(VolFxProc._source._sourceTex));
            var rt    = property.FindPropertyRelative(nameof(VolFxProc._source._renderTex));
            var buf   = property.FindPropertyRelative(nameof(VolFxProc._source._buffer));
            var cam   = property.FindPropertyRelative(nameof(VolFxProc._source._screenOutput));
            var mask  = property.FindPropertyRelative(nameof(VolFxProc._source._render));
            
            var output    = property.FindPropertyRelative(nameof(VolFxProc._source._output));
            var outputTex = property.FindPropertyRelative(nameof(VolFxProc._source._outputTex));
            
            var line = 0;
            EditorGUI.PropertyField(_fieldRect(line ++), mode, label, true);
            EditorGUI.indentLevel ++;
            
            switch ((VolFxProc.SourceOptions.Source)mode.intValue)
            {
                case VolFxProc.SourceOptions.Source.Camera:
                    break;
                case VolFxProc.SourceOptions.Source.GlobalTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), tex, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                case VolFxProc.SourceOptions.Source.RenderTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), rt, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                case VolFxProc.SourceOptions.Source.LayerMask:
                    EditorGUI.PropertyField(_fieldRect(line ++), mask, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), output, true);
                    if (((VolFxProc.SourceOptions.MaskOutput)output.intValue) == VolFxProc.SourceOptions.MaskOutput.Texture)
                        EditorGUI.PropertyField(_fieldRect(line ++), outputTex, true);
                    if (((VolFxProc.SourceOptions.MaskOutput)output.intValue) != VolFxProc.SourceOptions.MaskOutput.Camera)
                        EditorGUI.PropertyField(_fieldRect(line ++), cam, true);
                    break;
                case VolFxProc.SourceOptions.Source.Buffer:
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