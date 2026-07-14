namespace hexreader
{
    partial class UIEmbroidery
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.EmbroideryImage = new System.Windows.Forms.PictureBox();
            this.lblFilename = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.EmbroideryImage)).BeginInit();
            this.SuspendLayout();
            // 
            // EmbroideryImage
            // 
            this.EmbroideryImage.BackColor = System.Drawing.Color.White;
            this.EmbroideryImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EmbroideryImage.Location = new System.Drawing.Point(3, 3);
            this.EmbroideryImage.Name = "EmbroideryImage";
            this.EmbroideryImage.Size = new System.Drawing.Size(150, 150);
            this.EmbroideryImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.EmbroideryImage.TabIndex = 0;
            this.EmbroideryImage.TabStop = false;
            this.EmbroideryImage.DoubleClick += new System.EventHandler(this.EmbroideryImage_DoubleClick);
            this.EmbroideryImage.Click += new System.EventHandler(this.EmbroideryImage_Click);
            // 
            // lblFilename
            // 
            this.lblFilename.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblFilename.Location = new System.Drawing.Point(3, 156);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(152, 21);
            this.lblFilename.TabIndex = 1;
            this.lblFilename.Text = "Filname.ext";
            this.lblFilename.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // UIEmbroidery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblFilename);
            this.Controls.Add(this.EmbroideryImage);
            this.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "UIEmbroidery";
            this.Size = new System.Drawing.Size(158, 179);
            ((System.ComponentModel.ISupportInitialize)(this.EmbroideryImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox EmbroideryImage;
        private System.Windows.Forms.Label lblFilename;
    }
}
