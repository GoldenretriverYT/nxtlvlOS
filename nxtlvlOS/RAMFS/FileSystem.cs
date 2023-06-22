using Cosmos.Core;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using nxtlvlOS.Loaders;
using nxtlvlOS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.RAMFS {
    // Maybe take a look at how we could possibly serialize this into a byte array so we can like store on a BlockDevice
    // Paths are structured like this: /mydir/eedgd/test123.txt
    //                                  ^dir ^dir   ^name   ^ext
    public class FileSystem {
        public FSNode RootNode = new() {
            Type = FSNodeType.Directory,
            Name = "RootDirectory__DO__NOT__INCLUDE__IN__PATH"
        };

        public FileSystem() {
            LoadFS();
        }

        public ErrorOrNothing WriteAllBytes(string path, byte[] data) {
            var pathInfo = ParsePath(path);
            var directory = GetDirectory(pathInfo.Directory);

            Kernel.Instance.Logger.Log(LogLevel.Sill, "Writing " + data.Length + " bytes to " + pathInfo.FullName + " in directory " + pathInfo.Directory);  

            if(directory.IsError) {
                return ErrorOrNothing.MakeError("WriteAllBytes: Resolving directory failed: " + directory.Error);
            }

            if(!FileExists(path)) {
                InsertNodeIn(directory.Data, new() {
                    Name = pathInfo.FullName,
                    Type = FSNodeType.File,
                    Content = data
                });
            }else {
                var file = GetNodeIn(directory.Data, FSNodeType.File, pathInfo.FullName);

                if(file.IsError) {
                    return ErrorOrNothing.MakeError("WriteAllBytes: File apparently exists but was not resolved somehow: " + file.Error);
                }

                file.Data.Content = data;
            }

            return ErrorOrNothing.MakeResult();
        }

        public ErrorOrNothing WriteAllText(string path, string text, Encoding encoding = null) {
            if(encoding == null) {
                encoding = Encoding.ASCII;
            }

            return WriteAllBytes(path, encoding.GetBytes(text));
        }

        public ErrorOr<byte[]> ReadAllBytes(string path) {
            var pathInfo = ParsePath(path);
            var directory = GetDirectory(pathInfo.Directory);

            if (directory.IsError) {
                return ErrorOr<byte[]>.MakeError("ReadAllBytes: Resolving directory failed: " + directory.Error);
            }

            if (!FileExists(path)) {
                return ErrorOr<byte[]>.MakeError("ReadAllBytes: File was not found!");
            } else {
                var file = GetNodeIn(directory.Data, FSNodeType.File, pathInfo.FullName);

                if (file.IsError) {
                    return ErrorOr<byte[]>.MakeError("ReadAllBytes: File apparently exists but was not resolved somehow: " + file.Error);
                }

                return ErrorOr<byte[]>.MakeResult(file.Data.Content);
            }
        }

        public ErrorOr<string> ReadAllText(string path, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.ASCII;
            }

            var res = ReadAllBytes(path);

            if(res.IsError) {
                return ErrorOr<string>.MakeError(res.Error);
            }

            return ErrorOr<string>.MakeResult(encoding.GetString(res.Data));
        }

        public ErrorOr<List<FSNode>> GetDirectories(string directoryPath) {
            var directory = GetDirectory(directoryPath);

            if (directory.IsError) {
                return ErrorOr<List<FSNode>>.MakeError("GetDirectories: Resolving directory failed: " + directory.Error);
            }

            List<FSNode> l = new();

            foreach(var node in directory.Data.ChildNodes) {
                if (node.Type == FSNodeType.Directory) l.Add(node);
            }

            return ErrorOr<List<FSNode>>.MakeResult(l);
        }

        public ErrorOrNothing CreateDirectory(string path) {
            if (path.EndsWith("/")) path = path.TrimEnd('/');

            var pathInfo = ParsePath(path);
            var directory = GetDirectory(pathInfo.Directory);

            if (directory.IsError) {
                Kernel.Instance.Logger.Log(LogLevel.Fail, "CreateDirectory failed with path " + path + " due to " + directory.Error);
                return ErrorOrNothing.MakeError("CreateDirectory: Resolving parts of path failed: " + directory.Error);
            }

            InsertNodeIn(directory.Data, new() {
                Name = pathInfo.Name,
                Type = FSNodeType.Directory
            });

            Kernel.Instance.Logger.Log(LogLevel.Verb, "Added directory " + pathInfo.Name + " to " + directory.Data.Name);

            return ErrorOrNothing.MakeResult();
        }

        public ErrorOr<List<FSNode>> GetFiles(string directoryPath) {
            var directory = GetDirectory(directoryPath);

            if (directory.IsError) {
                return ErrorOr<List<FSNode>>.MakeError("GetFiles: Resolving directory failed: " + directory.Error);
            }

            Kernel.Instance.Logger.Log(LogLevel.Verb, "listing files of " + directory.Data.Name);

            List<FSNode> l = new();

            foreach (var node in directory.Data.ChildNodes) {
                if (node.Type == FSNodeType.File) l.Add(node);
            }

            return ErrorOr<List<FSNode>>.MakeResult(l);
        }

        private void InsertNodeIn(FSNode parentNode, FSNode newNode) {
            parentNode.ChildNodes.Add(newNode);
        }

        public bool FileExists(string path) {
            var pathInfo = ParsePath(path);
            var directory = GetDirectory(pathInfo.Directory);

            if (directory.IsError) return false;

            var file = GetNodeIn(directory.Data, FSNodeType.File, pathInfo.FullName);

            if (file.IsError) return false;
            return true;
        }

        public bool DirectoryExists(string path) {
            var dir = GetDirectory(path);

            return !dir.IsError;
        }

        public ErrorOr<FSNode> GetDirectory(string path) {
            var parts = GetDirectoryParts(path);

            var currentNode = RootNode;
            var currentPartIdx = 0;

            Kernel.Instance.Logger.Log(LogLevel.Sill, "GetDirectory called with " + path + "; got " + parts.Length + " parts from that");

            while(currentPartIdx < parts.Length) {
                var result = GetNodeIn(currentNode, FSNodeType.Directory, parts[currentPartIdx]);
                currentPartIdx++;

                if(result.IsError && result.Error == "Not found") {
                    return ErrorOr<FSNode>.MakeError("GetDirectory: The full path or a part of the path was not found (part not found: " + parts[currentPartIdx-1]);
                }

                currentNode = result.Data;
            }

            return ErrorOr<FSNode>.MakeResult(currentNode);
        }

        public ErrorOr<FSNode> GetFile(string path) {
            var parts = GetDirectoryParts(path);

            var currentNode = RootNode;
            var currentPartIdx = 0;

            while (currentPartIdx < parts.Length) {
                var result = GetNodeIn(currentNode, FSNodeType.Directory, parts[currentPartIdx]);
                currentPartIdx++;

                if (result.IsError && result.Error == "Not found") {
                    return ErrorOr<FSNode>.MakeError("GetFile: The full path or a part of the path was not found");
                }

                currentNode = result.Data;
            }

            return GetNodeIn(currentNode, FSNodeType.File, parts[parts.Length - 1]);
        }

        private ErrorOr<FSNode> GetNodeIn(FSNode node, FSNodeType searchingFor, string dirName) {
            foreach(var child in node.ChildNodes) {
                if(child.Type == searchingFor && child.Name == dirName) {
                    return ErrorOr<FSNode>.MakeResult(child);
                }
            }

            return ErrorOr<FSNode>.MakeError("Not found");
        }

        public void StoreFS() {
            try {
                if (Kernel.Instance.VFS != null) {
                    if (Kernel.Instance.VFS.Disks.Count < 1) return;
                    var disk = Kernel.Instance.VFS.Disks[0];

                    ManagedPartition foundPart = null;

                    foreach (var part in disk.Partitions) {
                        if (!part.HasFileSystem) {
                            foundPart = part;
                            break;
                        }
                    }

                    if (foundPart == null) {
                        disk.CreatePartition(1024 * 1024 * 100);
                        foreach (var part in disk.Partitions) {
                            if (!part.HasFileSystem) {
                                foundPart = part;
                                break;
                            }
                        }
                    }

                    if (foundPart == null) return;

                    var node = SerializeFSNode(RootNode);
                    Kernel.Instance.Logger.Log(LogLevel.Info, "Got serialized node!");
                    var json = JSONSerializer.Serialize(node);
                    Kernel.Instance.Logger.Log(LogLevel.Info, "Got JSON!");

                    int blockCount = (json.Length / 512) + 1;

                    for (var i = 0; i < blockCount; i++) {
                        var data = Encoding.ASCII.GetBytes(json[(i * 512)..Math.Min(json.Length, ((i + 1) * 512))]);
                        if(data.Length != 512) {
                            var tmp = new byte[512];
                            Array.Copy(data, tmp, 512);
                            data = tmp;
                        }
                        foundPart.Host.WriteBlock((ulong)i, 1, ref data);
                        GCImplementation.Free(data);
                    }
                }
            }catch(Exception ex) {
                Kernel.Instance.Logger.Log(LogLevel.Fail, "Storing failed: " + ex.Message);
            }
        }

        public void LoadFS() {
            if (Kernel.Instance != null && Kernel.Instance.VFS != null) {
                Kernel.Instance.Logger.Log(LogLevel.Info, "Attempting to restore RAMFS");
                if (Kernel.Instance.VFS.Disks.Count < 1) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "No disks found!");
                    return;
                }

                var disk = Kernel.Instance.VFS.Disks[0];

                ManagedPartition foundPart = null;

                foreach (var part in disk.Partitions) {
                    if (!part.HasFileSystem) {
                        foundPart = part;
                        break;
                    }
                }

                if (foundPart == null) {
                    disk.CreatePartition(1024 * 1024 * 100);
                    foreach (var part in disk.Partitions) {
                        if (!part.HasFileSystem) {
                            foundPart = part;
                            break;
                        }
                    }
                }

                if (foundPart == null) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "No partition found!");
                    return;
                }

                var firstBlock = new byte[512];
                foundPart.Host.ReadBlock(0, 1, ref firstBlock);

                if (firstBlock[0] != (byte)'{' || firstBlock[1] != (byte)'"') {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "Partition is not the RAMFS!");
                    return;
                }

                List<byte[]> fileParts = new();
                ulong i = 0;

                while(true) {
                    byte[] data = new byte[512];
                    foundPart.Host.ReadBlock(i, 1, ref data);
                    fileParts.Add(data);

                    if (data[511] == 0) break;
                    i++;
                }

                string finalJsonString = "";

                foreach(var arr in fileParts) {
                    finalJsonString += Encoding.ASCII.GetString(arr);
                    GCImplementation.Free(arr);
                }

                try {
                    JSONParser parser = new(finalJsonString);
                    var obj = parser.Parse();
                    RootNode = DeserializeFSNode(obj);

                    Kernel.Instance.Logger.Log(LogLevel.Info, "Restored RAMFS!");
                    DumpFSNode(RootNode);
                }catch(Exception ex) {
                    Kernel.Instance.Logger.Log(LogLevel.Fail, "Could not restore FS: invalid json! " + ex.Message + "       " + finalJsonString);
                }
            }
        }

        public void DumpFSNode(FSNode node, int tab = 0) {
            string pref = "";
            for(int i = 0; i < tab; i++) {
                pref += " ";
            }

            Kernel.Instance.mDebugger.Send(pref + "Name: " + node.Name);
            Kernel.Instance.mDebugger.Send(pref + "Type: " + (node.Type == FSNodeType.File ? "File" : "Directory"));
            Kernel.Instance.mDebugger.Send(pref + "ContentLen: " + node.Content.Length);
            Kernel.Instance.mDebugger.Send(pref + "Children: ");

            if(node.ChildNodes.Count == 0) {
                Kernel.Instance.mDebugger.Send(pref + "  None!");
            }else {
                foreach(var child in node.ChildNodes) {
                    DumpFSNode(child, tab + 2);
                }
            }
        }

        public Dictionary<string, object> SerializeFSNode(FSNode node) {
            Kernel.Instance.Logger.Log(LogLevel.Sill, "Serializing FSNode " + node.Name);
            var res = new Dictionary<string, object>();

            res["Name"] = node.Name;
            res["Type"] = node.Type == FSNodeType.File ? "File" : "Directory";
            res["Content"] = Convert.ToBase64String(node.Content);

            var children = new List<object>();
            
            foreach(var child in node.ChildNodes) {
                children.Add(SerializeFSNode(child));
            }

            res["Children"] = children;

            return res;
        }

        public FSNode DeserializeFSNode(Dictionary<string, object> data) {
            var node = new FSNode();
            node.Name = data["Name"] as string;
            node.Type = ((data["Type"] as string) == "File") ? FSNodeType.File : FSNodeType.Directory;
            node.Content = Convert.FromBase64String(data["Content"] as string);

            var childList = data["Children"] as object[];
            Kernel.Instance.Logger.Log(LogLevel.Sill, "Deserializing node, got child list of type " + data["Children"].GetType().Name + " with " + childList.Length + " elements!");

            foreach (var child in childList) {
                node.ChildNodes.Add(DeserializeFSNode((Dictionary<string, object>)child));
            }

            return node;
        }


        public static string[] GetDirectoryParts(string path) {
            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts;
        }

        public static PathInfo ParsePath(string path) {
            string[] parts = path.Split('/');

            string directory = "";
            string fileName = parts[parts.Length - 1];
            int dotIndex = -1;

            for (int i = fileName.Length - 1; i >= 0; i--) {
                if (fileName[i] == '.') {
                    dotIndex = i;
                    break;
                }
            }

            string name = dotIndex >= 0 ? fileName.Substring(0, dotIndex) : fileName;
            string extension = dotIndex >= 0 && dotIndex < fileName.Length - 1 ? fileName.Substring(dotIndex + 1) : "";

            if (parts.Length > 1) {
                directory = string.Join("/", parts, 0, parts.Length - 1);
            }

            return new PathInfo(directory, name, extension);
        }
    }

    public static class FSExtensions {
        public static string[] GetNames(this List<FSNode> nodes) {
            var res = new string[nodes.Count];

            for (var i = 0; i < nodes.Count; i++) res[i] = nodes[i].Name;

            return res;
        }
    }

    public class PathInfo {
        public string Directory { get; }
        public string Name { get; }
        public string Extension { get; }

        public string FullName => Name + (Extension == "" ? "" : "." + Extension);

        public PathInfo(string directory, string name, string extension) {
            Directory = directory;
            Name = name;
            Extension = extension;
        }
    }

    public class FSNode {
        public string Name = "";
        public List<FSNode> ChildNodes = new();
        public FSNodeType Type = FSNodeType.File;
        public byte[] Content = new byte[0];
    }

    public enum FSNodeType {
        File,
        Directory
    }
}
