using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections;

namespace DataStructures
{
    [Serializable]
    public class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue> 
    {
        private Entry header;
        private Dictionary<TKey, Entry> entryMap;
        private int version;
        private KeyCollection keyCollection;
        private ValueCollection valueCollection;

        public LinkedDictionary() : this(0, null) { }

        public LinkedDictionary(int capacity) : this(capacity, null) { }

        public LinkedDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public LinkedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.entryMap = new Dictionary<TKey, Entry>(capacity, comparer);
            this.header = new Entry(default(TKey), default(TValue));
            this.header.After = this.header;
            this.header.Before = this.header;
        }

        public LinkedDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public LinkedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer):
            this(dictionary != null? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException();
            }

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                this.Add(kvp);
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return entryMap[key].Value; }

            set
            {
                Entry entry;
                if (entryMap.TryGetValue(key, out entry))
                {
                    this.UpdateValueInLinkedList(entry, value);
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }

        public int Count
        {
            get { return entryMap.Count; }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (keyCollection == null) keyCollection = new KeyCollection(this);
                return keyCollection;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (valueCollection == null) valueCollection = new ValueCollection(this);
                return valueCollection;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            version++;
            Entry entry = new Entry(key, value);
            entryMap.Add(key, entry);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            if (entryMap.Count > 0)
            {
                entryMap.Clear();
                header = null;
                version++;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            Entry entry;
            if (entryMap.TryGetValue(item.Key, out entry))
            {
                return (entry.Value != null && entry.Value.Equals(item.Value));
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return entryMap.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException();
            }

            foreach (var kvp in this)
            {
                array.SetValue(kvp, arrayIndex++);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException();
            }

            Entry entry;
            if (entryMap.TryGetValue(item.Key, out entry))
            {
                if(entry.Value != null && entry.Value.Equals(item.Value))
                {
                    return Remove(item.Key);
                }
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            Entry entry;
            bool removed = false;
            if (entryMap.TryGetValue(key, out entry))
            {
                version++;
                removed = entryMap.Remove(key);
                this.RemoveFromLinkedList(entry);
            }
            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Entry entry;
            bool success = entryMap.TryGetValue(key, out entry);
            if (success)
            {
                value = entry.Value;
            }
            else
            {
                value = default(TValue);
            }
            return success;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }


        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        private void UpdateValueInLinkedList(Entry entry, TValue newValue)
        {
            version++;
            RemoveFromLinkedList(entry);
            entry.Value = newValue;
            InsertInLinkedList(entry);
        }

        private void RemoveFromLinkedList(Entry entry)
        {
            entry.Before.After = entry.After;
            entry.After.Before = entry.Before;
        }

        private void InsertInLinkedList(Entry entry)
        {
            entry.Before = header;
            entry.After = header;
            header.Before.After = entry;
            header.Before = entry;
        }

        private class KeyCollection : ICollection<TKey>, IReadOnlyCollection<TKey>
        {
            private LinkedDictionary<TKey, TValue> dictionary;

            public KeyCollection(LinkedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }
                this.dictionary = dictionary;
            }

            public int Count
            {
                get { return this.dictionary.Count; }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return this.dictionary.ContainsKey(item);
            }

            void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if (arrayIndex < 0 || arrayIndex > array.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (array.Length - arrayIndex < dictionary.Count)
                {
                    throw new ArgumentException();
                }

                int count = this.Count;
                foreach (TKey key in this)
                {
                    array[arrayIndex++] = key;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this.dictionary);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator(this.dictionary);
            }

            [Serializable]
            public class Enumerator : IEnumerator<TKey>
            {
                private LinkedDictionary<TKey, TValue> dictionary;
                private int version;
                private Entry current;

                internal Enumerator(LinkedDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    this.version = dictionary.version;
                    this.current = dictionary.header;
                }

                public bool MoveNext()
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException();
                    }

                    if (current.After == dictionary.header)
                    {
                        return false;
                    }

                    current = current.After;
                    return true;
                }

                object IEnumerator.Current
                {
                    get { return this.Current; }
                }

                public TKey Current
                {
                    get
                    {
                        if (version != dictionary.version)
                        {
                            throw new InvalidOperationException();
                        }
                        return current.Key;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException();
                    };

                    this.current = dictionary.header;
                }

                void IDisposable.Dispose() { }
            }
        }

        public sealed class ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>
        {
            private LinkedDictionary<TKey, TValue> dictionary;

            public ValueCollection(LinkedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }
                this.dictionary = dictionary;
            }

            public int Count
            {
                get { return this.dictionary.Count; }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                if (item == null)
                {
                    foreach (TValue value in this)
                    {
                        if (value == null)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    foreach (TValue value in this)
                    {
                        if (item.Equals(value))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if (arrayIndex < 0 || arrayIndex > array.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (array.Length - arrayIndex < dictionary.Count)
                {
                    throw new ArgumentException();
                }

                int count = this.Count;
                foreach (TValue value in this)
                {
                    array[arrayIndex++] = value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this.dictionary);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator(this.dictionary);
            }

            [Serializable]
            public class Enumerator : IEnumerator<TValue>
            {
                private LinkedDictionary<TKey, TValue> dictionary;
                private readonly int version;
                private Entry current;

                internal Enumerator(LinkedDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    this.version = dictionary.version;
                    this.current = dictionary.header;
                }

                public bool MoveNext()
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException();
                    }

                    if (current.After == dictionary.header)
                    {
                        return false;
                    }

                    current = current.After;
                    return true;
                }

                object IEnumerator.Current
                {
                    get { return this.Current; }
                }

                public TValue Current
                {
                    get
                    {
                        if (version != dictionary.version)
                        {
                            throw new InvalidOperationException();
                        }
                        return current.Value;
                    }
                }

                void IEnumerator.Reset()
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException();
                    };

                    this.current = dictionary.header;
                }


                void IDisposable.Dispose() { }
            }
        }

        [Serializable]
        public class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private LinkedDictionary<TKey, TValue> dictionary;
            private readonly int version;
            private Entry current;

            internal Enumerator(LinkedDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
                this.version = dictionary.version;
                this.current = dictionary.header;
            }

            public bool MoveNext()
            {
                if (version != dictionary.version)
                {
                    throw new InvalidOperationException();
                }

                if (current.After == dictionary.header)
                {
                    return false;
                }

                current = current.After;
                return true;
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (version != dictionary.version)
                    {
                        throw new InvalidOperationException();
                    }
                    return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                }
            }

            void IEnumerator.Reset()
            {
                if (version != dictionary.version)
                {
                    throw new InvalidOperationException();
                };

                this.current = dictionary.header;
            }


            void IDisposable.Dispose() { }
        }

        [Serializable]
        protected class Entry
        {
            private readonly TKey key;
            private TValue val;
            private Entry after;
            private Entry before;

            public Entry(TKey key, TValue value)
            {
                this.key = key;
                this.val = value;
            }

            public TKey Key
            {
                get { return key; }
            }

            public TValue Value
            {
                get { return val; }
                set { this.val = value; }
            }

            public Entry After
            {
                get { return after; }
                set { after = value; }
            }

            public Entry Before
            {
                get { return before; }
                set { before = value; }
            }

            public override int GetHashCode()
            {
                return (key == null ? 0 : key.GetHashCode()) ^
                       (val == null ? 0 : val.GetHashCode());
            }

            public bool Equals(Entry other)
            {
                if (this == other)
                {
                    return true;
                }

                TKey otherKey = other.key;
                if (key != null && key.Equals(otherKey))
                {
                    TValue otherValue = other.val;
                    if (val != null && val.Equals(otherValue))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override bool Equals(object obj)
            {
                Entry other = obj as Entry;
                if (other == null)
                {
                    return false;
                }

                return this.Equals(other);
            }

            public override string ToString()
            {
                return key + ":" + val;
            }
        }
    }
}
