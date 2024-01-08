using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SocUniqueAttribute : PropertyAttribute
    {
        public Type[] _except;
        
        public SocUniqueAttribute(params Type[] except)
        {
            _except = except;
        }
    }
}