using Cosmos.Core.Memory;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Utils {
    public class FileUtils {
        /// <summary>
        /// Safely copies a file preventing out of memory crashes
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void SafeCopy(string src, string dst) {
            /*var readStream = VFSManager.GetFileStream(src);

            if (!File.Exists(dst)) File.Create(dst).Close();
            var writeStream = File.OpenWrite(dst);

            byte[] buffer = new byte[16384];
            int totalBytesReadSinceLastCollect = 0;

            while (readStream.CanRead) {
                var readBytes = readStream.Read(buffer);
                if (readBytes == 0) break;
                writeStream.Write(buffer.AsSpan().Slice(0, readBytes));

                totalBytesReadSinceLastCollect += readBytes;

                if(totalBytesReadSinceLastCollect > 1024 * 128) {
                    Heap.Collect();
                    totalBytesReadSinceLastCollect = 0;
                }
            }*/
        }
    }
}
