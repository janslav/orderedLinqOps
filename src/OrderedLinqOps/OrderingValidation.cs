using System;
using System.Collections.Generic;

namespace OrderedLinqOps
{
    public static partial class OrderedLinqOperators
    {
        private static string BuildOutOfOrderMessage<TKey>(int index, TKey previousKey, TKey currentKey, int comparisonResult, IComparer<TKey> comparer)
        {
            var comparerType = comparer.GetType().FullName ?? comparer.GetType().Name;

            return $"The source collection is not ordered. At zero-based index {index}, previous key was {FormatKeyValue(previousKey)}, but current key is {FormatKeyValue(currentKey)} (comparer.Compare(current, previous) = {comparisonResult}). Comparer: {comparerType}. Ensure the input is sorted by keySelector using the provided comparer. Verify the comparer matches the actual data ordering. If you only need this as a fallback, pre-sort first with .OrderBy(keySelector) before using these streaming operators.";
        }

        private static string FormatKeyValue<TKey>(TKey key)
        {
            return key == null ? "<null>" : $"'{key}'";
        }
    }
}
