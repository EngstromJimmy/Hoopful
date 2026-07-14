using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Web;
using System.Net;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace hexreader
{
	public class stitch
	{
		public int x=0;
		public int y=0;
		public Color threadcolor;
		public stitchtype type;
	}
	public enum stitchtype
	{
		Normal=1,Jump=2,ColorChange=3,Stop=4
	}


	/// <summary>
	/// Summary description for Embroidery.
	/// </summary>
	public unsafe class Embroidery
	{	
		public string Name="";
		public long NumberOfStiches=0;
		public int NumberOfColors=0;
		public int Width=0;
		public int Height=0;
		public Color[] Colors;
        public List<stitch> Stitches = new List<stitch>();
		public int PositiveX;
		public int PositiveY;
		public int NegativeX;
		public int NegativeY;
		public bool rotate=false;
		public string Coordinates="";
        private int stride = 0;
        public int StartXOffset = 0;
        public int StartYOffset = 0;
        public int *bytestitch;
        public int* bumpmap;

		public unsafe Image GetImage(Color[] colors,int ThreadWidth,bool threed,bool Bumpmapping,double light,string fabricurl,double xmm,double ymm,int quility)
		{
			int colorNumber=0;
			Pen p = new Pen(colors[colorNumber],ThreadWidth);
			int xbefore=StartXOffset+10;
			int ybefore=StartYOffset+10;

			Bitmap img = new Bitmap(1,1);
			
			//Get Fabric
			if (fabricurl!="")
			{
				Stream ImageStream = new WebClient().OpenRead(fabricurl);
				img = new Bitmap(Image.FromStream(ImageStream));
				Bitmap b2= Antropoid.Drawing.Image.Crop(new Bitmap(img),0,0,300,350);

				double  sizex =0.1/(b2.Size.Width/(xmm))*100;
				double  sizey =0.1/(b2.Size.Height/(ymm))*100;

                b2=new Bitmap(b2,Convert.ToInt32(b2.Size.Width*sizex)+10,Convert.ToInt32( b2.Size.Height*sizey)+10);

				img=Antropoid.Drawing.Image.Tile(b2,NegativeX+PositiveX+10,PositiveY+NegativeY+10);
			}
			else
			{
				//TODO:Change this back
                img = new Bitmap((Width+20) / quility, (Height+20) / quility, PixelFormat.Format32bppRgb);
				//img = new Bitmap(500/quility,500/quility);

			}
                                    

#region old1
            //List<Point> poi = new List<Point>();
            //int counter = 0;
            //foreach (stitch s in Stitches)
            //{
            //    //try
            //    //{
                    
            //        switch (s.type)
            //        {

            //            case stitchtype.Normal:
            //                if (s.x!=0 || s.y!=0)
            //                {
            //                //try
            //                //{
            //                    Point start=new Point(xbefore/quility,ybefore/quility);
            //                    Point middle =new Point((xbefore +(s.x/2))/quility,(ybefore + (s.y/2))/quility);
            //                    Point stop=new Point((xbefore+s.x)/quility,(ybefore+s.y)/quility);

            //                    Color cStart = new Color();
            //                    int red = colors[colorNumber].R;
            //                    int green = colors[colorNumber].G;
            //                    int blue = colors[colorNumber].B;
            //                    red = red - 100;
            //                    green = green - 100;
            //                    blue = blue - 100;
            //                    if (red < 0) red = 0;
            //                    if (green < 0) green = 0;
            //                    if (blue < 0) blue = 0;
            //                    if (red > 255) red = 255;
            //                    if (green > 255) green = 255;
            //                    if (blue > 255) blue = 255;
            //                    cStart = Color.FromArgb(red, green, blue);


            //                    //Middle
            //                    Color cMiddle;
            //                    red = colors[colorNumber].R;
            //                    green = colors[colorNumber].G;
            //                    blue = colors[colorNumber].B;
            //                    if (Bumpmapping == true)
            //                    {
            //                        double dangle;

            //                        double danglerad = (s.y) / Math.Sqrt((s.y * s.y) + (s.x * s.x));
            //                        dangle = Math.Asin(danglerad);
            //                        dangle = dangle * (180 / Math.PI);

            //                        int angle = Convert.ToInt32(dangle);
            //                        angle = Math.Abs(angle);


            //                        double ColorChange = (angle * light) / 4;
            //                        double dprocent;


            //                        if (start.Y > stop.Y)
            //                        {
            //                            Point tmp = start;
            //                            start = stop;
            //                            stop = tmp;
            //                        }

            //                        dprocent = ((0.25 / 90) * angle) + 0.5;
            //                        int procent = Convert.ToInt32(dprocent * 100);
            //                        double movex = s.x * (100 - procent) / 100;
            //                        double movey = s.y * (100 - procent) / 100;
            //                        red = red + (int)(ColorChange);
            //                        green = green + (int)(ColorChange);
            //                        blue = blue + (int)(ColorChange);


            //                        if (red > 255) red = 255;
            //                        if (green > 255) green = 255;
            //                        if (blue > 255) blue = 255;
            //                        cMiddle = Color.FromArgb(red, green, blue);
            //                    }
            //                    else
            //                    {
            //                        cMiddle = Color.FromArgb(red, green, blue);
            //                    }

            //                    poi.Add(start);
            //                    poi.Add(stop);
            //                    counter++;
                                
                                
            //                    /*if(start.X>0 && start.Y>0)
            //                        img.SetPixel(start.X,start.Y,cStart);
            //                    if (middle.X > 0 && middle.Y > 0)
            //                        img.SetPixel(middle.X, middle.Y, cMiddle);
            //                    if (stop.X > 0 && stop.Y > 0)
            //                        img.SetPixel(stop.X, stop.Y, cStart);
            //                    */
            //                /*}
            //                catch (Exception ex)
            //                {
            //                    MessageBox.Show(ex.Message);
            //                    //MaxStitch=size;
            //                }*/

            //                //    Color cStart=new Color();
            //                //    int red=colors[colorNumber-1].R;
            //                //    int green=colors[colorNumber-1].G;
            //                //    int blue=colors[colorNumber-1].B;
            //                //    red=red-100;
            //                //    green=green-100;
            //                //    blue=blue-100;
            //                //    if (red<0)red=0;
            //                //    if(green<0)green=0;
            //                //    if (blue<0)blue=0;
            //                //    if (red>255)red=255;
            //                //    if(green>255)green=255;
            //                //    if (blue>255)blue=255;
            //                //    cStart=Color.FromArgb(red,green,blue);
								

            //                //    //Middle
            //                //    Color cMiddle;
            //                //    red=colors[colorNumber-1].R;
            //                //    green=colors[colorNumber-1].G;
            //                //    blue=colors[colorNumber-1].B;
            //                //    if (Bumpmapping==true)
            //                //    {
            //                //        double dangle;

            //                //        double danglerad=(s.y)/Math.Sqrt((s.y*s.y) + (s.x*s.x));
            //                //        dangle=Math.Asin(danglerad);
            //                //        dangle=dangle * (180/Math.PI);
								
            //                //        int angle =  Convert.ToInt32(dangle);		
            //                //        angle = Math.Abs(angle);


            //                //        double ColorChange=(angle*light)/4;
            //                //        double dprocent;

									
            //                //        if (start.Y>stop.Y)
            //                //        {
            //                //            Point tmp = start;
            //                //            start=stop;
            //                //            stop=tmp;
            //                //        }
									
            //                //        dprocent=((0.25/90)*angle) + 0.5;
            //                //        int procent=Convert.ToInt32(dprocent*100);
            //                //        double movex=s.x * (100-procent)/100;
            //                //        double movey=s.y * (100-procent)/100;
            //                //        red=red + (int)(ColorChange);
            //                //        green=green + (int)(ColorChange);
            //                //        blue=blue + (int)(ColorChange);

									
            //                //        if (red>255)red=255;
            //                //        if(green>255)green=255;
            //                //        if (blue>255)blue=255;
            //                //        cMiddle=Color.FromArgb(red,green,blue);
            //                //    }
            //                //    else
            //                //    {
            //                //        cMiddle=Color.FromArgb(red,green,blue);
            //                //    }

								
            //                //    Color cStop=new Color();
            //                //    cStop=cStart;
            //                //    if (start!=middle && stop!=middle && threed==true)
            //                //    {
            //                //        if (colors[colorNumber-1]==Color.Black)
            //                //        {
            //                //            cStop=Color.FromArgb(0,0,30);
            //                //            cStart=Color.FromArgb(0,0,30);
            //                //            p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(start,middle, cStart,cMiddle),stitchsize+1);
            //                //            g.DrawLine(p,start,middle);
            //                //            p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(middle,stop,cMiddle,cStop),stitchsize+1);
            //                //            g.DrawLine(p,middle,stop);
            //                //        }
            //                //        else
            //                //        {									

            //                //            p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(start,middle, cStart,cMiddle),stitchsize);
            //                //            g.DrawLine(p,start,middle);

            //                //            Point gmiddle=new Point(middle.X,middle.Y);
            //                //            p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(gmiddle,stop,cMiddle,cStop),stitchsize);	
            //                //            g.DrawLine(p,middle,stop);
            //                //        }
									
            //                //    }
            //                //    else
            //                //    {
            //                //        g.DrawLine(new Pen(colors[colorNumber-1],stitchsize),start,stop);
            //                //    }
								
            //                }
            //                xbefore=xbefore+s.x;
            //                ybefore=ybefore+s.y;
            //                //Coordinates+="Stitch" + (char)9 +  s.x + (char)9 + s.y + "\r\n";
            //                break;
            //            case stitchtype.Jump:
            //                if (poi.Count > 0)
            //                {
            //                    p = new Pen(colors[colorNumber] ,stitchsize);	
            //                    g.DrawLines(p, poi.ToArray());
            //                    poi.Clear();
            //                }
            //                xbefore=xbefore+s.x;
            //                ybefore=ybefore+s.y;
            //                //Coordinates+="Jump" + (char)9 +  s.x + (char)9 + s.y + "\r\n";
            //                break;
            //            case stitchtype.ColorChange:
            //                if (poi.Count > 0)
            //                {
            //                    p = new Pen(colors[colorNumber], stitchsize);	
            //                    g.DrawLines(new Pen(colors[colorNumber]), poi.ToArray());
            //                    poi.Clear();

            //                }
            //                if (NumberOfColors>colorNumber+1)
            //                    p.Color=colors[colorNumber++];
            //                NumberOfColors=NumberOfColors+1;
            //                //Coordinates+="ColorChange \r\n";
            //                break;
            //            default:
            //                break;
            //        }
            //    /*}
            //    catch(Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //        //MaxStitch=size;
            //    }*/



            //}
            //if (poi.Count > 0)
            //{
            //    g.DrawLines(new Pen(colors[colorNumber]), poi.ToArray());
            //}
#endregion

            Color[] EndColors = new Color[colors.Length];
            for (int a = 0; a < colors.Length;a++)
            {
                int red =   colors[a].R;
                int green = colors[a].G;
                int blue =  colors[a].B;
                red = red - 100;
                green = green - 100;
                blue = blue - 100;
                if (red < 0) red = 0;
                if (green < 0) green = 0;
                if (blue < 0) blue = 0;
                if (red > 255) red = 255;
                if (green > 255) green = 255;
                if (blue > 255) blue = 255;
                EndColors[a] = Color.FromArgb(red, green, blue);
            }
            
            //CreateImage
            BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            bytestitch = (int*)Scan0.ToPointer();


            //Create Bumpmap
            Bitmap bumpmapimage=null;
            if(Bumpmapping)
            {
                bumpmapimage= new Bitmap(img.Width,img.Height, PixelFormat.Format32bppRgb);
                BitmapData bumpdata = bumpmapimage.LockBits(new Rectangle(0, 0, bumpmapimage.Width, bumpmapimage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                System.IntPtr bumpScan0 = bumpdata.Scan0;
                bumpmap = (int*)bumpScan0.ToPointer();
            }

            //Make background White
            int* whitener = (int*)Scan0.ToPointer();
            for (int a = 0; a < ((img.Width)) * img.Height; a++)
                *(whitener++) = 16777215;


            Point start;
            Point stop;
                foreach (stitch s in Stitches)
                {
                    switch (s.type)
                    {
                        case stitchtype.Normal:
                            if (s.x != 0 || s.y != 0)
                            {
                                start = new Point(xbefore/quility, ybefore/quility);
                                stop = new Point((xbefore + s.x) / quility, (ybefore + s.y) / quility);
                                if (start.Y >= 0 && start.X >= 0 && stop.Y >= 0 && stop.X >= 0)
                                {
                                    DrawLine(start, stop, colors[colorNumber], EndColors[colorNumber], ThreadWidth,Bumpmapping);
                                }
                                xbefore = xbefore + s.x;
                                ybefore = ybefore + s.y;
                            }
                            break;
                        case stitchtype.Jump:
                            start = new Point(xbefore / quility, ybefore / quility);
                            stop = new Point((xbefore + s.x) / quility, (ybefore + s.y) / quility);
                            if (start.Y >= 0 && start.X >= 0 && stop.Y >= 0 && stop.X >= 0)
                            {
                                DrawLine(start, stop, colors[colorNumber], EndColors[colorNumber], ThreadWidth, Bumpmapping);
                            }
                            xbefore = xbefore + s.x;
                            ybefore = ybefore + s.y;
                            break;
                        case stitchtype.ColorChange:
                            colorNumber++;
                            xbefore = xbefore + s.x;
                            ybefore = ybefore + s.y;
                            break;
                    }
                }
                //System.Runtime.InteropServices.Marshal.Copy(bytestitch, 0, bmData.Scan0, bytestitch.Length);
                img.UnlockBits(bmData);
            if(Bumpmapping)
            {    
                bumpmapimage.UnlockBits(bmData);
                bumpmapimage.Save(@"c:\bumpmap.jpg",ImageFormat.Jpeg);
                img.Save(@"c:\Image.jpg", ImageFormat.Jpeg);
            }
			if (rotate==false)
				img.RotateFlip(RotateFlipType.Rotate180FlipX);
			else
				img.RotateFlip(RotateFlipType.Rotate270FlipX);
			return img;
		}


        private int CalculatePixel(int x, int y)
        {
            return ((y * (stride/4)) + (x));
        }

        private void DrawLine(Point start,Point stop,Color color,Color EndColor,int Pensize,bool Bumpmapping)
        {
            try
            {
                int pixel = 0;
                int xpos = start.X;

                int ymovement = Math.Abs(start.Y - stop.Y);
                int xmovement = Math.Abs(start.X - stop.X);
                //if (Bumpmapping)
                //{
                //    double dangle;
                //    double danglerad = (ymovement) / Math.Sqrt((ymovement * ymovement) + (xmovement * xmovement));
                //    dangle = Math.Asin(danglerad);
                //    dangle = dangle * (180 / Math.PI);

                //    int angle = Convert.ToInt32(dangle);
                //    angle = Math.Abs(angle);


                //    //double ColorChange = (angle * light) / 4;
                //    //double dprocent;

                //    //dprocent = ((0.25 / 90) * angle) + 0.5;
                //    //int procent = Convert.ToInt32(dprocent * 100);


                //    // double movex = s.x * (100 - procent) / 100;
                //   // double movey = s.y * (100 - procent) / 100;

                //}



                int a, b, c, d;
                a = start.X;
                b = start.Y;
                c = stop.X;
                d = stop.Y;
                int s, n, m, d1x, d1y, d2x, d2y, i;
                int u, v;

                d2x = (d1x = Math.Sign(u = c - a));     /* length and direction */
                d1y = Math.Sign(v = d - b);
                d2y = 0;
                m = Math.Abs(u);
                n = Math.Abs(v);
                if (n > m)
                {
                    d2x = 0;
                    d2y = d1y;
                    m = n;
                    n = Math.Abs(u);
                }
                s = m >> 1;     /* Integer divide by 2 - FAST! */
                List<List<int>> linepixels = new List<List<int>>();
                for (i = m; i-- != 0; )
                {
                    List<int> pix = new List<int>();
                    for (int penstroke = 0; penstroke < Pensize; penstroke++)
                    {
                        if (ymovement == 0 && xmovement > 0)
                            pixel = CalculatePixel(a, b + penstroke);
                        else if (ymovement > xmovement)
                            pixel = CalculatePixel(a + penstroke, b);
                        else if (ymovement < xmovement)
                            pixel = CalculatePixel(a, b + penstroke);
                        else if (ymovement == xmovement)
                            pixel = CalculatePixel(a + penstroke, b);
                        else
                            pixel = CalculatePixel(a + penstroke, b);
                        pix.Add(pixel);

                    }
                    linepixels.Add(pix);
                    s += n;
                    if (m < s)
                    {
                        s -= m;
                        a += d1x;
                        b += d1y;
                    }
                    else
                    {
                        a += d2x;
                        b += d2y;
                    }
                }

                int cr;
                int cg;
                int cb;

                for (int linepixel = 0; linepixel < linepixels.Count / 2; linepixel++)
                {
                    //Calculate Gradient
                    //cr = EndColor.R + linepixel * (color.R - EndColor.R) / (linepixels.Count / 2);
                    //cg = EndColor.G + linepixel * (color.G - EndColor.G) / (linepixels.Count / 2);
                    //cb = EndColor.B + linepixel * (color.B - EndColor.B) / (linepixels.Count / 2);

                    for (int pen = 0; pen < Pensize; pen++)
                    {
                        //USE SOLID COLOR
                        *(bytestitch + linepixels[linepixel][pen]) = (color.R << 16) + (color.G << 8) + color.B;
                        *(bytestitch + linepixels[(linepixels.Count - linepixel) - 1][pen]) = (color.R << 16) + (color.G << 8) + color.B;

                        //USE GRADIENT
                        //*(bytestitch + linepixels[linepixel][pen]) = (cr << 16) + (cg << 8) + cb;
                        //*(bytestitch + linepixels[(linepixels.Count - linepixel) - 1][pen]) = (cr << 16) + (cg << 8) + cb;
                        if (Bumpmapping)
                        {     //This setting is normally used when dealing with 6 pixel stitch
                            cr = Color.FromArgb(181, 181, 181).R + linepixel * (Color.FromArgb(136, 136, 136).R - Color.FromArgb(181, 181, 181).R) / (linepixels.Count / 2);
                            cg = Color.FromArgb(181, 181, 181).G + linepixel * (Color.FromArgb(136, 136, 136).G - Color.FromArgb(136, 136, 136).G) / (linepixels.Count / 2);
                            cb = Color.FromArgb(181, 181, 181).B + linepixel * (Color.FromArgb(136, 136, 136).B - Color.FromArgb(136, 136, 136).B) / (linepixels.Count / 2);

                            //if ((pen == 3) || (pen == 4))
                            //{
                            *(bumpmap + linepixels[linepixel][pen]) = (cr << 16) + (cg << 8) + cb;
                            *(bumpmap + linepixels[(linepixels.Count - linepixel) - 1][pen]) = (cr << 16) + (cg << 8) + cb;



                            //*(bumpmap + linepixels[linepixel][pen]) = (cr << 16) + (cg << 8) + cb;
                            //*(bumpmap + linepixels[(linepixels.Count - linepixel) - 1][pen]) = (cr << 16) + (cg << 8) + cb;

                            //if ((linepixels.Count & 0x01) > 0)
                            //{ //not an even number
                            //*(bumpmap + linepixels[1][pen]) = Color.FromArgb(52, 52, 52).ToArgb();
                            //*(bumpmap + linepixels[2][pen]) = Color.FromArgb(52, 52, 52).ToArgb();
                            //*(bumpmap + linepixels[(linepixels.Count) - 1][pen]) = Color.FromArgb(95, 95, 95).ToArgb();
                            //*(bumpmap + linepixels[(linepixels.Count) - 2][pen]) = Color.FromArgb(95, 95, 95).ToArgb();
                            //*(bumpmap + linepixels[(linepixels.Count) - 1][pen]) = Color.FromArgb(136, 136, 136).ToArgb();
                            //*(bumpmap + linepixels[(linepixels.Count) - 2][pen]) = Color.FromArgb(136, 136, 136).ToArgb();
                                //}
                            //}
                        }

                    }
                }
            }
            catch { }
        }

        public Image GetImageByGradient(Color[] colors, float stitchsize, bool threed, bool Bumpmapping, double light, string fabricurl, double xmm, double ymm, int quility)
        {
            int colorNumber = 0;
            Pen p = new Pen(colors[colorNumber++], stitchsize);
            int xbefore = StartXOffset;
            int ybefore = StartYOffset;
            //int xbefore = Math.Abs(NegativeX);
            //int ybefore = Math.Abs(NegativeY);
            Bitmap img = new Bitmap(1, 1);

            //Get Fabric
            if (fabricurl == "apa")
            {
                Stream ImageStream = new WebClient().OpenRead(fabricurl);
                img = new Bitmap(Image.FromStream(ImageStream));
                Bitmap b2 = Antropoid.Drawing.Image.Crop(new Bitmap(img), 0, 0, 300, 350);



                double sizex = 0.1 / (b2.Size.Width / (xmm)) * 100;
                double sizey = 0.1 / (b2.Size.Height / (ymm)) * 100;


                b2 = new Bitmap(b2, Convert.ToInt32(b2.Size.Width * sizex), Convert.ToInt32(b2.Size.Height * sizey));

                img = Antropoid.Drawing.Image.Tile(b2, NegativeX + PositiveX, PositiveY + NegativeY);
            }
            else
            {
                //TODO:Change this back
                img = new Bitmap((Width) / quility, (Height) / quility);
                //img = new Bitmap(500/quility,500/quility);

            }


            Graphics g = Graphics.FromImage(img);
            GraphicsPath path = new GraphicsPath();
            int counter=0;
            foreach (stitch s in Stitches)
            {
                if (counter > Convert.ToInt32(fabricurl))
                    return img;

                counter++;
                try
                {

                    switch (s.type)
                    {

                        case stitchtype.Normal:
                            if (s.x != 0 || s.y != 0)
                            {
                                Point start = new Point(xbefore / quility, ybefore / quility);
                                Point middle = new Point((xbefore + (s.x / 2)) / quility, (ybefore + (s.y / 2)) / quility);
                                Point stop = new Point((xbefore + s.x) / quility, (ybefore + s.y) / quility);

                                Color cStart = new Color();
                                int red = colors[colorNumber - 1].R;
                                int green = colors[colorNumber - 1].G;
                                int blue = colors[colorNumber - 1].B;
                                red = red - 100;
                                green = green - 100;
                                blue = blue - 100;
                                if (red < 0) red = 0;
                                if (green < 0) green = 0;
                                if (blue < 0) blue = 0;
                                if (red > 255) red = 255;
                                if (green > 255) green = 255;
                                if (blue > 255) blue = 255;
                                cStart = Color.FromArgb(red, green, blue);


                                //Middle
                                Color cMiddle;
                                red = colors[colorNumber - 1].R;
                                green = colors[colorNumber - 1].G;
                                blue = colors[colorNumber - 1].B;
                                if (Bumpmapping == true)
                                {
                                    double dangle;
                                    double danglerad = (s.y) / Math.Sqrt((s.y * s.y) + (s.x * s.x));
                                    dangle = Math.Asin(danglerad);
                                    dangle = dangle * (180 / Math.PI);

                                    int angle = Convert.ToInt32(dangle);
                                    angle = Math.Abs(angle);


                                    double ColorChange = (angle * light) / 4;
                                    double dprocent;


                                    if (start.Y > stop.Y)
                                    {
                                        Point tmp = start;
                                        start = stop;
                                        stop = tmp;
                                    }

                                    dprocent = ((0.25 / 90) * angle) + 0.5;
                                    int procent = Convert.ToInt32(dprocent * 100);
                                    double movex = s.x * (100 - procent) / 100;
                                    double movey = s.y * (100 - procent) / 100;
                                    red = red + (int)(ColorChange);
                                    green = green + (int)(ColorChange);
                                    blue = blue + (int)(ColorChange);


                                    if (red > 255) red = 255;
                                    if (green > 255) green = 255;
                                    if (blue > 255) blue = 255;
                                    cMiddle = Color.FromArgb(red, green, blue);
                                }
                                else
                                {
                                    cMiddle = Color.FromArgb(red, green, blue);
                                }


                                Color cStop = new Color();
                                cStop = cStart;
                                if (start != middle && stop != middle && threed == true)
                                {
                                    if (colors[colorNumber - 1] == Color.Black)
                                    {
                                        cStop = Color.FromArgb(0, 0, 30);
                                        cStart = Color.FromArgb(0, 0, 30);
                                        p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(start, middle, cStart, cMiddle), stitchsize + 1);
                                        g.DrawLine(p, start, middle);
                                        p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(middle, stop, cMiddle, cStop), stitchsize + 1);
                                        g.DrawLine(p, middle, stop);
                                    }
                                    else
                                    {

                                        p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(start, middle, cStart, cMiddle), stitchsize);
                                        g.DrawLine(p, start, middle);

                                        Point gmiddle = new Point(middle.X, middle.Y);
                                        p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(gmiddle, stop, cMiddle, cStop), stitchsize);
                                        g.DrawLine(p, middle, stop);
                                    }

                                }
                                else
                                {
                                    g.DrawLine(new Pen(colors[colorNumber - 1], stitchsize), start, stop);
                                }

                            }
                            xbefore = xbefore + s.x;
                            ybefore = ybefore + s.y;
                            Coordinates+="Stitch" + (char)9 +  s.x + (char)9 + s.y + "\r\n";
                            break;
                        case stitchtype.Jump:
                            xbefore = xbefore + s.x;
                            ybefore = ybefore + s.y;
                            Coordinates+="Jump" + (char)9 +  s.x + (char)9 + s.y + "\r\n";
                            break;
                        case stitchtype.ColorChange:
                            if (NumberOfColors > colorNumber + 1)
                                p.Color = colors[colorNumber++];
                            NumberOfColors = NumberOfColors + 1;
                            xbefore = xbefore + s.x;
                            ybefore = ybefore + s.y;
                            Coordinates+="ColorChange \r\n";
                            break;
                        default:
                            break;
                    }
                    //Console.WriteLine(xbefore + "," + ybefore);
                }
                catch
                {
                    //MaxStitch=size;
                }

            }
            //bin.Close();
            if (rotate == false)
                img.RotateFlip(RotateFlipType.Rotate180FlipX);
            else
                img.RotateFlip(RotateFlipType.Rotate270FlipX);
            return img;

        }
	}
}
