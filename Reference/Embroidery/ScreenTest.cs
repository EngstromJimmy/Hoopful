using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace hexreader
{
	/// <summary>
	/// Summary description for Viewer.
	/// </summary>
	public class ScreenTest : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lbl_Line;
		private System.Windows.Forms.HScrollBar hScrollBar1;
		private System.Windows.Forms.Label lbl_Instructions;
		private System.Windows.Forms.Button bt_OK;
		private System.Windows.Forms.Button bt_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ScreenTest()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lbl_Line = new System.Windows.Forms.Label();
			this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
			this.lbl_Instructions = new System.Windows.Forms.Label();
			this.bt_OK = new System.Windows.Forms.Button();
			this.bt_Cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbl_Line
			// 
			this.lbl_Line.BackColor = System.Drawing.Color.Black;
			this.lbl_Line.Location = new System.Drawing.Point(32, 48);
			this.lbl_Line.Name = "lbl_Line";
			this.lbl_Line.Size = new System.Drawing.Size(500, 8);
			this.lbl_Line.TabIndex = 0;
			// 
			// hScrollBar1
			// 
			this.hScrollBar1.Location = new System.Drawing.Point(16, 64);
			this.hScrollBar1.Maximum = 500;
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.Size = new System.Drawing.Size(536, 17);
			this.hScrollBar1.TabIndex = 1;
			this.hScrollBar1.Value = 500;
			this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
			// 
			// lbl_Instructions
			// 
			this.lbl_Instructions.Location = new System.Drawing.Point(8, 8);
			this.lbl_Instructions.Name = "lbl_Instructions";
			this.lbl_Instructions.Size = new System.Drawing.Size(536, 32);
			this.lbl_Instructions.TabIndex = 2;
			this.lbl_Instructions.Text = "För att få en förhandsgranskning som är i rätt storlek dra i regeln tills den sva" +
				"rta linjen är 5cm stor, klicka sedan på [OK]";
			// 
			// bt_OK
			// 
			this.bt_OK.Location = new System.Drawing.Point(464, 96);
			this.bt_OK.Name = "bt_OK";
			this.bt_OK.TabIndex = 3;
			this.bt_OK.Text = "OK";
			this.bt_OK.Click += new System.EventHandler(this.bt_OK_Click);
			// 
			// bt_Cancel
			// 
			this.bt_Cancel.Location = new System.Drawing.Point(376, 96);
			this.bt_Cancel.Name = "bt_Cancel";
			this.bt_Cancel.TabIndex = 4;
			this.bt_Cancel.Text = "Cancel";
			this.bt_Cancel.Click += new System.EventHandler(this.bt_Cancel_Click);
			// 
			// ScreenTest
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 11);
			this.ClientSize = new System.Drawing.Size(552, 134);
			this.Controls.Add(this.bt_Cancel);
			this.Controls.Add(this.bt_OK);
			this.Controls.Add(this.lbl_Instructions);
			this.Controls.Add(this.hScrollBar1);
			this.Controls.Add(this.lbl_Line);
			this.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Name = "ScreenTest";
			this.Text = "Viewer";
			this.ResumeLayout(false);

		}
		#endregion

		private void hScrollBar1_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			lbl_Line.Width=hScrollBar1.Value;
		}

		private void bt_Cancel_Click(object sender, System.EventArgs e)
		{
			this.Dispose();
		}

		private void bt_OK_Click(object sender, System.EventArgs e)
		{
			Antropoid.Embroidery.Application.Settings.PreviewSize=hScrollBar1.Value;
			Antropoid.Embroidery.Application.Settings.Save();
		}
	}
}
