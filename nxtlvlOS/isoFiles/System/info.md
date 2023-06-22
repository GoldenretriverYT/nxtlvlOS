# System
Included files are copied to the main disk in the OOBE. Critical assets (cursors, fallback file icon, etc.) should still be included using ManifestResourceStream.

## Images
Images are using the .bmp extensions even though they are NXTBmps - this is to support 8.3 file names.

## Structure

### AvailBgs
AvailBgs contains backgrounds that can be selected in the desktop appearance settings.

### FileExts
FileExts contains various information about file extensions

#### Icons
Contains the icons for each file extension. Extension: .bmp

#### Assoc
Contains the associations for a file extension. Extension: .asc