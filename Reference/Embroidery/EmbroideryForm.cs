using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hexreader
{
    public partial class EmbroideryForm : Form
    {
        public EmbroideryForm(Image img)
        {
            InitializeComponent();
            double cmWidth = img.Width * 0.01;
            double cmHeight = img.Height * 0.01;
            Bitmap bmp = new Bitmap(img, Convert.ToInt32(cmWidth * (Antropoid.Embroidery.Application.Settings.PreviewSize / 5)), Convert.ToInt32(cmHeight * (Antropoid.Embroidery.Application.Settings.PreviewSize / 5)));
            pictureBox1.Image = bmp;
        }

        private void EmbroideryForm_Load(object sender, EventArgs e)
        {

        }
    }
}