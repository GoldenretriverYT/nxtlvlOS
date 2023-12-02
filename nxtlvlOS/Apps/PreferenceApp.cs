using Cosmos.System;
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
            if (prefForm != null) prefForm.Close();
        }

        public override void Init(string[] args)
        {
            prefForm = new(SelfProcess);
            prefForm.RelativePosX = (1280 - 600) / 2;
            prefForm.RelativePosY = (720 - 424) / 2;
            prefForm.SizeX = 600;
            prefForm.SizeY = 424;
            prefForm.SetTitlebarEnabled(true);
            prefForm.SetTitle("Preferences - nxtlvlOS");

            #region Create container
            Container prefInputContainer = new();
            prefInputContainer.RelativePosX = 0;
            prefInputContainer.RelativePosY = 0;

            Container actionsContainer = new();
            actionsContainer.RelativePosX = 0;
            actionsContainer.RelativePosY = 0;
            #endregion

            #region Create pref elements
            Label prefWMFontTitle = new();
            prefWMFontTitle.SizeX = 580;
            prefWMFontTitle.SizeY = 16;
            prefWMFontTitle.RelativePosX = 0;
            prefWMFontTitle.RelativePosY = 0;
            prefWMFontTitle.SetText("Window Manager Font");

            // TODO: When dropdowns are added, make this a list of fonts in the 0:\System\Fonts folder
            TextField prefWMFontField = new();
            prefWMFontField.SizeX = 580;
            prefWMFontField.SizeY = 24;
            prefWMFontField.RelativePosX = 0;
            prefWMFontField.RelativePosY = 20;
            prefWMFontField.SetText(SystemPreferenceService.Instance.GetPreferenceOrDefault("wm.default_font", "system_default"));

            Label prefWMFontDesc = new();
            prefWMFontDesc.SizeX = 580;
            prefWMFontDesc.SizeY = 32;
            prefWMFontDesc.RelativePosX = 0;
            prefWMFontDesc.RelativePosY = 48;
            prefWMFontDesc.SetNewlinesEnabled(true);
            prefWMFontDesc.SetText("The font used by the window manager. Path to PSF file \nor \"system_default\" for the default font.");
            #endregion

            #region Create actions
            TextButton prefSaveButton = new();
            prefSaveButton.SizeX = 150;
            prefSaveButton.SizeY = 24;
            prefSaveButton.RelativePosX = 0;
            prefSaveButton.RelativePosY = 0;
            prefSaveButton.SetText("Save");
            prefSaveButton.Click = (state, mX, mY) => {
                SystemPreferenceService.Instance.SetPreference("wm.default_font", prefWMFontField.Text.Replace("/", "\\"));
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
