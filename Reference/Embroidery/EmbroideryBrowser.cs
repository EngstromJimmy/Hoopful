using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace hexreader
{
    
    public partial class EmbroideryBrowser : Form
    {
        public EmbroideryBrowser()
        {
            InitializeComponent();
            
        }

        private void EmbroideryBrowser_Load(object sender, EventArgs e)
        {

        }
        public void LoadPath(string Path)
        {
            int counter=0;
            flowLayoutPanel1.Controls.Clear();
            foreach (string ext in new string[] { "*.vp3", "*.hus", "*.exp", "*.vip", "*.dst", "*.ksm","*.jef","*.sew" })//"*.pes","*.xxx"
            {
                string[] files = Directory.GetFiles(Path, ext);
                foreach (string f in files)
                {
                    Embroidery emb = EmbroideryHandlers.EmbroideryHandlerFactrory.GetEmbroideryHandler(f).GetEmbroidery(f);
                    int Width=0;
                    int Height=0;
                    Image img=emb.GetImage(emb.Colors, 2, false, false, 2, "", 0, 0,4);
                    if (img.Width < img.Height)
                    {
                        double procent = Convert.ToDouble(Convert.ToDouble(150) / Convert.ToDouble(img.Height));
                        Width = Convert.ToInt32(Convert.ToDouble(img.Width) * procent);
                        Height=Convert.ToInt32(Convert.ToDouble(img.Height) * procent);

                    }
                    else
                    {
                        double procent = Convert.ToDouble(150) / Convert.ToDouble(img.Width);
                        Height = Convert.ToInt32(Convert.ToDouble(img.Height) * procent);
                        Width = Convert.ToInt32(Convert.ToDouble(img.Width) * procent);
                    }

                    Bitmap bmp=new Bitmap(img,Width,Height);
                    //bmp.Save("C:\\" + counter.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    //counter++;
                    UIEmbroidery uie = new UIEmbroidery(bmp,f);
                    flowLayoutPanel1.Controls.Add(uie);
                }
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        


    }
}