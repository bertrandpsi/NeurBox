using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.Utilities
{
    public static class EnumerationOperations
    {
        public static IEnumerable<(TTypeA, TTypeB)> Mix<TTypeA, TTypeB>(this IEnumerable<TTypeA> a, IEnumerable<TTypeB> b)
        {
            var enumerator = b.GetEnumerator();
            foreach (var av in a)
            {
                enumerator.MoveNext();
                yield return (av, enumerator.Current);
            }
        }

        public static IEnumerable<(TType, TType)> AllCouplePermutations<TType>(this IEnumerable<TType> enumerableSource)
        {
            List<TType> source;
            if (enumerableSource is List<TType>)
                source = enumerableSource as List<TType>;
            else
                source = enumerableSource.ToList();
            var alreadyChecked = new Dictionary<(int, int), bool>();
            for (int i = 0; i < source.Count; i++)
            {
                for (int j = 0; j < source.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (alreadyChecked.ContainsKey((i, j)) || alreadyChecked.ContainsKey((j, i)))
                        continue;
                    alreadyChecked.Add((i, j), true);
                    yield return (source[i], source[j]);
                }
            }
        }
    }
}
