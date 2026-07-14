using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Web;
using System.Net;

namespace hexreader
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox tb_name;
		private System.Windows.Forms.TextBox tb_stitches;
		private System.Windows.Forms.TextBox tb_NoColors;
		private System.Windows.Forms.TextBox tb_xPos;
		private System.Windows.Forms.TextBox tb_ypos;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox tb1;
		private System.Windows.Forms.TextBox tb2;
		private System.Windows.Forms.TextBox tb3;
		private System.Windows.Forms.ListView lswColors;
		private System.Windows.Forms.ColumnHeader color;
		Embroidery emb = new Embroidery();
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.CheckBox chk_3d;
		private System.Windows.Forms.Button bt_saveColors;
		private System.Windows.Forms.CheckBox chk_Bump;
		private System.Windows.Forms.TextBox tbLight;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TextBox tb_fabric;
		private System.Windows.Forms.TextBox tb_y;
		private System.Windows.Forms.TextBox tb_x;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox cboQuality;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button bt_Settings;
        private Button button5;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
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
				if (components != null) 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tb_name = new System.Windows.Forms.TextBox();
            this.tb_stitches = new System.Windows.Forms.TextBox();
            this.tb_NoColors = new System.Windows.Forms.TextBox();
            this.tb_xPos = new System.Windows.Forms.TextBox();
            this.tb_ypos = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.tb1 = new System.Windows.Forms.TextBox();
            this.tb2 = new System.Windows.Forms.TextBox();
            this.tb3 = new System.Windows.Forms.TextBox();
            this.lswColors = new System.Windows.Forms.ListView();
            this.color = new System.Windows.Forms.ColumnHeader();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.chk_3d = new System.Windows.Forms.CheckBox();
            this.bt_saveColors = new System.Windows.Forms.Button();
            this.chk_Bump = new System.Windows.Forms.CheckBox();
            this.tbLight = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cboQuality = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.tb_fabric = new System.Windows.Forms.TextBox();
            this.tb_y = new System.Windows.Forms.TextBox();
            this.tb_x = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.bt_Settings = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(8, 208);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load info";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(8, 8);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(520, 504);
            this.textBox1.TabIndex = 1;
            // 
            // tb_name
            // 
            this.tb_name.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_name.Location = new System.Drawing.Point(72, 432);
            this.tb_name.Name = "tb_name";
            this.tb_name.Size = new System.Drawing.Size(152, 18);
            this.tb_name.TabIndex = 2;
            // 
            // tb_stitches
            // 
            this.tb_stitches.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_stitches.Location = new System.Drawing.Point(72, 456);
            this.tb_stitches.Name = "tb_stitches";
            this.tb_stitches.Size = new System.Drawing.Size(152, 18);
            this.tb_stitches.TabIndex = 3;
            // 
            // tb_NoColors
            // 
            this.tb_NoColors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_NoColors.Location = new System.Drawing.Point(72, 480);
            this.tb_NoColors.Name = "tb_NoColors";
            this.tb_NoColors.Size = new System.Drawing.Size(152, 18);
            this.tb_NoColors.TabIndex = 4;
            // 
            // tb_xPos
            // 
            this.tb_xPos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_xPos.Location = new System.Drawing.Point(72, 504);
            this.tb_xPos.Name = "tb_xPos";
            this.tb_xPos.Size = new System.Drawing.Size(152, 18);
            this.tb_xPos.TabIndex = 5;
            // 
            // tb_ypos
            // 
            this.tb_ypos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_ypos.Location = new System.Drawing.Point(72, 528);
            this.tb_ypos.Name = "tb_ypos";
            this.tb_ypos.Size = new System.Drawing.Size(152, 18);
            this.tb_ypos.TabIndex = 7;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(336, 16);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(424, 8);
            this.progressBar1.TabIndex = 10;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(16, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(504, 528);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(160, 208);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 24);
            this.button2.TabIndex = 12;
            this.button2.Text = "Show";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tb1
            // 
            this.tb1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb1.Location = new System.Drawing.Point(8, 16);
            this.tb1.Name = "tb1";
            this.tb1.Size = new System.Drawing.Size(168, 18);
            this.tb1.TabIndex = 13;
            this.tb1.Text = "C:\\OIUJ\\apahuve2_2.dst";
            // 
            // tb2
            // 
            this.tb2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb2.Location = new System.Drawing.Point(88, 16);
            this.tb2.Name = "tb2";
            this.tb2.Size = new System.Drawing.Size(100, 18);
            this.tb2.TabIndex = 14;
            this.tb2.Text = "1";
            // 
            // tb3
            // 
            this.tb3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb3.Location = new System.Drawing.Point(88, 240);
            this.tb3.Name = "tb3";
            this.tb3.Size = new System.Drawing.Size(24, 18);
            this.tb3.TabIndex = 15;
            this.tb3.Text = "0";
            // 
            // lswColors
            // 
            this.lswColors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lswColors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.color});
            this.lswColors.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lswColors.FullRowSelect = true;
            this.lswColors.Location = new System.Drawing.Point(8, 264);
            this.lswColors.Name = "lswColors";
            this.lswColors.Size = new System.Drawing.Size(216, 88);
            this.lswColors.TabIndex = 17;
            this.lswColors.UseCompatibleStateImageBehavior = false;
            this.lswColors.View = System.Windows.Forms.View.Details;
            this.lswColors.DoubleClick += new System.EventHandler(this.lswColors_DoubleClick);
            // 
            // color
            // 
            this.color.Text = "";
            this.color.Width = 214;
            // 
            // chk_3d
            // 
            this.chk_3d.Location = new System.Drawing.Point(8, 40);
            this.chk_3d.Name = "chk_3d";
            this.chk_3d.Size = new System.Drawing.Size(104, 24);
            this.chk_3d.TabIndex = 19;
            this.chk_3d.Text = "3d";
            // 
            // bt_saveColors
            // 
            this.bt_saveColors.Location = new System.Drawing.Point(88, 208);
            this.bt_saveColors.Name = "bt_saveColors";
            this.bt_saveColors.Size = new System.Drawing.Size(64, 24);
            this.bt_saveColors.TabIndex = 21;
            this.bt_saveColors.Text = "Save";
            this.bt_saveColors.Click += new System.EventHandler(this.bt_saveColors_Click);
            // 
            // chk_Bump
            // 
            this.chk_Bump.Location = new System.Drawing.Point(8, 0);
            this.chk_Bump.Name = "chk_Bump";
            this.chk_Bump.Size = new System.Drawing.Size(96, 16);
            this.chk_Bump.TabIndex = 22;
            this.chk_Bump.Text = "Bumpmapping";
            // 
            // tbLight
            // 
            this.tbLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbLight.Location = new System.Drawing.Point(40, 24);
            this.tbLight.Name = "tbLight";
            this.tbLight.Size = new System.Drawing.Size(32, 18);
            this.tbLight.TabIndex = 24;
            this.tbLight.Text = "0";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.tb1);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(216, 48);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filename";
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.Location = new System.Drawing.Point(184, 16);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(24, 23);
            this.button4.TabIndex = 14;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cboQuality);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.tb2);
            this.groupBox2.Controls.Add(this.chk_3d);
            this.groupBox2.Location = new System.Drawing.Point(8, 64);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(216, 136);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Drawing";
            // 
            // cboQuality
            // 
            this.cboQuality.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cboQuality.Location = new System.Drawing.Point(168, 40);
            this.cboQuality.Name = "cboQuality";
            this.cboQuality.Size = new System.Drawing.Size(40, 20);
            this.cboQuality.TabIndex = 24;
            this.cboQuality.Text = "1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.chk_Bump);
            this.groupBox3.Controls.Add(this.tbLight);
            this.groupBox3.Location = new System.Drawing.Point(8, 72);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 56);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 23);
            this.label2.TabIndex = 25;
            this.label2.Text = "Light:";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 23);
            this.label1.TabIndex = 15;
            this.label1.Text = "Pen size:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(232, 8);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(536, 568);
            this.tabControl1.TabIndex = 27;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pictureBox1);
            this.tabPage1.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPage1.Location = new System.Drawing.Point(4, 21);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(528, 543);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Picture";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 21);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(528, 543);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Output";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 432);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 28;
            this.label3.Text = "Name:";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 456);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 16);
            this.label4.TabIndex = 29;
            this.label4.Text = "Stitches:";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 480);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 16);
            this.label5.TabIndex = 30;
            this.label5.Text = "# Colors:";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(8, 504);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 23);
            this.label6.TabIndex = 31;
            this.label6.Text = "Width";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(8, 528);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 23);
            this.label8.TabIndex = 33;
            this.label8.Text = "Height";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(16, 240);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 16);
            this.label10.TabIndex = 35;
            this.label10.Text = "Stitch Count";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(40, 568);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(48, 16);
            this.button3.TabIndex = 36;
            this.button3.Text = "button3";
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // tb_fabric
            // 
            this.tb_fabric.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_fabric.Location = new System.Drawing.Point(8, 360);
            this.tb_fabric.Name = "tb_fabric";
            this.tb_fabric.Size = new System.Drawing.Size(216, 18);
            this.tb_fabric.TabIndex = 37;
            // 
            // tb_y
            // 
            this.tb_y.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_y.Location = new System.Drawing.Point(64, 384);
            this.tb_y.Name = "tb_y";
            this.tb_y.Size = new System.Drawing.Size(48, 18);
            this.tb_y.TabIndex = 38;
            this.tb_y.Text = "0";
            // 
            // tb_x
            // 
            this.tb_x.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_x.Location = new System.Drawing.Point(8, 384);
            this.tb_x.Name = "tb_x";
            this.tb_x.Size = new System.Drawing.Size(48, 18);
            this.tb_x.TabIndex = 39;
            this.tb_x.Text = "0";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(56, 392);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(8, 23);
            this.label7.TabIndex = 40;
            this.label7.Text = "x";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(112, 392);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 23);
            this.label9.TabIndex = 41;
            this.label9.Text = "mm";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "VP3|(*.vp3) ";
            // 
            // bt_Settings
            // 
            this.bt_Settings.Location = new System.Drawing.Point(144, 560);
            this.bt_Settings.Name = "bt_Settings";
            this.bt_Settings.Size = new System.Drawing.Size(75, 23);
            this.bt_Settings.TabIndex = 42;
            this.bt_Settings.Text = "Settings";
            this.bt_Settings.Click += new System.EventHandler(this.bt_Settings_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(160, 233);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(64, 23);
            this.button5.TabIndex = 43;
            this.button5.Text = "Show2";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 11);
            this.ClientSize = new System.Drawing.Size(776, 590);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.bt_Settings);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tb_x);
            this.Controls.Add(this.tb_y);
            this.Controls.Add(this.tb_fabric);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bt_saveColors);
            this.Controls.Add(this.lswColors);
            this.Controls.Add(this.tb3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tb_ypos);
            this.Controls.Add(this.tb_xPos);
            this.Controls.Add(this.tb_NoColors);
            this.Controls.Add(this.tb_stitches);
            this.Controls.Add(this.tb_name);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion



		private void button1_Click(object sender, System.EventArgs e)
		{
            emb= EmbroideryHandlers.EmbroideryHandlerFactrory.GetEmbroideryHandler(tb1.Text).GetEmbroidery(tb1.Text);
			//emb.Open(tb1.Text);
			lswColors.Items.Clear();
			for (int a=0;a<emb.NumberOfColors;a++)
			{
				ListViewItem lst = new ListViewItem();
				lst.BackColor=Color.Black;
				if (emb.Colors.GetUpperBound(0)>=a) 
                    lst.BackColor=emb.Colors[a];
				lswColors.Items.Add(lst);
			}
			

			//Name
			//chars = new string (bin.ReadChars(3,16));
			tb_name.Text= emb.Name;
//			//Stitches
//			chars = new string (bin.ReadChars(23,7));
			tb_stitches.Text= emb.NumberOfStiches.ToString();
//			//NOColors
//			chars = new string (bin.ReadChars(34,3));
//			tb_NoColors.Text= Convert.ToString(Convert.ToInt32(chars.ToString().Trim()) +1);
			tb_NoColors.Text=emb.NumberOfColors.ToString();
//			//x+
//			chars = new string (bin.ReadChars(41,5));
			tb_xPos.Text= emb.Width.ToString();
//			//x-
//			chars = new string (bin.ReadChars(50,5));
//			tb_xneg.Text= chars.ToString().Trim();
			//y+
//			chars = new string (bin.ReadChars(59,5));
			tb_ypos.Text= emb.Height.ToString();
//			//y-
//			chars = new string (bin.ReadChars(68,5));
//			tb_yneg.Text= chars.ToString().Trim();
//			
//			//textBox1.Text="";
//			bin.SetPosition(512);
//			long size=bin.Lenght()-1;
//			//long size=Convert.ToInt32(tb_pos.Text);
//			//bin.Close();
//			
//			Pen p = new Pen(Color.DarkTurquoise);
//			int xbefore=Convert.ToInt32(tb_xneg.Text);
//			int ybefore=Convert.ToInt32(tb_yneg.Text);
//			Bitmap img = new Bitmap(Convert.ToInt32(tb_xneg.Text)+Convert.ToInt32(tb_xPos.Text),Convert.ToInt32(tb_ypos.Text)+Convert.ToInt32(tb_yneg.Text));
//			Graphics g = Graphics.FromImage(img);
//			
//			for (int stitch=512;stitch<=size;stitch=stitch+3)
//			{
//			//	tb_pos.Text=stitch.ToString();
//			//	progressBar1.Value=stitch;
//				
//				//byte[] b=new byte[3];
//				byte[] b;
//				b =  bin.ReadNextbytes(3);
//				//b[0]=bin.bytes[stitch];
//				//b[1]=bin.bytes[stitch+1];
//				//b[2]=bin.bytes[stitch+2];
//			//	Application.DoEvents();
//
//				string zeros="00000000";
//			
//				string byte1=Convert.ToString(Convert.ToInt32(b[0].ToString()), 2);
//				byte1=zeros.Substring(0,8-byte1.Length)+ byte1 ;
//				string byte2=Convert.ToString(Convert.ToInt32(b[1].ToString()), 2);
//				byte2=zeros.Substring(0,8-byte2.Length)+ byte2 ;
//				string byte3=Convert.ToString(Convert.ToInt32(b[2].ToString()), 2);
//				byte3=zeros.Substring(0,8-byte3.Length)+ byte3 ;
//
//				int y=0;
//				int x=0;
//				//Byte 1
//				if(byte1.Substring(0,1)=="1")y=y+1;
//				if(byte1.Substring(1,1)=="1")y=y-1;
//				if(byte1.Substring(2,1)=="1")y=y+9;
//				if(byte1.Substring(3,1)=="1")y=y-9;
//				if(byte1.Substring(4,1)=="1")x=x-9;
//				if(byte1.Substring(5,1)=="1")x=x+9;
//				if(byte1.Substring(6,1)=="1")x=x-1;
//				if(byte1.Substring(7,1)=="1")x=x+1;
//				//Byte 2
//				if(byte2.Substring(0,1)=="1")y=y+3;
//				if(byte2.Substring(1,1)=="1")y=y-3;
//				if(byte2.Substring(2,1)=="1")y=y+27;
//				if(byte2.Substring(3,1)=="1")y=y-27;
//				if(byte2.Substring(4,1)=="1")x=x-27;
//				if(byte2.Substring(5,1)=="1")x=x+27;
//				if(byte2.Substring(6,1)=="1")x=x-3;
//				if(byte2.Substring(7,1)=="1")x=x+3;
//				//Byte 3
//				if(byte3.Substring(2,1)=="1")y=y+81;
//				if(byte3.Substring(3,1)=="1")y=y-81;
//				if(byte3.Substring(4,1)=="1")x=x-81;
//				if(byte3.Substring(5,1)=="1")x=x+81;
//
//				switch (byte3.Substring(0,1) + byte3.Substring(1,1) + byte3.Substring(6,1) +byte3.Substring(7,1))
//				{
//
//					case "0011":
//						text+="Normal\r\n"; 
//						g.DrawLine(p,xbefore,ybefore,xbefore+x,ybefore+y);
//						xbefore=xbefore+x;
//						ybefore=ybefore+y;
//						break;
//					case "1011":
//						text+="Jump\r\n"; 
//						xbefore=xbefore+x;
//						ybefore=ybefore+y;
//						break;
//					case "1111":
//						p.Color=Color.DarkRed;
//						text+="Color\r\n"; 
//						break;
//				}
//
//			}
//			bin.Close();
//			img.RotateFlip(RotateFlipType.Rotate180FlipX);
//			pictureBox1.Image = img;
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
            Color[] colors=new Color[emb.NumberOfColors+1];
			int counter=0;
			foreach (ListViewItem lst in lswColors.Items)
			{
				colors[counter]=lst.BackColor;
				counter ++;
			}
            DateTime start = DateTime.Now;
            Image img=emb.GetImage(colors,Convert.ToInt32(tb2.Text),chk_3d.Checked,chk_Bump.Checked,Convert.ToDouble(tbLight.Text),tb_fabric.Text,Convert.ToDouble(tb_x.Text),Convert.ToDouble(tb_y.Text),Convert.ToInt32(cboQuality.Text));
            DateTime stop = DateTime.Now;
			img=Antropoid.Drawing.Image.AddText(img,"Picture created by Antropoid embroidery preview Beta 1");
			img.Save("C:\\preview.jpg");
			pictureBox1.Image=img;
			textBox1.Text=emb.Coordinates;
			double cmWidth=img.Width *0.01;
			double cmHeight=img.Height*0.01;
            pictureBox1.Width = Convert.ToInt32(cmWidth * (Antropoid.Embroidery.Application.Settings.PreviewSize / 5));
            pictureBox1.Height = Convert.ToInt32(cmHeight * (Antropoid.Embroidery.Application.Settings.PreviewSize / 5));
            DateTime full = DateTime.Now;
            MessageBox.Show(start.ToString() + "\r\n" + stop.ToString() + "\r\n" + full.ToString() + "\r\n");

            //if (pictureBox1.Image.Width < pictureBox1.Image.Height)
            //{
            //    double procent = Convert.ToDouble(Convert.ToDouble(pictureBox1.Height) / Convert.ToDouble(pictureBox1.Image.Height));
            //    pictureBox1.Width = Convert.ToInt32(Convert.ToDouble(pictureBox1.Image.Width) * procent);
            //}
            //else
            //{
            //    double procent = Convert.ToDouble(pictureBox1.Width) / Convert.ToDouble(pictureBox1.Image.Width);
            //    pictureBox1.Height = Convert.ToInt32(Convert.ToDouble(pictureBox1.Image.Height) * procent);
            //}
			

		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			FillColor(Color.DarkRed,Color.DarkSlateGray);
		}
		public void FillColor(Color fromColor, Color toColor)
		{
			Bitmap bm=new Bitmap(pictureBox1.Image);
			Color cr1;

			for (int xpos = 0; xpos < bm.Width; xpos++)
			{
				for (int ypos = 0; ypos < bm.Height; ypos++)
				{
					cr1 = bm.GetPixel(xpos, ypos);
					//textBox1.Text+=cr1.R.ToString() + "\r\n";
					if ((cr1.R == fromColor.R && cr1.G == fromColor.G && cr1.B == fromColor.B))
					{
						bm.SetPixel(xpos, ypos, toColor);
					} 
				} 
			} 
			//bm.Width
			//bm.Height
			pictureBox1.Image=bm;
		}

		private void lswColors_DoubleClick(object sender, System.EventArgs e)
		{
				if(lswColors.SelectedItems[0]!=null)
		  {
			  colorDialog1.ShowDialog();
			  lswColors.SelectedItems[0].BackColor=colorDialog1.Color;
		  }
		}

		private void pictureBox1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void button4_Click(object sender, System.EventArgs e)
		{	
			
			
			Bitmap b=new Bitmap(500,500);
			Graphics g = Graphics.FromImage(b);
			GraphicsPath path=new GraphicsPath();
			int xbefore=1;
			int ybefore=1;
			int x=250;
			int y=250;
			Point start=new Point(xbefore,ybefore);
			double dblhyp= Math.Sqrt(Math.Pow(x,2)+Math.Pow(y,2));
			int half=Convert.ToInt32(dblhyp/2);
			Point middle=new Point((xbefore+x)-half,(ybefore+y)-half);
			Point stop=new Point(xbefore+x,ybefore+y);
								
			if (start!=middle && stop!=middle)
			{
				Pen p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(start,middle, Color.Black,Color.Blue),2);
				g.DrawLine(p,start,middle);
				p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(middle,stop,Color.Blue, Color.Black),2);
				g.DrawLine(p,middle,stop);
									
				//p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(xbefore,ybefore), new Point(xbefore+x,ybefore+y),colors[colorNumber-1], Color.Black),stitchsize);
				//g.DrawLine(p,xbefore,ybefore,xbefore+x,ybefore+y);
			}
			else
			{
				g.DrawLine(new Pen(Color.Aquamarine),start,stop);
			}
			
			pictureBox1.Image=b;

		}

		private void bt_saveColors_Click(object sender, System.EventArgs e)
		{
		DataSet ds = new DataSet();

        DataTable myTable = new DataTable();
        DataColumn myColumn = new DataColumn();
        myColumn = new DataColumn("red", System.Type.GetType("System.String"));
        myTable.Columns.Add(myColumn);
        myColumn = new DataColumn("green", System.Type.GetType("System.String"));
        myTable.Columns.Add(myColumn);
        myColumn = new DataColumn("blue", System.Type.GetType("System.String"));
        myTable.Columns.Add(myColumn);
        myTable.TableName = "colors";
        ds.Tables.Add(myTable);
		string [] col=new string[3];

			foreach (ListViewItem lst in lswColors.Items)
			{	col=new string[3];
				col[0]=lst.BackColor.R.ToString();
				col[1]=lst.BackColor.G.ToString();
				col[2]=lst.BackColor.B.ToString();
				ds.Tables[0].Rows.Add(col);
			}
			ds.WriteXml(tb1.Text + ".ytlc");
		
		}

		private void button6_Click(object sender, System.EventArgs e)
		{
		}

		private void tb_pos_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void button3_Click_1(object sender, System.EventArgs e)
		{
			Bitmap b=new Bitmap(500,500);
			Graphics g = Graphics.FromImage(b);
			GraphicsPath path=new GraphicsPath();
			int xbefore=1;
			int ybefore=1;
			int x=20;
			int y=20;
			Point start=new Point(xbefore,ybefore);
			double dblhyp= Math.Sqrt(Math.Pow(x,2)+Math.Pow(y,2));
			int half=Convert.ToInt32(dblhyp/2);
			Point middle=new Point((xbefore+x)-half,(ybefore+y)-half);
			Point stop=new Point(xbefore+x,ybefore+y);
								
			if (start!=middle && stop!=middle)
			{
				Pen p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(stop,middle, Color.Black,Color.Yellow),3);
				g.DrawLine(p,stop,middle);
				middle.Y=middle.Y+1;
				middle.X=middle.X+1;
				p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(start,middle, Color.Black,Color.Yellow),3);
				g.DrawLine(p,start,middle);
