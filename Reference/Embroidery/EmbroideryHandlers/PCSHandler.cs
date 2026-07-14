using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace hexreader.EmbroideryHandlers
{
	/// <summary>
	/// Summary description for PCSHandler.
	/// </summary>
	public class PCSHandler:IEmbroideryHandler
	{
		#region PCS
		public Embroidery GetEmbroidery(string filename)
		{
			Embroidery emb=new Embroidery();
			Binary bin = new Binary(filename);
			bin.Open();
            bin.SetPosition(70);

            int xpos = 0;
            int ypos = 0;
            int oldxpos = 0;
            int oldypos = 0;
            byte[] b;
            emb.NumberOfColors = 16;
            emb.Colors = new Color[emb.NumberOfColors];
            for (int a = 0; a < emb.NumberOfColors; a++)
            {
                emb.Colors[a] = Color.Black;
            }

            for (long a = 70; a < bin.Lenght()-9; a = a + 9)
            {
                stitch s = new stitch();
                b = bin.ReadNextbytes(9);
                switch (b[8])
                {
                    case 0:
                    case 2:
                        s.x = GetIntegerFromBytes(GetPart(b, 1, 3)) - xpos;
                        s.y = GetIntegerFromBytes(GetPart(b, 5, 7)) - ypos;
                        xpos += s.x;
                        ypos += s.y;
                        break;
                    case 3:
                        s.type = stitchtype.ColorChange;
                        //ColorChange

                        break;
                }
                emb.Stitches.Add(s);
            }



            bin.Close();
			return emb;
		}

        public byte[] GetPart(byte[] b,int start, int stop)
        {
            byte[] retbyte=new byte[stop-start+1];
            int count=0;
            for (int pos = start; pos <= stop; pos++)
            {
                retbyte[count++] = b[pos];
                
            }
            return retbyte;
        }


        private int GetIntegerFromBytes(byte[] bytes)
        {
            int result = 0;
            for (int i = bytes.Length-1; i >=0; i--)
                result = (result >> 8) | bytes[i];

            return result;
        }
		#endregion
	}
}
