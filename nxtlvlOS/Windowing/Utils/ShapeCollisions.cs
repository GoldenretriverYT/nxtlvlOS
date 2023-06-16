using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Windowing.Utils {
    public class ShapeCollisions {
        public static bool RectIntersectsWithPoint(uint top, uint left, uint bottom, uint right, uint posX, uint posY){
            return (posX > left && posX < right &&
                posY > top && posY < bottom);
        }
    }
}
