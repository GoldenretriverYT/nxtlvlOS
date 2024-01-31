using Cosmos.HAL.BlockDevice;
using Cosmos.System;
using Cosmos.System.FileSystem;
using nxtlvlOS.Loaders;
using nxtlvlOS.Processing;
using nxtlvlOS.Services;
using nxtlvlOS.Utils;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using nxtlvlOS.Windowing.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps {
    public class FileExplorer : App {
        private Form fileExplorerForm;

        private ScrollView fileExplorerDiskList, fileExplorerFileList;
        private Font fileExplorerLabelFont;
        private TextField addressBar;

        private string currentPath = "0:\\";

        public override void Exit() {
            if (fileExplorerForm != null && !fileExplorerForm.IsBeingClosed) WindowManager.RemoveForm(fileExplorerForm);
        }

        public override void Init(string[] args) {
            fileExplorerLabelFont = WindowManager.DefaultFont.Copy();
            
            if(fileExplorerLabelFont is TTFFont ttf) {
                ttf.Size = 12;
            }

            fileExplorerForm = new(SelfProcess) {
                SizeX = 600,
                SizeY = 400,
                RelativePosX = 100,
                RelativePosY = 100,
                Title = "File Explorer - nxtlvlOS",
                TitlebarEnabled = true
            };

            fileExplorerForm.Closed += () => {
                ProcessManager.KillProcess(SelfProcess);
            };

            fileExplorerDiskList = new ScrollView {
                RelativePosX = 0,
                RelativePosY = 0,
                SizeX = 64,
                SizeY = 400,
                ContainerSizeX = 64,
                ContainerSizeY = 400,
            };

            fileExplorerFileList = new ScrollView {
                RelativePosX = 64,
                RelativePosY = 34,
                SizeX = 536,
                SizeY = 366,
                ContainerSizeX = 536,
                ContainerSizeY = 366,
            };

            var goUpButton = new TextButton {
                RelativePosX = 70,
                RelativePosY = 5,
                SizeX = 24,
                SizeY = 24,
                Text = "^", // TODO: add proper up icon
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle
            };

            goUpButton.Click += (state, x, y) => {
                if(currentPath != "0:\\") {
                    currentPath = currentPath.Substring(0, currentPath.Length - 1);
                    currentPath = currentPath.Substring(0, currentPath.LastIndexOf('\\') + 1);
                    addressBar.Text = currentPath;
                    LoadFileList();
                }
            };

            var refreshButton = new TextButton {
                RelativePosX = 100,
                RelativePosY = 5,
                SizeX = 24,
                SizeY = 24,
                Text = "R", // TODO: add proper refresh icon
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle
            };

            refreshButton.Click += (state, x, y) => {
                LoadFileList();
            };

            addressBar = new() {
                RelativePosX = 130,
                RelativePosY = 5,
                SizeX = 300,
                SizeY = 24,
                Text = "0:\\",
                EnterIsConfirm = true
            };
            addressBar.Confirmed += () => {
                if(Directory.Exists(addressBar.Text)) {
                    currentPath = addressBar.Text;
                    LoadFileList();
                } else {
                    addressBar.Text = currentPath;
                    // TODO: Show error
                }
            };

            fileExplorerForm.AddChild(goUpButton);
            fileExplorerForm.AddChild(refreshButton);
            fileExplorerForm.AddChild(fileExplorerDiskList);
            fileExplorerForm.AddChild(fileExplorerFileList);
            fileExplorerForm.AddChild(addressBar);

            WindowManager.AddForm(fileExplorerForm);
            
            LoadDiskList();
            LoadFileList();
        }

        public override void Update() {
        }

        public void LoadDiskList() {
            var disks = Kernel.Instance.VFS.GetDisks();
            List<ManagedPartition> partitions = new();

            foreach (var disk in disks) {
                foreach(var part in disk.Partitions) {
                    partitions.Add(part);
                }
            }


            fileExplorerDiskList.ClearItems();

            var yOff = 5;

            foreach (var disk in partitions) {
                var diskButton = new TextButton {
                    RelativePosX = 0,
                    RelativePosY = yOff,
                    SizeX = 64,
                    SizeY = 24,
                    Text = disk.RootPath,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Middle
                };

                diskButton.Click += (state, x, y) => {
                    currentPath = disk.RootPath;
                    LoadFileList();
                };

                fileExplorerDiskList.AddItem(diskButton);

                yOff += 26;
            }

            fileExplorerDiskList.ContainerSizeY = (uint)yOff;
        }

        enum FSType {
            File,
            Directory
        }

        public void LoadFileList() {
            fileExplorerFileList.ClearItems();

            var files = Directory.GetFiles(currentPath);
            var directories = Directory.GetDirectories(currentPath);

            var all = new (FSType type, string name)[files.Length + directories.Length];


            for (int i = 0; i < directories.Length; i++) {
                all[i] = (FSType.Directory, directories[i]);
            }

            for (int i = 0; i < files.Length; i++) {
                all[i + directories.Length] = (FSType.File, files[i]);
            }


            var offX = 0;
            var offY = 5;

            foreach(var entry in all) {
                // Create label
                var label = new TextButton {
                    RelativePosX = offX,
                    RelativePosY = offY + 64,
                    SizeX = 88,
                    SizeY = 24,
                    Text = entry.name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Middle,
                    CustomId = "lbl" + entry.name
                };
                
                void Clicked(MouseState state, uint absX, uint absY) {
                    if (entry.type == FSType.Directory) {
                        currentPath += entry.name + "\\";
                        addressBar.Text = currentPath;
                        LoadFileList();
                    } else {
                        FileAssociationService.Instance.StartAppFromPath(currentPath + entry.name, new string[] { });
                    }
                }

                label.Font = fileExplorerLabelFont;
                label.Click += Clicked;

                // Add icon if file, TODO: directories will get icons later
                // TODO: Put icon loading in a method in FileAssociationService
                if (entry.type == FSType.File) {
                    AssociationFile assoc = new() {
                        Name = "Unknown",
                        NativeTarget = "NotepadApp",
                    };

                    ErrorOr.Resolve(FileAssociationService.Instance.GetAssocFromPath(entry.name), out assoc, assoc);

                    NXTBmp icon = assoc.Icon ?? new NXTBmp(Assets.AssetManager.FileIconGeneric);

                    if (File.Exists(assoc.IconPath)) {
                        icon = new(File.ReadAllBytes(assoc.IconPath));
                    }

                    ImageLabel img = new() {
                        RelativePosX = (int)(label.RelativePosX + (label.SizeX - 64) / 2), // Center image
                        RelativePosY = offY,
                        SizeX = 64,
                        SizeY = 64,
                        Image = (icon.Data)
                    };
                    img.SetTransparent(true);
                    img.Click += Clicked;
                    fileExplorerFileList.AddItem(img);
                }

                fileExplorerFileList.AddItem(label);

                offX += (int)label.SizeX;

                if(offX + label.SizeX > fileExplorerFileList.SizeX) {
                    offX = 0;
                    offY += 90;
                }
            }

            fileExplorerFileList.ContainerSizeY = (uint)offY + 90;
        }
    }
}
