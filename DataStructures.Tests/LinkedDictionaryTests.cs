using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Xunit;

namespace DataStructures.Tests
{
    public class LinkedDictionaryTests
    {
        [Fact]
        public void Count_AddKeys_CorrectCount()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(3);
            Assert.Equal(3, linkedDictionary.Count);
        }

        [Fact]
        public void Clear_InitializedDictionary_EntryCountBecomesZero()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            linkedDictionary.Clear();
            Assert.Equal(0, linkedDictionary.Count);
        }

        [Fact]
        public void IndexOperator_KnownKey_ReturnsCorrectValue()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            Assert.Equal(4, linkedDictionary[2]);
        }

        [Fact]
        public void IndexOperator_UpdateValue_ValueIsUpdatedAndAddedToEndOfLinkedList()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            linkedDictionary[5] = 100;
            Assert.Equal(100, linkedDictionary.Values.Last());
        }

        [Fact]
        public void IndexOperator_NewValue_ValueIsAddedToEndOfLinkedList()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            linkedDictionary[25] = 25;
            Assert.Equal(25, linkedDictionary.Values.Last());
        }

        [Fact] 
        public void ContainsKey_KeyPresent_ReturnsTrue()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            Assert.True(linkedDictionary.ContainsKey(4));
        }

        [Fact]
        public void ContainsKey_KeyMissing_ReturnsFalse()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            Assert.False(linkedDictionary.ContainsKey(100));
        }

        [Fact]
        public void Contains_KVPMissing_ReturnsFalse()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(15);
            Assert.False(linkedDictionary.Contains(new KeyValuePair<int, int>(16, 32)));
        }

        [Fact]
        public void Contains_KVPPresent_ReturnsTrue()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(15);
            Assert.True(linkedDictionary.Contains(new KeyValuePair<int, int>(10, 20)));
        }

        [Fact]
        public void Contains_KeyPresentButValueDifferent_ReturnsFalse()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(15);
            Assert.False(linkedDictionary.Contains(new KeyValuePair<int, int>(10, 18)));
        }

        [Fact]
        public void CopyTo_NullArray_ThrowsException()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            Assert.ThrowsAny<ArgumentNullException>(() => linkedDictionary.CopyTo(null, 0));
        }

        [Fact]
        public void CopyTo_OutOfRangeIndex_ThrowsException()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            KeyValuePair<int, int>[] targetArray = new KeyValuePair<int, int>[10];
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => linkedDictionary.CopyTo(targetArray, 11));
        }

        [Fact]
        public void CopyTo_InvalidIndex_ThrowsException()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            KeyValuePair<int, int>[] targetArray = new KeyValuePair<int, int>[10];
            Assert.ThrowsAny<ArgumentException>(() => linkedDictionary.CopyTo(targetArray, 1));
        }

        [Fact]
        public void CopyTo_ValidIndex_CopiesKeysInCorrectOrder()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            KeyValuePair<int, int>[] targetArray = new KeyValuePair<int, int>[11];
            linkedDictionary.CopyTo(targetArray, 1);

            var expectedKeys = new List<int>() { 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.Equal<int>(expectedKeys, targetArray.Select(x => x.Key));
        }

        [Fact]
        public void Remove_KeyNotPresent_ReturnsFalseAndDoesntMutateDictionary()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);
            var copyDictionary = InitializeDictionarySequentialKeys(10);

            bool removed = linkedDictionary.Remove(15);
            Assert.False(removed || !DictionariesHaveKVPEquality(linkedDictionary, copyDictionary));
        }

        [Fact]
        public void Remove_KeyPresent_ReturnsTrueAndMutatesDictionary()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(10);

            bool removed = linkedDictionary.Remove(8);
            Assert.True(removed && !linkedDictionary.ContainsKey(8));
        }

        [Fact]
        public void TryGetValue_KeyNotPresent_ReturnsFalseAndAssignsDefaultValue()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(12);
            int value;
            bool success = linkedDictionary.TryGetValue(-5, out value);
            Assert.True(!success && value == default(int));
        }

        [Fact]
        public void TryGetValue_KeyPresent_ReturnsTrueAndRetrievesValue()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(12);
            int value;
            bool success = linkedDictionary.TryGetValue(11, out value);
            Assert.True(success && value == 22);
        }

        [Fact]
        public void CopyConstructor_MutateOriginal_CopyIsUnaffected()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(30);
            var copyDictionary = new LinkedDictionary<int, int>(linkedDictionary);
            linkedDictionary.Remove(15);
            Assert.True(copyDictionary.ContainsKey(15));
        }

        [Fact]
        public void GetEnumerator_AddValueDuringEnumeration_ThrowsException()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(20);
            Action f = () =>
            {
                int i = 0;
                foreach (var kvp in linkedDictionary)
                {
                    i++;
                    if (i == 5)
                    {
                        linkedDictionary.Add(-1, -1);
                    }
                }
            };

            Assert.ThrowsAny<InvalidOperationException>(f);
        }

        [Fact]
        public void GetEnumerator_DeleteValueDuringEnumeration_ThrowsException()
        {
            var linkedDictionary = InitializeDictionarySequentialKeys(20);
            Action f = () =>
            {
                int i = 0;
                foreach (var kvp in linkedDictionary)
                {
                    i++;
                    if (i == 5)
                    {
                        linkedDictionary.Remove(10);
                    }
                }
            };

            Assert.ThrowsAny<InvalidOperationException>(f);
        }

        [Fact]
        public void GetEnumerator_ArbitarilyOrderedDictionary_KeysRetainOrder()
        {
            List<string> keys = new List<string>() { "a", "c", "q", "f", "t", "s", "v", "x", "z", "y" };
            var linkedDictionary = new LinkedDictionary<string, int>();
            foreach (string key in keys)
            {
                linkedDictionary.Add(key, key.GetHashCode());
            }

            Assert.Equal<string>(keys, linkedDictionary.Keys);
        }

        private static LinkedDictionary<int, int> InitializeDictionarySequentialKeys(int nKeys)
        {
            // value = 2*key as default
            return InitializeDictionarySequentialKeys(nKeys, x => 2*x);
        }

        private static LinkedDictionary<int, int> InitializeDictionarySequentialKeys(int nKeys, Func<int, int> valueFunc)
        {
            var linkedDictionary = new LinkedDictionary<int, int>();
            for (int i = 0; i < nKeys; i++)
            {
                linkedDictionary[i] = valueFunc(i);
            }
            return linkedDictionary;
        }

        private static bool DictionariesHaveKVPEquality<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            foreach(var kvp in first)
            {
                if(!second.Contains(kvp))
                {
                    return false;
                }
            }

            foreach(var kvp in second)
            {
                if(!first.Contains(kvp))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
