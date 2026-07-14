using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for Hus.
	/// </summary>
	public class HUSHandler:IEmbroideryHandler
	{

		#region Hus
		public Embroidery GetEmbroidery(string filename)
		{
			Embroidery emb=new Embroidery();
			Binary bin = new Binary(filename);
			bin.Open();
			byte [] b;
			b = bin.Readbytes(4,4);
			emb.NumberOfStiches= GetIntegerFromHex(b);
			//Number of Colors
			b = bin.Readbytes(8,4);
			emb.NumberOfColors= GetIntegerFromHex(b);
			//x+
			b = bin.Readbytes(12,2);
			emb.PositiveX= GetIntegerFromHex(b);
			//x-
			b = bin.Readbytes(16,2);
			emb.NegativeX=0xffff - GetIntegerFromHex(b);
			//y+
			b = bin.Readbytes(14,2);
			emb.PositiveY=GetIntegerFromHex(b);
			//y-
			b = bin.Readbytes(18,2);
			emb.NegativeY=0xffff -GetIntegerFromHex(b);

			emb.Width=emb.NegativeX+emb.PositiveX;
			emb.Height=emb.NegativeY+emb.PositiveY;
            emb.StartXOffset = emb.NegativeX;
            emb.StartYOffset = emb.NegativeY;
			
			//Load Colors
			if (File.Exists(filename + ".ytlc"))
			{
				DataSet ds = new DataSet();
				ds.ReadXml(filename + ".ytlc");
				emb.Colors=new Color[ds.Tables[0].Rows.Count];
				for (int a=0;a<ds.Tables[0].Rows.Count;a++)
				{
					emb.Colors[a]=Color.FromArgb(Convert.ToInt32(ds.Tables[0].Rows[a]["red"]),Convert.ToInt32(ds.Tables[0].Rows[a]["green"]),Convert.ToInt32(ds.Tables[0].Rows[a]["blue"]));
				}
			}
			else
			{
				int c=0;
				b = bin.Readbytes(42,2 * emb.NumberOfColors);
				emb.Colors=new Color[emb.NumberOfColors];
				for (int a=0;a<b.Length;a++)
				{
					switch (b[a])
					{
						case 0:
							emb.Colors[c]=Color.FromArgb(0,0,0);
							c++;
							break;
						case 1:
							emb.Colors[c]=Color.FromArgb(0,0,255);
							c++;
							break;
						case 2:
							emb.Colors[c]=Color.FromArgb(0,255,0);
							c++;
							break;
						case 3:
							emb.Colors[c]=Color.FromArgb(255,0,0);
							c++;
							break;
						case 4:
							emb.Colors[c]=Color.FromArgb(255,0,255);
							c++;
							break;
						case 5:
							emb.Colors[c]=Color.FromArgb(255,255,0);
							c++;
							break;
						case 6:
							emb.Colors[c]=Color.FromArgb(132,130,132);
							c++;
							break;
						case 7:
							emb.Colors[c]=Color.FromArgb(0,130,255);
							c++;
							break;
						case 8:
							emb.Colors[c]=Color.FromArgb(0,255,132);
							c++;
							break;
						case 9:
							emb.Colors[c]=Color.FromArgb(255,130,0);
							c++;
							break;
						case 10:
							emb.Colors[c]=Color.FromArgb(255,162,181);
							c++;
							break;
						case 11:
							emb.Colors[c]=Color.FromArgb(198,65,0);
							c++;
							break;
						case 12:
							emb.Colors[c]=Color.FromArgb(255,255,255);
							c++;
							break;
						case 13:
							emb.Colors[c]=Color.FromArgb(0,0,132);
							c++;
							break;
						case 14:
							emb.Colors[c]=Color.FromArgb(0,130,0);
							c++;
							break;
						case 15:
							emb.Colors[c]=Color.FromArgb(165,0,0);
							c++;
							break;
						case 16:
							emb.Colors[c]=Color.FromArgb(255,121,123);
							c++;
							break;
						case 17:
							emb.Colors[c]=Color.FromArgb(132,0,132);
							c++;
							break;
						case 18:
							emb.Colors[c]=Color.FromArgb(255,130,255);
							c++;
							break;
						case 19:
							emb.Colors[c]=Color.FromArgb(198,195,0);
							c++;
							break;
						case 20:
							emb.Colors[c]=Color.FromArgb(255,255,165);
							c++;
							break;
						case 21:
							emb.Colors[c]=Color.FromArgb(66,65,66);
							c++;
							break;
						case 22:
							emb.Colors[c]=Color.FromArgb(198,195,198);
							c++;
							break;
						case 23:
							emb.Colors[c]=Color.FromArgb(231,65,0);
							c++;
							break;
						case 24:
							emb.Colors[c]=Color.FromArgb(255,174,66);
							c++;
							break;
						case 25:
							emb.Colors[c]=Color.FromArgb(255,89,123);
							c++;
							break;
						case 26:
							emb.Colors[c]=Color.FromArgb(255,211,214);
							c++;
							break;
						case 27:
							emb.Colors[c]=Color.FromArgb(132,32,0);
							c++;
							break;
						case 28:
							emb.Colors[c]=Color.FromArgb(231,97,33);
							c++;
							break;				
					}
					a++;
				}

				byte[] s1=bin.Readbytes(20,4);
				byte[] s2=bin.Readbytes(24,4);
				byte[] s3=bin.Readbytes(28,4);
				int possec1=GetIntegerFromHex(s1);
				int possec2=GetIntegerFromHex(s2);
				int possec3=GetIntegerFromHex(s3);
			

			
				byte[] CompressedSection1=bin.Readbytes(possec1,possec2-possec1);
				byte[] DecompressedSection1=Greenleaf.ArchiveLibrary.DecompressBytes(CompressedSection1);

				byte[] CompressedSection2=bin.Readbytes(possec2,possec3-possec2);
				byte[] DecompressedSection2=Greenleaf.ArchiveLibrary.DecompressBytes(CompressedSection2);

                byte[] CompressedSection3 = bin.Readbytes(possec3, Convert.ToInt32(bin.Lenght()) - possec3);
				byte[] DecompressedSection3=Greenleaf.ArchiveLibrary.DecompressBytes(CompressedSection3);


				for(int a=0;a<DecompressedSection2.Length;a++)
				{
					stitch s =new stitch();
					
					switch(DecompressedSection1[a])
					{
						case 0x80: //Normal Stitch
							s.type=stitchtype.Normal;
							break;
						case 0x81: //Jump Stitch
							s.type=stitchtype.Jump;
							break;
						case 0x84: //Color change
							s.type=stitchtype.ColorChange;
							break;
						case 0x90: //Last stitch in pattern
						
							break;
						default:
							break;
					}
					s.x=Convert.ToInt32(Convert.ToInt32(DecompressedSection2[a].ToString()).ToString("x"),16);
					s.y=Convert.ToInt32(Convert.ToInt32(DecompressedSection3[a].ToString()).ToString("x"),16);
					if (s.x>128)
						s.x=s.x-256;
					if(s.y>128)
						s.y=s.y-256;
					emb.Stitches.Add (s);
				}
			}
			bin.Close();
			return emb;
		}
		#endregion

		private int GetIntegerFromHex(byte[] Offset)
		{
			string sectionoffset="";
			string section="";

			for(int a=Offset.Length-1;a>=0;a--)
				if(Offset[a].ToString()!="0")
				{
					section="0";
					section=Convert.ToInt32(Offset[a].ToString()).ToString("x");
					if (section.Length<2)
						section="0" + section;
					sectionoffset+= section;
				}
			if(sectionoffset!="")
				return Convert.ToInt32(sectionoffset,16);
			else
				return 0;

		}
		}
}
