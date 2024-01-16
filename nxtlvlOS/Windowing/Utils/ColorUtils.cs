using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Utils {
    public class ColorUtils {
        /// <summary>
        /// Super fast alpha blend. Plugged.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Thrown if plug is missing.</exception>
        public static uint AlphaBlend(uint original, uint target) {
            throw new NotImplementedException();
        }

        public const uint Primary100 = 0xFF8888AA;
        public const uint Primary200 = 0xFF777799;
        public const uint Primary300 = 0xFF666688;
        public const uint Primary400 = 0xFF555577;
        public const uint Primary500 = 0xFF444466;
        public const uint Primary600 = 0xFF333355;
        public const uint Primary700 = 0xFF222244;
        public const uint Primary800 = 0xFF111133;
        public const uint Primary900 = 0xFF000022;
    }
}
