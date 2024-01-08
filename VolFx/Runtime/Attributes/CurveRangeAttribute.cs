using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CurveRangeAttribute : PropertyAttribute
    {
        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }

        // =======================================================================
        public CurveRangeAttribute() : this(new Vector2(0, 0), new Vector2(1, 1))
        {
        }
        
        public CurveRangeAttribute(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public CurveRangeAttribute(float minX, float minY, float maxX, float maxY)
            : this(new Vector2(minX, minY), new Vector2(maxX, maxY))
        {
        }
    }
}