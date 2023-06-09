using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Utils {
    internal static class IEnumerableExtensions {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> start, Func<T, IEnumerable<T>> on) {
            List<T> values = new();

            foreach(var item in start) {
                values.Add(item);
                values.AddRange(Flatten(on(item), on));
            }

            return values;
        }
    }
}
