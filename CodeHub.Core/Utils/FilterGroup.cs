using System.Collections.Generic;
using System.Linq;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Utils
{
    public static class FilterGroup
    {
        public static int[] IntegerCeilings = { 6, 11, 21, 31, 41, 51, 61, 71, 81, 91, 101, 251, 501, 1001, 2001, 4001, 8001, 16001, int.MaxValue };

        private static string CreateRangeString(int key)
        {
            return IntegerCeilings.LastOrDefault(x => x < key) + " to " + (key - 1);
        }

        public static List<IGrouping<string, TElement>> CreateNumberedGroup<TElement>(IEnumerable<IGrouping<int, TElement>> results, string title, string prefix = null)
        {
            return results.Select(x => {
                var text = (prefix != null ? prefix + " " : "") + CreateRangeString(x.Key) + " " + title;
                return (IGrouping<string, TElement>)new FilterGroup<TElement>(text, x.ToList());
            }).ToList();
        }

    }
}

