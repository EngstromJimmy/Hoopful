using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace hexreader
{
    public partial class UIEmbroidery : UserControl
    {
        private string Filename;
        public UIEmbroidery(Image img,string filename)
        {
            InitializeComponent();
            EmbroideryImage.Image = img;
            lblFilename.Text = Path.GetFileName(filename);
            this.Filename = filename;
        }

        private void EmbroideryImage_DoubleClick(object sender, EventArgs e)
        {
            Embroidery emb = EmbroideryHandlers.EmbroideryHandlerFactrory.GetEmbroideryHandler(this.Filename).GetEmbroidery(this.Filename);
            EmbroideryForm ef = new EmbroideryForm(emb.GetImage(emb.Colors, 3, true, true, 2, "", 0, 0, 1));
            ef.MdiParent = this.FindForm().MdiParent;
            ef.Show();
        }

        private void EmbroideryImage_Click(object sender, EventArgs e)
        {

        }
    }
}
