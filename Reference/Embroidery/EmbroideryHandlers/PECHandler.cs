using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for PesHandler.
	/// </summary>
	public class PECHandler:IEmbroideryHandler
	{
		
#region PES
		public Embroidery GetEmbroidery(string filename)
		{
			
				Embroidery emb=new Embroidery();
				Binary bin = new Binary(filename);
				bin.Open();

                emb.Height = 1000;
                emb.Width = 4000;

                //Todo:Add colors supprt
                emb.NumberOfColors = 10;
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
			
				stitch s;
			
				s= new stitch();
                //byte[] bx=bin.Readbytes(110,2);
                //byte[] by=bin.Readbytes(112,2);
			
                //s.type=stitchtype.Jump;
                //emb.Stitches.Add(s);

                ////Read data
			
                ////Pecstart
                //byte[] bps=bin.Readbytes(8,4);
                //int pecstart =bps[0] | (bps[1] << 8) | (bps[2] << 16) | (bps[3] << 32);

                //bin.SetPosition(pecstart+520);
			
                //byte[] bPositiveX=bin.ReadNextbytes(2);
                //emb.PositiveX=(int) bPositiveX[0] | ((int) bPositiveX[1] << 8);
			
                //byte[] bPositiveY=bin.ReadNextbytes(2);
                //emb.PositiveY=(int) bPositiveY[0] | ((int) bPositiveY[1] << 8);
			
                //emb.Width=emb.PositiveX;
                //emb.Height=emb.PositiveY;

                //bin.SetPosition(pecstart+529);
                long size = bin.Lenght() - 1 - 0x214;

                emb.StartXOffset = emb.NegativeX;
                emb.StartYOffset = emb.NegativeY;
				long MaxStitch=0;
				if (MaxStitch==0) 
					MaxStitch=size;

                bin.SetPosition(0x214);
				byte[] b=new byte[2];
				for (int stitchcount=0;stitchcount<=size;stitchcount++)
				{
					try
					{
						b =  bin.ReadNextbytes(2);
	
						int y=0;
						int x=0;
				


						if (b.Length==2)
						{
							//Colorchange
                            if ((b[0] == 254) && (b[1] == 176))
                            {
                                s = new stitch();
                                s.type = stitchtype.ColorChange;
                                emb.Stitches.Add(s);
                                emb.NumberOfColors++;
                                bin.ReadNextbytes(1);
                                bin.ReadNextbytes(2);
                                stitchcount++;
                                stitchcount++;
                                stitchcount++;
                            }
                            else if ((b[1] == 0) && (b[0] == 255))
                            {
                                stitchcount = (int)size;
                            }
                            else
                            {
                                s = new stitch();
                                if (b[0] < 128)
                                {		// normaler Stich in X
                                    if (b[0] < 64)
                                        s.x = b[0];
                                    else
                                        s.x = (b[0] - 128);
                                    bin.SetPosition(bin.BytePosition - 1);
                                    s.type = stitchtype.Normal;
                                    stitchcount++;
                                }
                                else
                                {					// Positionier-Stich in X
                                    b[0] &= 0x0F;
                                    if (b[0] < 8)
                                        s.x = b[1] + (b[0] * 256);
                                    else
                                        s.x = (b[1] - 256) + ((b[0] - 15) * 256);
                                    s.type = stitchtype.Jump;
                                    stitchcount++;
                                    stitchcount++;
                                }

                                b = bin.ReadNextbytes(2);

                                //y
                                if (b[0] < 128)
                                {		// normaler Stich in X
                                    if (b[0] < 64)
                                        s.y = b[0]*-1;
                                    else
                                        s.y = (128-b[0] );
                                    bin.SetPosition(bin.BytePosition - 1);
                                    s.type = stitchtype.Normal;
                                    stitchcount++;
                                }
                                
                                else
                                {					// Positionier-Stich in X
                                    b[0] &= 0x0F;
                                    if (b[0] <8)
                                        s.y = (b[1] + (b[0] * 256))*-1;
                                    else
                                        s.y = (b[1] - 256) + ((b[0] - 15) * 256);
                                    s.type = stitchtype.Jump;
                                    stitchcount++;
                                    stitchcount++;
                                }
                                emb.Stitches.Add(s);
                            
                            }
                            
                            
                            
                            
                            
                            //else if (b[0] > 128 && b[0] <= 254)
                            //{
                            //    short val_1 = (short)b[0];
                            //    short val_2 = (short)b[1];

                            //    b=bin.ReadNextbytes(2);
                            //    short val_3=(short)b[0];
                            //    short val_4=(short)b[1];
                            //    s=new stitch();
                            //    s.type=stitchtype.Jump;
							
                            //    //X
                            //    val_1 &= 0x0F;
                            //    if (val_1 < 8)
                            //        s.x  = val_2 + (val_1 * 256);
                            //    else
                            //        s.x  = (val_2 - 256) + ((val_1 - 15)*256);
							
                            //    //Y
                            //    val_3&= 0x0F;
                            //    if (val_3 < 8)
                            //        s.y  = val_4 + (val_3 * 256);
                            //    else
                            //        s.y  = (val_4 - 256) + ((val_3 - 15)*256);


                            //    s.x=0;
                            //    s.y=0;
                            //    emb.Stitches.Add (s);
						
                            //}


						

                            ////Normal
                            //     //((b[0]<128)&&(b[1]<128))
                            //else
                            //{
                            //    x=b[0];
                            //    y=b[1];
                            //    if (x>63)
                            //    {
                            //        x=x-128;
                            //    }
                            //    if (y>63)
                            //    {
                            //        y=y-128;
                            //    }
						
                            //    if (x!=0 || y!=0)
                            //    {	
                            //        s=new stitch();
                            //        s.x=x;
                            //        s.y=y;
                            //        s.type=stitchtype.Normal;
                            //        emb.Stitches.Add (s);
                            //        emb.NumberOfStiches++;
                            //    }


                            //}

							
							//Jump
							//if (128 <= b[0] && b[0]<= 254)
							
						
							
						}
						else
						{
							stitchcount=(int)size;
						}

					}
					catch
					{
						MaxStitch=size;
					}

				}
                emb.NegativeX = 580;
                emb.NegativeY = 400;
				emb.StartXOffset = emb.NegativeX;
                emb.StartYOffset = emb.NegativeY;
                emb.NegativeX = emb.NegativeX * -1;
                emb.NegativeY = emb.NegativeY * -1;
				bin.Close();
			return emb;
			}
		
#endregion
		
	}
}
