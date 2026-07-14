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
	public class VP3Handler:IEmbroideryHandler
	{

		#region Vp3
		public Embroidery GetEmbroidery(string filename)
		{
			Embroidery emb=new Embroidery();
			Binary bin = new Binary(filename);
            int pos = 0;
			bin.Open();
			byte [] b;
            //Get 
			b = bin.Readbytes(6,2);
			int nb= GetIntegerFromBytes(b);
            b=bin.Readbytes(8, nb);
            String identifier;
            identifier = System.Text.Encoding.BigEndianUnicode.GetString(b, 0, b.Length);

            pos = 8+7 + nb; //don´t mind the unknown string
            b = bin.Readbytes(pos, 2);
            nb = GetIntegerFromBytes(b);
            pos += nb+2;
            //

            //hoop
            emb.PositiveX=GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            emb.PositiveY = GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            emb.NegativeX = GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            emb.NegativeY = GetIntegerFromBytes(bin.Readbytes(pos, 4)) / 100;
            pos += 4;


            
            pos += 16;//Skip unkonw dwords

            //emb.StartXOffset = GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            //emb.StartYOffset = GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            pos += 3; //skip unkwon bytes

            //Centered hoop?
            //emb.PositiveX = GetIntegerFromHex(bin.Readbytes(pos, 4));
            pos += 4;
            //emb.PositiveY = GetIntegerFromHex(bin.Readbytes(pos, 4));
            pos += 4;
            //emb.NegativeX = GetIntegerFromHex(bin.Readbytes(pos, 4));
            pos += 4;
            //emb.NegativeY = GetIntegerFromHex(bin.Readbytes(pos, 4));
            pos += 4;

            emb.Width = GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            emb.Height = GetIntegerFromBytes(bin.Readbytes(pos, 4))/100;
            pos += 4;
            
            pos += 20; //Skip unknowns

            pos += 6; //skip Magic number
            nb = GetIntegerFromBytes(bin.Readbytes(pos, 2));
            pos += nb+2; //skip idetity (same as above)

            emb.NumberOfColors=GetIntegerFromBytes(bin.Readbytes(pos, 2));
            emb.Colors = new Color[emb.NumberOfColors];
           
            emb.StartXOffset = emb.Width - emb.PositiveX;
            emb.StartYOffset = emb.Height - emb.PositiveY;


            pos += 2;
            int colorsection =pos;
            for (int a = 0; a < emb.NumberOfColors; a++)
            {
                pos += 3; //skip 
                int nextcoloroffset = GetIntegerFromBytes(bin.Readbytes(pos, 4));
                pos += 4;
                nextcoloroffset += pos;

                pos += 8; //skipping unknown xy offset
                int ts = GetIntegerFromBytes(bin.Readbytes(pos, 1));
                pos++;
                int blue = GetIntegerFromBytes(bin.Readbytes(pos, 1));
                pos++;
                int green = GetIntegerFromBytes(bin.Readbytes(pos, 1));
                pos++;
                int red = GetIntegerFromBytes(bin.Readbytes(pos, 1));
                pos++;
                pos += 6 * ts;

                emb.Colors[a] = new Color();
                emb.Colors[a] = Color.FromArgb(red,green,blue);

                //Colorstrings
                nb = GetIntegerFromBytes(bin.Readbytes(pos, 2));
                pos += 2;
                b = bin.Readbytes(pos, nb);
                pos += nb;
                string Colorstring1 = System.Text.Encoding.ASCII.GetString(b, 0, b.Length);
                
                nb = GetIntegerFromBytes(bin.Readbytes(pos, 2));
                pos += 2;
                b = bin.Readbytes(pos, nb);
                pos += nb;
                string Colorstring2 = System.Text.Encoding.ASCII.GetString(b, 0, b.Length);

                nb = GetIntegerFromBytes(bin.Readbytes(pos, 2));
                pos += 2;
                b = bin.Readbytes(pos, nb);
                pos += nb;
                string Colorstring3 = System.Text.Encoding.ASCII.GetString(b, 0, b.Length);

                pos += 8; //skip unknown xy offset

                nb = GetIntegerFromBytes(bin.Readbytes(pos, 2));
                pos += 2;
                b = bin.Readbytes(pos, nb);
                pos += nb;
                string Unknown = System.Text.Encoding.ASCII.GetString(b, 0, b.Length);

                int numerofstitchbytes=GetIntegerFromBytes(bin.Readbytes(pos, 4));
                pos += 4;

                pos += 3; //skip unknown bytes

                //read movments
                int bx = 0x00;
                int by = 0x00;
                stitch s=new stitch();
                int stitchcounter = 0;
                while (stitchcounter < (numerofstitchbytes - 3) / 2 && nextcoloroffset >pos)
                {
                    bx = bin.Readbytes(pos, 1)[0];
                    pos++;
                    by = bin.Readbytes(pos, 1)[0];
                    pos++;
                    stitchcounter++;
                    s = new stitch();
                    if (bx == 0x80) //Special stitch
                    {
                        if ((0x00 == by) || (0x03 == by))
                        {
                            // 0x80 0x00 and 0x80 0x03 are skipped
                            stitchcounter++;
                            continue;
                        }
                        else
                        {
                            s.type = stitchtype.Jump;
                            bx = GetIntegerFromBytes(bin.Readbytes(pos, 2)) ;
                            if ((bx & 0x8000) != 0)
                                bx -= 0x10000;
                            pos+=2;
                            by = GetIntegerFromBytes(bin.Readbytes(pos, 2)) ;
                            if ((by & 0x8000) != 0)
                                by -= 0x10000;
                            pos+=2;
                            pos += 2; //skip stop bytes
                            stitchcounter+=2;
                            stitchcounter++;
                            s.x = bx;
                            s.y = by;
                        }
                    }
                    else
                    {
                        s.type = stitchtype.Normal;
                        s.x = bx; 
                        s.y = by; 
                        if (s.x > 128)
                            s.x = s.x - 256;
                        if (s.y > 128)
                            s.y = s.y - 256;
                    }
                    s.threadcolor = emb.Colors[a];
                    emb.Stitches.Add(s);
                }
                s = new stitch();
                s.type = stitchtype.ColorChange;
                emb.Stitches.Add(s);


                if (a != emb.NumberOfColors - 1)
                {
                    pos = nextcoloroffset;
                }

            }
			bin.Close();
			return emb;
		}
		#endregion

		private int GetIntegerFromBytes(byte[] bytes)
		{
			int result = 0;
		    for (int i = 0; i < bytes.Length; i++)
			    result = (result << 8) |  bytes[i];

			return result;
		}


        private int GetIntegerFromDword(byte[] Offset)
        {
            return (Offset[0] << 24) | (Offset[1] << 16) | (Offset[2] << 8) | (Offset[3]);

        }

		}
}
