using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for DSTHandler.
	/// </summary>
	public class DSTHandler:IEmbroideryHandler
	{
		#region DST
		public Embroidery GetEmbroidery(string filename)
		{
			Embroidery emb=new Embroidery();
			Binary bin = new Binary(filename);
			//pFilename=filename;
			bin.Open();
			string chars;
			//Name
			chars = new string (bin.ReadChars(3,16));
//			emb.Name= chars.ToString();
			//Number of Stitches
			chars = new string (bin.ReadChars(23,7));
			emb.NumberOfStiches= Convert.ToInt32(chars.ToString().Trim());
			//Number of Colors
			chars = new string (bin.ReadChars(34,3));
			emb.NumberOfColors= Convert.ToInt32(chars.ToString().Trim()) +1;
			//x+
			chars = new string (bin.ReadChars(41,5));
			emb.PositiveX= Convert.ToInt32(chars.ToString().Trim());
			//x-
			chars = new string (bin.ReadChars(50,5));
			emb.NegativeX=Convert.ToInt32(chars.ToString().Trim());
			//y+
			chars = new string (bin.ReadChars(59,5));
			emb.PositiveY=Convert.ToInt32(chars.ToString().Trim());
			//y-
			chars = new string (bin.ReadChars(68,5));
			emb.NegativeY=Convert.ToInt32(chars.ToString().Trim());

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
				emb.Colors=new Color[emb.NumberOfColors];
				for (int a=0;a<emb.NumberOfColors;a++)
				{
					emb.Colors[a]=Color.Black;
				}
			}
			


			//Read data
			
			bin.SetPosition(512);
			long size=bin.Lenght()-1;
			int xbefore=emb.NegativeX;
			int ybefore=emb.NegativeY;
			stitch s;
			long MaxStitch=0;
			if (MaxStitch==0) 
				MaxStitch=size;
			
			byte[] b=new byte[3];
			for (int stitchcount=512;stitchcount<=MaxStitch;stitchcount=stitchcount+3)
			{
				try
				{
					b =  bin.ReadNextbytes(3);
					if ((b[0].ToString()=="243") || (b[1].ToString()=="243")|| (b[2].ToString()=="243"))
						break;
					int y=0;
					int x=0;
					//Byte1
					if ((b[0]&0x80)==128) y=y+1;
					if ((b[0]&0x40)==64) y=y-1;
					if ((b[0]&0x20)==32) y=y+9;
					if ((b[0]&0x10)==16) y=y-9;
					if ((b[0]&0x08)==8) x=x-9;
					if ((b[0]&0x04)==4)x=x+9;
					if ((b[0]&0x02)==2) x=x-1;
					if ((b[0]&0x01)==1)x=x+1;

					//Byte2
					if ((b[1]&0x80)==128) y=y+3;
					if ((b[1]&0x40)==64) y=y-3;
					if ((b[1]&0x20)==32) y=y+27;
					if ((b[1]&0x10)==16) y=y-27;
					if ((b[1]&0x08)==8) x=x-27;
					if ((b[1]&0x04)==4)x=x+27;
					if ((b[1]&0x02)==2) x=x-3;
					if ((b[1]&0x01)==1)x=x+3;

					//Byte3
					if ((b[2]&0x20)==32) y=y+81;
					if ((b[2]&0x10)==16) y=y-81;
					if ((b[2]&0x08)==8) x=x-81;
					if ((b[2]&0x04)==4)x=x+81;


					//Color Change
					if(( (b[2]&0xC0) ==192) &&  ((b[2]&0x03)==3))
					{
						s=new stitch();
						s.type=stitchtype.ColorChange;
						emb.Stitches.Add (s);
					}
					//Normal
					if(((b[2]&0xC0)==0))
					{
						if (x!=0 || y!=0)
						{	
							s=new stitch();
							s.x=x;
							s.y=y;
							s.type=stitchtype.Normal;
							emb.Stitches.Add (s);
						}
					}

					//Jump
					if(((b[2]&0x80)==128) &&  ((b[2]&0x03)==3))
					{
						s=new stitch();
						s.x=x;
						s.y=y;
						
						s.type=stitchtype.Jump;
						emb.Stitches.Add (s);
					}
				}
				catch
				{
					MaxStitch=size;
				}

			}
			bin.Close();
			return emb;
		}

		#endregion
	}
}
