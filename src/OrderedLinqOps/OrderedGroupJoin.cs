using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderedLinqOps
{
    public static partial class OrderedLinqOperators
    {
        /// <summary>
        /// Correlates the elements of two ordered sequences based on matching keys and groups the results, using a specified optional comparer.
        /// Yields a result for each element of the outer sequence.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the inputs is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>A collection that has elements of type TResult that are obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="ArgumentException">Any of the input sequences are out of order.</exception>
        public static IEnumerable<TResult> OrderedGroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IReadOnlyCollection<TInner>, TResult> resultSelector, IComparer<TKey>? comparer = null)
        {
            if (outer == null) throw new ArgumentNullException(nameof(outer));
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null)
                comparer = Comparer<TKey>.Default;

            var innerOrdered = inner.OrderedGroupBy(innerKeySelector, (k, i) => (k, i), comparer);
            using (var innerIterator = innerOrdered.GetEnumerator())
            {
                var innerHasValue = innerIterator.MoveNext();
                foreach (var (outerKey, outerValue) in outer.OrderedSelect(outerKeySelector, (k, i) => (k, i), comparer))
                {
                    while (innerHasValue)
                    {
                        var comparisonResult = comparer.Compare(innerIterator.Current.k, outerKey);
                        if (comparisonResult >= 0)
                        {
                            break;
                        }

                        // advance (discard) inner groups that are below current outer key
                        innerHasValue = innerIterator.MoveNext();
                    }

                    var hasMatch = innerHasValue && comparer.Compare(innerIterator.Current.k, outerKey) == 0;
                    var innerGroup = hasMatch ? innerIterator.Current.i : (IReadOnlyCollection<TInner>)Array.Empty<TInner>();
                    yield return resultSelector(outerValue, innerGroup);
                }
            }
        }
    }
}
