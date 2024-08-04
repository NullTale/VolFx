using System;
using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Tools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SocTypesAttribute : PropertyAttribute
    {
        public Type[] Types;

        public SocTypesAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}