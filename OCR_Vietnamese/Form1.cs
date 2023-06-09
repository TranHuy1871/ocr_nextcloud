﻿using AForge.Video.DirectShow;      //camera
using System;
using System.Diagnostics;
using System.Drawing;   //bitmap
using System.IO;
using System.Threading.Tasks; //luồng
using System.Windows.Forms;
using Tesseract;    //ocr

namespace OCR_Vietnamese
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection cameras;   //liệt kê các thiết bị video
        private VideoCaptureDevice cam; //nguồn video của cam, ghi lại dữ liệu video từ camera
        string localFilePath;
        public string UserName = "user";
        public string Password = "demo123";
        int xDown              = 0;
        int yDown              = 0;
        int xUp                = 0;
        int yUp                = 0;
        int scaleFactor        = 5;
        Rectangle rectCropArea = new Rectangle();
        MemoryStream ms        = new MemoryStream();
        //string fn = "";
        //Task timeout;
        public Form1()
        {
            InitializeComponent();
            //liệt kê các camera mà máy sử dụng r hiển thị ra combobox cho ng dùng chọn
            cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo info in cameras)
            {
                comboBox1.Items.Add(info.Name);
            }
            comboBox1.SelectedIndex = 0;
            pictureBox1.Cursor = Cursors.Cross;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //pictureBox1.Image.Clone();
            xUp = e.X;
            yUp = e.Y;
            Rectangle rec = new Rectangle(xDown, yDown, Math.Abs(xUp - xDown), Math.Abs(yUp - yDown));
            using (Pen pen = new Pen(Color.Black, 3))
                pictureBox1.CreateGraphics().DrawRectangle(pen, rec);
            rectCropArea = rec;
            crop.Enabled = true;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Invalidate();

            xDown        = e.X;
            yDown        = e.Y;
            crop.Enabled = true;
        }

        //mở file, sử dụng openFileDialog
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                pictureBox1.LoadAsync(openFileDialog.FileName);
        }

        //khởi động webcam
        private void btn_start_Click(object sender, EventArgs e)
        {
            if (cam != null && cam.IsRunning)
                cam.Stop();
            cam = new VideoCaptureDevice(cameras[comboBox1.SelectedIndex].MonikerString);
            cam.NewFrame += Cam_NewFrame;
            cam.Start();
        }

        private void Cam_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap     = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bitmap;
        }

        //chụp ảnh
        private void btn_takephoto_Click(object sender, EventArgs e)
        {
            if (cam != null && cam.IsRunning)
                cam.Stop();
        }

        //lưu ảnh
        private void btn_save_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = "F:\\Thực tập cs cn\\Images";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                pictureBox1.Image.Save(saveFileDialog1.FileName);
        }

        //chuyển ảnh qua text
        private void button2_Click(object sender, EventArgs e)
        {
            string result = "";
            Task.Factory.StartNew(() =>
            {
                picloading.BeginInvoke(new Action(() =>
                {
                    picloading.Visible = true;
                }));

                //thực hiện convert
                result = OCR((Bitmap)pictureBox1.Image);
                richTextBox1.BeginInvoke(new Action(() =>
                {
                    richTextBox1.Text = result;

                }));
                picloading.BeginInvoke(new Action(() =>
                {
                    picloading.Visible = false;
                }));
            });
        }

        //convert
        private string OCR(Bitmap b)
        {
            string res = "";
            using (var engine = new TesseractEngine(@"tessdata", "vie", EngineMode.Default))
            {
                using (var page = engine.Process(b, PageSegMode.AutoOnly))
                    res = page.GetText();
            }
            return res;
        }

        private void Crop(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Refresh();
                //Prepare a new Bitmap on which the cropped image will be drawn
                Bitmap sourceBitmap = new Bitmap(pictureBox1.Image, pictureBox1.Width, pictureBox1.Height);
                Graphics g          = pictureBox1.CreateGraphics();

                //Draw the image on the Graphics object with the new dimesions
                g.DrawImage(sourceBitmap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height), rectCropArea, GraphicsUnit.Pixel);
                sourceBitmap.Dispose();
                crop.Enabled = false;
                var path     = Environment.CurrentDirectory.ToString();
                ms           = new MemoryStream();
                pictureBox1.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] ar   = new byte[ms.Length];
                var timeout = ms.WriteAsync(ar, 0, ar.Length);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Crop thất bại! Lỗi: {ex.Message}");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                var imagerl = pictureBox1.Image;
                imagerl.RotateFlip(RotateFlipType.Rotate90FlipXY);
                pictureBox1.Image = imagerl;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                var imagerl = pictureBox1.Image;
                imagerl.RotateFlip(RotateFlipType.Rotate90FlipXY);
                pictureBox1.Image = imagerl;
            }
        }

        private void btn_zoom_in_Click(object sender, EventArgs e)
        {
            pictureBox1.Height += scaleFactor;
            pictureBox1.Width += scaleFactor;
        }

        private void btn_zoom_out_Click(object sender, EventArgs e)
        {
            pictureBox1.Height -= scaleFactor;
            pictureBox1.Width -= scaleFactor;
        }

        private void UploadFile(string localFilePath, string remoteFilePath)
        {
            string curlArguments = $"-k -u {UserName}:{Password} -T \"{localFilePath}\" \"{remoteFilePath}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName               = "curl",
                Arguments              = curlArguments,
                UseShellExecute        = false,
                RedirectStandardError  = true,
                RedirectStandardOutput = true,
                CreateNoWindow         = true
            };

            Process process = new Process { StartInfo = startInfo };

            process.Start();

            //string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"Upload thất bại. Error: {error}");
        }

        private void btn_saveresult_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|RTF Files (*.rtf)|*.rtf";
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var extension = System.IO.Path.GetExtension(saveFileDialog.FileName);
                if (extension.ToLower() == ".txt") /*saveFileDialog.FilterIndex==1*/
                    richTextBox1.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.PlainText);
                else
                    richTextBox1.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.RichText);
            }
        }

        private void btn_uploadnextcloud_Click(object sender, EventArgs e)
        {
            try
            {
                //string localFilePath;
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|RTF Files (*.rtf)|*.rtf";
                saveFileDialog.AddExtension = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox1.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.PlainText);
                    var extension = Path.GetExtension(saveFileDialog.FileName);
                    localFilePath = saveFileDialog.FileName;
                }
                string fileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                //string localFilePath = "F:\\Curl\\TestCurl.txt";
                string remoteFilePath = $"https://demo21.nextfiles.eu/remote.php/webdav/Test/{fileName}.txt";

                UploadFile(localFilePath, remoteFilePath);
                MessageBox.Show("Upload thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Upload thất bại! Lỗi: {ex.Message}");
            }
        }
    }
}