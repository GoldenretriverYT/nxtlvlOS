namespace BMPtoNXTBMP {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private byte[] newData = new byte[0];

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            Bitmap bmp = new Bitmap(openFileDialog1.FileName);
            newData = new byte[4 + (bmp.Height * bmp.Width * 4)];

            BitConverter.GetBytes((ushort)bmp.Width).CopyTo(newData, 0);
            BitConverter.GetBytes((ushort)bmp.Height).CopyTo(newData, 2);

            for (var x = 0; x < bmp.Width; x++) {
                for (var y = 0; y < bmp.Height; y++) {
                    BitConverter.GetBytes((uint)bmp.GetPixel(x, y).ToArgb()).CopyTo(newData, 4 + ((y * bmp.Width) + x) * 4);
                }
            }

            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            File.WriteAllBytes(saveFileDialog1.FileName, newData);
        }
    }
}