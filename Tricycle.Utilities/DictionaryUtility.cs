using System;
using System.Collections.Generic;

namespace Tricycle.Utilities
{
    public static class DictionaryUtility
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return dictionary.TryGetValue(key, out var value) ? value : default;
        }
    }
}
