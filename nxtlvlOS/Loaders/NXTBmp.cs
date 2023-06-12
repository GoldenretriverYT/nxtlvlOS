using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Loaders {
    public class NXTBmp {
        public uint[] Data = new uint[0];
        public ushort SizeX, SizeY;

        public NXTBmp(byte[] imgData) {
            SizeX = (ushort)(imgData[0] + (imgData[1] << 8));
            SizeY = (ushort)(imgData[2] + (imgData[3] << 8));

            Data = new uint[SizeX * SizeY];

            Buffer.BlockCopy(imgData, 4, Data, 0, imgData.Length - 4);
        }
    }
}
