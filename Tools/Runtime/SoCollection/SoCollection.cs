using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers
{
    [Serializable]
    public class SoCollection<T> : IReadOnlyDictionary<string, T>, IList<T>
        where T : ScriptableObject
    {
        [SerializeField]
        private List<T> m_List = new List<T>();

        public T this[string key] => m_List.FirstOrDefault(n => n.name == key);
        
        public T this[int index]
        {
            get => m_List[index];
            set => m_List[index] = value;
        }

        public IEnumerable<string> Keys   => m_List.Select(n => n.name);
        public IEnumerable<T>      Values => m_List;

        public int  Count      => m_List.Count;
        public bool IsReadOnly => false;

        // =======================================================================
        private class EnumeratorWrapper : IEnumerator<KeyValuePair<string, T>>
        {
            private IEnumerator<T>          m_Source;
            public  KeyValuePair<string, T> Current => new KeyValuePair<string, T>(m_Source.Current.name, m_Source.Current);
            object IEnumerator.             Current => Current;

            // =======================================================================
            public EnumeratorWrapper(IEnumerable<T> source)
            {
                m_Source = source.GetEnumerator();
            }

            public bool MoveNext()
            {
                return m_Source.MoveNext();
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
                m_Source.Dispose();
            }
        }

        // =======================================================================
        public void Add(T item)
        {
            m_List.Add(item);
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public bool Contains(T item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return m_List.Remove(item);
        }

        public int IndexOf(T item)
        {
            return m_List.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            m_List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        {
            return new EnumeratorWrapper(m_List);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public bool ContainsKey(string key)
        {
            return m_List.Any(n => n.name == key);
        }

        public bool TryGetValue(string key, out T value)
        {
            var val = m_List.FirstOrDefault(n => n.name == key);
            if (val == null)
            {
                value = null;
                return false;
            }
            
            value = val;
            return true;
        }
        
        // =======================================================================
        public void Destroy()
        {
#if UNITY_EDITOR
            foreach (var obj in m_List)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(obj);
                Object.DestroyImmediate(obj);
            }
            m_List.Clear();
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}