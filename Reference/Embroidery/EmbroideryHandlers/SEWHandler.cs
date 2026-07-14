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
	public class SEWHandler:IEmbroideryHandler
	{
	#region SEW
        Color[] ColorTable = new Color[80]
        {    
             Color.FromArgb(192,192,192),  //Not in use
             Color.FromArgb(0,0,0),         
             Color.FromArgb(240,240,240),   
             Color.FromArgb(255,255,23),    
             Color.FromArgb(255,102,0),     
             Color.FromArgb(47,89,51),      
             Color.FromArgb(35,115,54),     
             Color.FromArgb(101,194,200),   
             Color.FromArgb(171,90,50),                 
             Color.FromArgb(246,105,160),   
             Color.FromArgb(255,0,0),       //10
             Color.FromArgb(156,100,69),    
             Color.FromArgb(11,47,132),     
             Color.FromArgb(228,195,93),    
             Color.FromArgb(72,26,5),           
             Color.FromArgb(171,156,199),   
             Color.FromArgb(253,145,181),   
             Color.FromArgb(249,153,183),   
             Color.FromArgb(250,179,129),   
             Color.FromArgb(215,189,164),   
             Color.FromArgb(151,5,51),      //20
             Color.FromArgb(160,184,204),   
             Color.FromArgb(127,194,28),    
             Color.FromArgb(229,229,229),   
             Color.FromArgb(136,155,155),   
             Color.FromArgb(152,214,189),   
             Color.FromArgb(178,225,227),      
             Color.FromArgb(152,243,254),   
             Color.FromArgb(112,169,226),   
             Color.FromArgb(29,84,120),    
             Color.FromArgb(7,22,80),      //30
             Color.FromArgb(255,187,187),   
             Color.FromArgb(255,96,72),     
             Color.FromArgb(255,90,39),     
             Color.FromArgb(226,161,136),   
             Color.FromArgb(181,148,116),   
             Color.FromArgb(245,219,139),   
             Color.FromArgb(255,204,0),     
             Color.FromArgb(255,189,227),   
             Color.FromArgb(195,0,126),     
             Color.FromArgb(168,0,67),      //40
             Color.FromArgb(84,5,113),      
             Color.FromArgb(255,9,39),      
             Color.FromArgb(198,238,203),   
             Color.FromArgb(96,133,65),     
             Color.FromArgb(96,148,24),     
             Color.FromArgb(6,72,13),       
             Color.FromArgb(91,210,181),    
             Color.FromArgb(76,181,143),    
             Color.FromArgb(4,125,123),     
             Color.FromArgb(89,91,97),         //50
             Color.FromArgb(255,255,220),   
             Color.FromArgb(230,101,30),    
             Color.FromArgb(230,150,90),    
             Color.FromArgb(240,156,150),
             Color.FromArgb(167,108,61),
             Color.FromArgb(180,90,48),     
             Color.FromArgb(110,57,55),     
             Color.FromArgb(92,38,37),      
             Color.FromArgb(98,49,189),     
             Color.FromArgb(20,50,156),     //60
             Color.FromArgb(22,95,167),     
             Color.FromArgb(196,227,157),   
             Color.FromArgb(253,51,163),    
             Color.FromArgb(238,113,175),   
             Color.FromArgb(132,49,84),     
             Color.FromArgb(163,145,102),               
             Color.FromArgb(12,137,24),     
             Color.FromArgb(247,242,151),     
             Color.FromArgb(204,153,0),     
             Color.FromArgb(199,151,60),    //70
             Color.FromArgb(255,157,0),                 
             Color.FromArgb(255,186,94),    
             Color.FromArgb(252,241,33),   
             Color.FromArgb(255,71,32),    
             Color.FromArgb(0,181,82),    
             Color.FromArgb(2,87,181),     
             Color.FromArgb(208,186,176), 
             Color.FromArgb(227,190,129),  
             Color.FromArgb(192,192,192)    
             };

		public Embroidery GetEmbroidery(string filename)
		{
				Binary bin=new Binary(filename);
				Embroidery emb=new Embroidery();
				bin.Open();
				int xpos=0;
				int ypos=0;

				//Read data
				bin.SetPosition(0);
                byte[] b = bin.ReadNextbytes(1);
                emb.NumberOfColors = b[0];
                //Read colors
                emb.Colors = new Color[emb.NumberOfColors];
                bin.SetPosition(2);
                for (int a = 0; a < emb.NumberOfColors; a++)
                {
                    b = bin.ReadNextbytes(1);
                    if (b[0] < 78)
                        emb.Colors[a] = ColorTable[b[0]];
                    else
                        emb.Colors[a] = ColorTable[77];
                    bin.ReadNextbytes(1);
                }



                int expstart = 7544;
                emb.StartXOffset = emb.NegativeX;
                emb.StartYOffset = emb.NegativeY;
                bin.SetPosition(expstart);
				long size=bin.Lenght()-expstart-1;
				int xbefore=emb.NegativeX;
				int ybefore=emb.NegativeY;
				stitch s;
				long MaxStitch=0;
				if (MaxStitch==0) 
					MaxStitch=size;

                b = new byte[2];
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

				emb.NegativeX=emb.NegativeX*-1;
				emb.NegativeY=emb.NegativeY*-1;
				emb.Width=emb.NegativeX+emb.PositiveX;
				emb.Height=emb.NegativeY+emb.PositiveY;
                emb.StartYOffset = emb.NegativeY;
                emb.StartXOffset = emb.NegativeX;

				bin.Close();
			return emb;
			}
	#endregion
		}

	}

