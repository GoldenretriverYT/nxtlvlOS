using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.ISO9660;

namespace FixedISO9660 {
    public class ISO9660FileSystemFactory : FileSystemFactory {
        public override string Name => "ISO9660";

        public override FileSystem Create(Partition aDevice, string aRootPath, long aSize) {
            return new ISO9660FileSystem(aDevice, aRootPath, aSize);
        }

        public override bool IsType(Partition aDevice) {
            var primarySectory = aDevice.NewBlockArray(1);
            aDevice.ReadBlock(0x10, 1, ref primarySectory);
            var str = Encoding.ASCII.GetString(primarySectory, 1, 5);
            Console.WriteLine(str);
            if (str == "CD001") {
                return true;
            } else {
                return false;
            }
        }
    }
}