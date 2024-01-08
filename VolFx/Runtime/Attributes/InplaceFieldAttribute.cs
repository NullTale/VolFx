using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InplaceFieldAttribute : PropertyAttribute
    {
        public string[] PropertyPath;

        public InplaceFieldAttribute(params string[] propertyPath)
        {
            PropertyPath = propertyPath;
        }
    }
}