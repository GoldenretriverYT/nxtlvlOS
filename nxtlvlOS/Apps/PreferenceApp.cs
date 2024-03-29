﻿using Cosmos.System;
using Cosmos.System.ScanMaps;
using nxtlvlOS.Services;
using nxtlvlOS.Processing;
using nxtlvlOS.Windowing;
using nxtlvlOS.Windowing.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace nxtlvlOS.Apps
{
    public class PreferenceApp : App
    {
        private Form prefForm;

        public override void Exit() {
            if (prefForm != null && !prefForm.IsBeingClosed) {
                prefForm.Close();
            }
        }

        public override void Init(string[] args)
        {
            prefForm = new(SelfProcess) {
                RelativePosX = (1280 - 600) / 2,
                RelativePosY = (720 - 424) / 2,
                SizeX = 600,
                SizeY = 424,
                TitlebarEnabled = true,
                Title = "Preferences - nxtlvlOS",
            };

            prefForm.Closed += () => {
                ProcessManager.KillProcess(SelfProcess);
            };

            #region Create container
            Container prefInputContainer = new() {
                RelativePosX = 0,
                RelativePosY = 0,
            };

            Container actionsContainer = new() {
                RelativePosX = 0,
                RelativePosY = 0,
            };
            #endregion

            #region Create pref elements
            Label prefWMFontTitle = new() {
                SizeX = 580,
                SizeY = 16,
                RelativePosX = 0,
                RelativePosY = 0,
                Text = "Default / Preferred Font",
            };

            // TODO: When dropdowns are added, make this a list of fonts in the 0:\System\Fonts folder
            BasicDropdown prefWMFontField = new() {
                SizeX = 580,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 20
            };
            prefWMFontField.AddElement("system_default");

            foreach (var file in Directory.GetFiles(@"0:\System\Fonts")) {
                if (file.EndsWith(".psf") || file.EndsWith(".ttf")) {
                    prefWMFontField.AddElement(@"0:\System\Fonts\" + file);
                }
            }

            prefWMFontField.SelectElement(SystemPreferenceService.Instance.GetPreferenceOrDefault("wm.default_font", "system_default"));

            Label prefWMFontDesc = new() {
                SizeX = 580,
                SizeY = 32,
                RelativePosX = 0,
                RelativePosY = 48,
                NewlinesEnabled = true,
                Text = "The default font used. Path to PSF file or \"system_default\" \nfor the default font. Apps may use their own fonts."
            };
            #endregion

            #region Create actions
            TextButton prefSaveButton = new() {
                SizeX = 150,
                SizeY = 24,
                RelativePosX = 0,
                RelativePosY = 0,
                Text = "Save"
            };
            prefSaveButton.Click += (state, mX, mY) => {
                SystemPreferenceService.Instance.SetPreference("wm.default_font", prefWMFontField.SelectedElement.Replace("/", "\\"));
                WindowManager.DefaultFont = null; // Reset the cached default font.

                // TODO: Show MessageBox which warns the user that a restart may be required to apply all changes
            };
            #endregion

            #region Add elements to containers
            prefInputContainer.AddChild(prefWMFontTitle);
            prefInputContainer.AddChild(prefWMFontField);
            prefInputContainer.AddChild(prefWMFontDesc);

            actionsContainer.AddChild(prefSaveButton);
            #endregion

            #region Add containers to form
            prefForm.AddChild(prefInputContainer);
            prefForm.AddChild(actionsContainer);
            #endregion

            #region Align containers
            prefInputContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Left, VerticalAlignment.Top, 10, 10);
            actionsContainer.AdjustBoundingBoxAndAlignToParent(HorizontalAlignment.Right, VerticalAlignment.Bottom, 10, 10); // TODO: Fix VerticalAlignment.Bottom (causes element to go outside the bounds)
            #endregion

            WindowManager.AddForm(prefForm);
        }

        public override void Update()
        {

        }
    }
}