//				middle.Y=middle.Y-1;
//					middle.X=middle.X-1;
//				p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(stop,middle, Color.Black,Color.Yellow),3);
//				g.DrawLine(p,middle,stop);
//									
				//p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(xbefore,ybefore), new Point(xbefore+x,ybefore+y),colors[colorNumber-1], Color.Black),stitchsize);
				//g.DrawLine(p,xbefore,ybefore,xbefore+x,ybefore+y);
			}
			else
			{
				g.DrawLine(new Pen(Color.Aquamarine),start,stop);
			}
			
			pictureBox1.Image=b;

		}

		private void button4_Click_1(object sender, System.EventArgs e)
		{
			openFileDialog1.ShowDialog();
			tb1.Text=openFileDialog1.FileName;
		}

		private void bt_Settings_Click(object sender, System.EventArgs e)
		{
			ScreenTest frm = new ScreenTest();
			frm.ShowDialog();
		}

        private void button5_Click(object sender, EventArgs e)
        {
            Color[] colors = new Color[emb.NumberOfColors + 1];
            int counter = 0;
            foreach (ListViewItem lst in lswColors.Items)
            {
                colors[counter] = lst.BackColor;
                counter++;
            }
            DateTime start = DateTime.Now;
            Image img = emb.GetImageByGradient(colors, Convert.ToInt32(tb2.Text), chk_3d.Checked, chk_Bump.Checked, Convert.ToDouble(tbLight.Text), tb_fabric.Text, Convert.ToDouble(tb_x.Text), Convert.ToDouble(tb_y.Text), Convert.ToInt32(cboQuality.Text));
            DateTime stop = DateTime.Now;
            img = Antropoid.Drawing.Image.AddText(img, "Picture created by Antropoid embroidery preview Beta 1");
            img.Save("C:\\preview.jpg");
            pictureBox1.Image = img;
            textBox1.Text = emb.Coordinates;
            double cmWidth = img.Width * 0.01;
            double cmHeight = img.Height * 0.01;
            pictureBox1.Width = Convert.ToInt32(cmWidth * (Antropoid.Embroidery.Application.Settings.PreviewSize / 5));
            pictureBox1.Height = Convert.ToInt32(cmHeight * (Antropoid.Embroidery.Application.Settings.PreviewSize / 5));
            DateTime full = DateTime.Now;
            MessageBox.Show(start.ToString() + "\r\n" + stop.ToString() + "\r\n" + full.ToString() + "\r\n");
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            Embroidery emb = new Embroidery();
            stitch s= new stitch();
            s.x=100;
            s.y=100;
            emb.Stitches.Add(s);
            s = new stitch();
            s.x = 50;
            s.y = 200;
            emb.Stitches.Add(s);


            
        }

        



	}
}
