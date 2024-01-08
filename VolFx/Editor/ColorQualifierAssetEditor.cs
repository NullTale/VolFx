using System.IO;
using UnityEditor;
using UnityEngine;

namespace VolFx.Editor
{
    [CustomEditor(typeof(ColorQualifierAsset))]
    public class ColorQualifierAssetEditor : UnityEditor.Editor
    {
        private Texture2D _lutNeutral;
        private Texture2D _lutBlended;
        
        // =======================================================================
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Save as File"))
            {
                var cqa  = (ColorQualifierAsset)target;
                var path = $"{Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(cqa))}\\{cqa.name}.png";
                File.WriteAllBytes(path, cqa.Lut.EncodeToPNG());

                UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
            }
        }

        public override bool HasPreviewGUI() => true;

        public override void DrawPreview(Rect previewArea)
        {
            var blend = lutBlended();
            //EditorGUI.DrawPreviewTexture(previewArea, ((ColorQualifierAsset)target).Lut, null, ScaleMode.ScaleToFit);
            //GUI.color = new Color(1, 1, 1, 0.33f);
            EditorGUI.DrawPreviewTexture(previewArea, blend, null, ScaleMode.ScaleToFit);
            //GUI.color = Color.white;
        }
        
        private Texture2D lutNeutral()
        {
            if (_lutNeutral != null)
                return _lutNeutral;
            
            var lutSize = ColorQualifierAsset.s_LutSize;
            
            _lutNeutral = new Texture2D(lutSize * lutSize, lutSize, TextureFormat.RGBA32, false, true);
            
            for (var x = 0; x < lutSize * lutSize; x++)
            for (var y = 0; y < lutSize; y++)
                _lutNeutral.SetPixel(x, y, _lutAt(x, y));
            
            _lutNeutral.Apply();
            return _lutNeutral;
        }
        
        private Texture2D lutBlended()
        {
            var lutSize = ColorQualifierAsset.s_LutSize;
            var lut = ((ColorQualifierAsset)target).Lut;
            
            if (_lutBlended == null)
                _lutBlended = new Texture2D(lutSize * lutSize, lutSize, TextureFormat.RGBA32, false, true);
            
            for (var x = 0; x < lutSize * lutSize; x++)
            for (var y = 0; y < lutSize; y++)
            {
                var lutBase = _lutAt(x, y);
                var gray    = lutBase.grayscale;
                var lutGray = new Color(gray, gray, gray, 1f);
                _lutBlended.SetPixel(x, y, Color.Lerp(lutGray, lutBase, lut.GetPixel(x, y).r));
            }
            
            _lutBlended.Apply();
            return _lutBlended;
        }
        
        private Color _lutAt(int x, int y)
        {
            var lutSize = ColorQualifierAsset.s_LutSize;
            return new Color((x % lutSize) / (lutSize - 1f), y / (lutSize - 1f), Mathf.FloorToInt(x / (float)lutSize) * (1f / (lutSize - 1f)), .5f);
        }
    }
}