using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nxtlvlOS.Windowing;

namespace nxtlvlOS.Windowing.Utils
{
    public static class IEnumerableHelpers
    {
        public static List<BufferedElement> FlattenElements(List<BufferedElement> start)
        {
            List<BufferedElement> values = new();

            foreach (var item in start)
            {
                values.Add(item);
                values.AddRange(FlattenElements(item.Children));
            }

            return values;
        }
    }
}
