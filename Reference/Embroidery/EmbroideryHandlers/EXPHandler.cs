using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for EXPHandler.
	/// </summary>
	public class EXPHandler:IEmbroideryHandler
	{
	#region EXP
		public Embroidery GetEmbroidery(string filename)
		{

				Binary bin=new Binary(filename);
				Embroidery emb=new Embroidery();
                emb.NumberOfColors++;	
                bin.Open();
				int xpos=0;
				int ypos=0;

				//Read data
				bin.SetPosition(0);
				long size=bin.Lenght()-1;
                emb.StartXOffset = emb.NegativeX;
                emb.StartYOffset = emb.NegativeY;
				int xbefore=emb.NegativeX;
				int ybefore=emb.NegativeY;
				stitch s;
				long MaxStitch=0;
				if (MaxStitch==0) 
					MaxStitch=size;
			
				byte[] b=new byte[2];
				for (int stitchcount=0;stitchcount<=size;stitchcount=stitchcount+2)
				{
					try
					{
						b =  bin.ReadNextbytes(2);
	
						int y=0;
						int x=0;
					
						//Colorchange
						if ((b[0]==0x80)&&(b[1]==0x01))
						{
							s=new stitch();
							s.type=stitchtype.ColorChange;
							emb.Stitches.Add (s);
                            emb.NumberOfColors++;
						}
					
						//Normal
						if ((b[0]!=0x80))
						{
							x=(int)b[0];
							y=(int)b[1];
						
							if (y>128)
							{
								y=(256-y)*-1;
							}
							if (x>128)
							{
								x=(256-x)*-1;
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

						//Jump
						if ((b[0]==0x80)&&(b[1]==0x02)||(b[1]==0x04))
						{
							b =  bin.ReadNextbytes(2);
							x=(int)b[0];
							y=(int)b[1];

							if (y>128)
							{
								y=(256-y)*-1;
							}
						
							if (x>128)
							{
								x=(256-x)*-1;
							}

							stitchcount=stitchcount+2;
							if (x!=0 || y!=0)
							{	
								s=new stitch();
								s.x=x;
								s.y=y;
								s.type=stitchtype.Jump;
								emb.Stitches.Add (s);
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
					

					}
					catch
					{
						MaxStitch=size;
					}

				}

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

				emb.NegativeX=emb.NegativeX*-1;
				emb.NegativeY=emb.NegativeY*-1;
				emb.Width=emb.NegativeX+emb.PositiveX;
				emb.Height=emb.NegativeY+emb.PositiveY;
			
				bin.Close();
			return emb;
			}
	#endregion
		}

	}

