using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.ISO9660;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sys = Cosmos.System;

namespace TestFS {
    public class Kernel : Sys.Kernel {
        CosmosVFS vfs = new();
        protected override void BeforeRun() {
            FileSystemManager.Remove("ISO9660");
            FileSystemManager.Register(new FixedISO9660.ISO9660FileSystemFactory());
            VFSManager.RegisterVFS(vfs);
            Console.Clear();

            int diskIdx = 0;
            foreach(var disk in vfs.Disks) {
                Console.WriteLine("== DISK " + diskIdx + " ==");
                Console.WriteLine("Size: " + disk.Size);
                Console.WriteLine("Type: " + (disk.Type == BlockDeviceType.RemovableCD ? "CD" : "HardDrive"));

                int partIdx = 0;

                foreach(var part in disk.Partitions) {
                    Console.WriteLine("== DISK " + diskIdx + ": PART " + partIdx + " ==");
                    Console.WriteLine("HasFS: " + part.HasFileSystem);
                    if(part.HasFileSystem) {
                        Console.WriteLine(" -> FSSize: " + part.MountedFS.Size);
                        Console.WriteLine(" -> FSType: " + part.MountedFS.Type);
                        Console.WriteLine(" -> FSRoot: " + part.MountedFS.RootPath);

                        if(part.MountedFS.Type == "ISO9660") {
                            Tree(part.MountedFS.RootPath);
                        }
                    }

                    partIdx++;
                }

                Console.WriteLine();
                diskIdx++;
            }

            Console.WriteLine("ALL DISKS DONE!");
        }

        private void Tree(string baseDir, int tab = 0) {
            string t = "";

            for (var i = 0; i < tab; i++) t += "    ";

            foreach (var file in Directory.GetFiles(baseDir)) {
                Console.WriteLine(t + "-> File: " + file);
            }

            foreach (var file in Directory.GetDirectories(baseDir)) {
                Console.WriteLine(t + "-> Dir: " + file);
                Tree(baseDir + file + "/", tab + 1);
            }
        }

        protected override void Run() {
            
        }
    }
}
