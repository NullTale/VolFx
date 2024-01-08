using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers
{
    public static class Utils
    {
        public static int s_MainTexId = Shader.PropertyToID("_MainTex");
        
        private static Mesh s_FullscreenQuad;
        private static Mesh s_FullscreenTriangle;
        public static  Mesh FullscreenMesh
        {
            get
            {
                _initFullScreenMeshes();
                return s_FullscreenTriangle;
            }
        }
        
        private static Matrix4x4 s_IndentityInvert = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, -1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));

        // =======================================================================
        private static void _initFullScreenMeshes()
        {
            // quad
            if (s_FullscreenQuad == null)
            {
                s_FullscreenQuad = new Mesh { name = "Fullscreen Quad" };
                s_FullscreenQuad.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f, 1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f)
                });

                s_FullscreenQuad.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, 1f),
                    new Vector2(0.0f, 0f),
                    new Vector2(1.0f, 1f),
                    new Vector2(1.0f, 0f)
                });

                s_FullscreenQuad.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
                s_FullscreenQuad.UploadMeshData(true);
            }
            
            // triangle
            if (s_FullscreenTriangle == null)
            { 
                s_FullscreenTriangle           = new Mesh() { name = "Fullscreen Triangle" };
                s_FullscreenTriangle.vertices  = _verts(0f);
                s_FullscreenTriangle.uv        = _texCoords();
                s_FullscreenTriangle.triangles = new int[3] { 0, 1, 2 };

                s_FullscreenTriangle.UploadMeshData(true);

                // -----------------------------------------------------------------------
                Vector3[] _verts(float z)
                {
                    var r = new Vector3[3];
                    for (var i = 0; i < 3; i++)
                    {
                        var uv = new Vector2((i << 1) & 2, i & 2);
                        r[i] = new Vector3(uv.x * 2f - 1f, uv.y * 2f - 1f, z);
                    }

                    return r;
                }

                Vector2[] _texCoords()
                {
                    var r = new Vector2[3];
                    for (var i = 0; i < 3; i++)
                    {
                        if (SystemInfo.graphicsUVStartsAtTop)
                            r[i] = new Vector2((i << 1) & 2, 1.0f - (i & 2));
                        else
                            r[i] = new Vector2((i << 1) & 2, i & 2);
                    }

                    return r;
                }
            }
        }
        
        public static void Blit(CommandBuffer cmd, RTHandle source, RTHandle destination, Material material, int pass = 0, bool invert = false, int mip = 0)
        {
            cmd.SetGlobalTexture(s_MainTexId, source);
            cmd.SetRenderTarget(destination, mip);
            cmd.DrawMesh(FullscreenMesh, invert ? s_IndentityInvert : Matrix4x4.identity, material, 0, pass);
        }
        public static void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RTHandle destination, Material material, int pass = 0, bool invert = false, int mip = 0)
        {
            cmd.SetGlobalTexture(s_MainTexId, source);
            cmd.SetRenderTarget(destination, mip);
            cmd.DrawMesh(FullscreenMesh, invert ? s_IndentityInvert : Matrix4x4.identity, material, 0, pass);
        }
        
        public static Vector2 ToNormal(this float rad)
        {
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
        
        public static float Round(this float f)
        {
            return Mathf.Round(f);
        }
        
        public static float Clamp01(this float f)
        {
            return Mathf.Clamp01(f);
        }
        
        public static float OneMinus(this float f)
        {
            return 1f - f;
        }
        
        public static float Remap(this float f, float min, float max)
        {
            return min + (max - min) * f;
        }
        
        public static Color Color()
        {
            return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f),
                             Random.Range(0.0f, 1.0f), 1.0f);
        }
        
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        public static Vector2 To2DXY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        
        public static Vector3 To3DXZ(this Vector2 vector)
        {
            return vector.To3DXZ(0);
        }
        
        public static Vector3 To3DXZ(this Vector2 vector, float y)
        {
            return new Vector3(vector.x, y, vector.y);
        }

        public static Vector3 To3DXY(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        public static Vector2 ToVector2XY(this float value)
        {
            return new Vector2(value, value);
        }
        
        public static Color MulA(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, color.a * a);
        }
        
        public static Rect GetRect(this Texture2D texture)
        {
            return new Rect(0, 0, texture.width, texture.height);
        }
        
        public static int RoundToInt(this float f)
        {
            return Mathf.RoundToInt(f);
        }
        
        public static TKey MaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource noOptionsValue = default)
        {
            var result = source.MaxOrDefault(selector, Comparer<TKey>.Default, noOptionsValue);
            if (Equals(result, default))
                return default;
            
            return selector(result);
        }

        public static TSource MaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource fallback = default)
        {
            using (var sourceIterator = source.GetEnumerator())
            {
                if (sourceIterator.MoveNext() == false)
                    return fallback;

                var max = sourceIterator.Current;
                var maxKey = selector(max);
	
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }
    }
}