using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers
{
    [Serializable]
    public sealed class Optional<T>
    {
        [SerializeField]
        internal bool enabled;

        [SerializeField]
        internal T value = default!;
    
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public T Value
        {
            get => value;
            set => this.value = value;
        }

        // =======================================================================
        public Optional(bool enabled)
        {
            this.enabled = enabled;
        }

        public Optional(T value, bool enabled)
        {
            this.enabled = enabled;
            this.value   = value;
        }

        public T GetValue(T disabledValue)
        {
            return enabled ? value : disabledValue;
        }
        
        public T GetValueOrDefault()
        {
            return enabled ? value : default;
        }
        
        public void Set(Optional<T> val)
        {
            enabled = val.enabled;
            value   = val.value;
        }
        
        public T GetValueOrDefault(T fallback)
        {
            return enabled ? value : fallback;
        }
        
        public static implicit operator bool(Optional<T> opt)
        {
            return opt.enabled;
        }

        public static implicit operator T(Optional<T> opt)
        {
            return opt.value;
        }
    }
}