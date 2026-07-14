using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for KSMHandler.
	/// </summary>
	public class KSMHandler:IEmbroideryHandler
	{
	
#region KSM
		public Embroidery GetEmbroidery(string filename)
		{	
				Embroidery emb=new Embroidery();
				Binary bin = new Binary(filename);
				bin.Open();
				int xpos=0;
				int ypos=0;

				//Read data
			
				bin.SetPosition(512);
				long size=bin.Lenght()-1;
				int xbefore=emb.NegativeX;
				int ybefore=emb.NegativeY;
                emb.StartXOffset = emb.NegativeX;
                emb.StartYOffset = emb.NegativeY;
				stitch s;
				long MaxStitch=0;
				if (MaxStitch==0) 
					MaxStitch=size;
			
				byte[] b=new byte[3];
				for (int stitchcount=512;stitchcount<=size;stitchcount=stitchcount+3)
				{
					try
					{
						b =  bin.ReadNextbytes(3);
	
						int y=0;
						int x=0;
					
						//Colorchange
						//					if ((b[0]==0x80)&&(b[1]==0x01))
						//					{
						//						s=new stitch();
						//						s.type=stitchtype.ColorChange;
						//						Stitches.Add (s);
						//						NumberOfColors++;
						//					}
					
						//Normal
						if ((b[2]&0x19)!=25)
						{
							x=(int)b[0];
							y=(int)b[1];
						
							if ((b[2]&0x40)==64)
							{
								y=y*-1;
							}
							if ((b[2]&0x20)==32)
							{
								x=x*-1;
							}

							if (x!=0 || y!=0)
							{	
								s=new stitch();
								s.x=x;
								s.y=y;
								s.type=stitchtype.Normal;
								emb.Stitches.Add (s);
								emb.NumberOfStiches++;
							}

							xpos=xpos+x;
							ypos=ypos+y;

							if(xpos<emb.NegativeX)
								emb.NegativeX=xpos;
							if (xpos>emb.PositiveX)
								emb.PositiveX=xpos;
							if (ypos<emb.NegativeY)
								emb.NegativeY=ypos;
							if (ypos>emb.PositiveY)
								emb.PositiveY=ypos;
						
						}
						else
						{//ColorChange
							s=new stitch();
							s.type=stitchtype.ColorChange;
							emb.Stitches.Add (s);
							emb.NumberOfColors++;

							x=(int)b[0];
							y=(int)b[1];
						
							if ((b[2]&0x40)==64)
							{
								y=y*-1;
							
							}
							if ((b[2]&0x20)==32)
							{
								x=x*-1;
							}

							if (x!=0 || y!=0)
							{	
								s=new stitch();
								s.x=x;
								s.y=y;
								s.type=stitchtype.Jump;
								emb.Stitches.Add (s);
								//NumberOfStiches++;
							}

							xpos=xpos+x;
							ypos=ypos+y;

							if(xpos<emb.NegativeX)
								emb.NegativeX=xpos;
							if (xpos>emb.PositiveX)
								emb.PositiveX=xpos;
							if (ypos<emb.NegativeY)
								emb.NegativeY=ypos;
							if (ypos>emb.PositiveY)
								emb.PositiveY=ypos;
						}


						//Jump
						//					if ((b[0]==0x80)&&(b[1]==0x02)||(b[1]==0x04))
						//					{
						//						b =  bin.ReadNextbytes(2);
						//						x=(int)b[0];
						//						y=(int)b[1];
						//
						//						if (y>128)
						//						{
						//							y=(256-y)*-1;
						//						}
						//						
						//						if (x>128)
						//						{
						//							x=(256-x)*-1;
						//						}
						//
						//						stitchcount=stitchcount+2;
						//						if (x!=0 || y!=0)
						//						{	
						//							s=new stitch();
						//							s.x=x;
						//							s.y=y;
						//							s.type=stitchtype.Jump;
						//							Stitches.Add (s);
						//						}
						//						xpos=xpos+x;
						//						ypos=ypos+y;
						//
						//						if(xpos<NeagativeX)
						//							NeagativeX=xpos;
						//						if (xpos>PositiveX)
						//							PositiveX=xpos;
						//						if (ypos<NeagativeY)
						//							NeagativeY=ypos;
						//						if (ypos>PositiveY)
						//							PositiveY=ypos;
						//					}
					

					}
					catch
					{
						MaxStitch=size;
					}

				}
				emb.rotate=true;
				emb.NegativeX=emb.NegativeX*-1;
				emb.NegativeY=emb.NegativeY*-1;



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

				emb.Width=emb.NegativeX+emb.PositiveX;
				emb.Height=emb.NegativeY+emb.PositiveY;

				bin.Close();
			return emb;
			}

		#endregion	}

	
	}
}
