using System.Collections.Generic;

namespace GalacticWasteManagement.Utilities
{

    public static class IntersperseExtension
    {
        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
        {
            var enumerator = source.GetEnumerator();
            var hasNext = enumerator.MoveNext();
            while(hasNext){
                
                yield return enumerator.Current;
                hasNext = enumerator.MoveNext();
                yield return element;
            }
        }
    }
}
