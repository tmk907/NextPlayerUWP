using System;
using Windows.UI;

namespace NextPlayerUWP.AppColors
{
    public class ColorConverter
    {
        public static int Red(int color)
        {
            return (color >> 16) & 0xff;
        }

        public static int Green(int color)
        {
            return (color >> 8) & 0xff;
        }

        public static int Blue(int color)
        {
            return (color) & 0xff;
        }

        public static int Alpha(int color)
        {
            return (color >> 24) & 0xff;
        }

        public static int RGB(int r, int g, int b)
        {
            r = (r << 16) & 0x00FF0000; //Shift red 16-bits and mask out other stuff
            g = (g << 8) & 0x0000FF00; //Shift Green 8-bits and mask out other stuff
            b = b & 0x000000FF; //Mask out anything not blue.

            //0xFF000000 for 100% Alpha. Bitwise OR everything together.
            return (int)(0xFF000000 | (uint)r | (uint)g | (uint)b);
        }

        public static int ARGB(int a, int r, int g, int b)
        {
            return (a & 0xff) << 24 | (r & 0xff) << 16 | (g & 0xff) << 8 | (b & 0xff);
        }

        public static Color ColorFromInt(int color)
        {
            return Color.FromArgb((byte)Alpha(color), (byte)Red(color), (byte)Green(color), (byte)Blue(color));
        }

        public static int ColorToInt(Color color)
        {
            return ARGB(color.A, color.R, color.G, color.B);
        }

        public double GetHue(Color color)
        {
            double hue;

            double r = color.R / 255d;
            double g = color.G / 255d;
            double b = color.B / 255d;

            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));

            if (max == 0)
            {
                hue = 0;
                return hue;
            }

            if (r == max)
            {
                hue = (g - b) / (max - min);
            }
            else if (g == max)
            {
                hue = 2.0 + (b - r) / (max - min);
            }
            else
            {
                hue = 4.0 + (r - g) / (max - min);
            }

            hue *= 60;
            if (hue < 0) hue += 360;
            return hue;
        }

        public Tuple<double, double, double> ColorToHSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            double hue = GetHue(color);

            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;

            Tuple<double, double, double> t = new Tuple<double, double, double>(hue, saturation, value);
            return t;
        }

        public Color HSVToColor(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            byte v2 = (byte)v;
            byte p2 = (byte)p;
            byte q2 = (byte)q;
            byte t2 = (byte)t;

            if (hi == 0)
                return Color.FromArgb(255, v2, t2, p2);
            else if (hi == 1)
                return Color.FromArgb(255, q2, v2, p2);
            else if (hi == 2)
                return Color.FromArgb(255, p2, v2, t2);
            else if (hi == 3)
                return Color.FromArgb(255, p2, q2, v2);
            else if (hi == 4)
                return Color.FromArgb(255, t2, p2, v2);
            else
                return Color.FromArgb(255, v2, p2, q2);
        }

        //hue[0,360], saturation[0,1], lightness[0,1]
        public Tuple<double, double, double> ColorToHSL(Color color)
        {
            double r = color.R / 255d;
            double g = color.G / 255d;
            double b = color.B / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double delta = max - min;

            double hue, saturation, lightness;

            lightness = (max + min) / 2.0;

            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                if (lightness < 0.5)
                {
                    saturation = (max - min) / (max + min);
                }
                else
                {
                    saturation = (max - min) / (2.0 - max - min);
                }

                if (max == r)
                {
                    hue = (g - b) / delta;
                }
                else if (max == g)
                {
                    hue = 2.0 + (b - r) / delta;
                }
                else
                {
                    hue = 4.0 + (r - g) / delta;
                }

                hue *= 60;
                if (hue < 0) hue += 360;
            }

            return new Tuple<double, double, double>(hue, saturation, lightness);
        }

        //hue[0,360], saturation[0,1], lightness[0,1]
        public Color HSLToColor(double hue, double saturation, double lightness)
        {
            double red, green, blue;

            if (saturation == 0)
            {
                red = green = blue = lightness * 255; // achromatic
            }
            else
            {
                double temp1;
                if (lightness < 0.5)
                {
                    temp1 = lightness * (1.0 + saturation);
                }
                else
                {
                    temp1 = lightness + saturation - lightness * saturation;
                }
                double temp2 = 2.0 * lightness - temp1;
                hue = hue / 360.0;

                double tempR, tempG, tempB;
                tempR = hue + 1.0 / 3.0;
                tempG = hue;
                tempB = hue - 1.0 / 3.0;

                red = HSLHelper(temp1, temp2, tempR);
                green = HSLHelper(temp1, temp2, tempG);
                blue = HSLHelper(temp1, temp2, tempB);
            }

            return Color.FromArgb(255, (byte)(int)Math.Round(red * 255), (byte)(int)Math.Round(green * 255), (byte)(int)Math.Round(blue * 255));
        }

        private double HSLHelper(double temp1, double temp2, double tempCol)
        {
            double col;
            if (tempCol > 1) tempCol -= 1.0;
            if (tempCol > 1) tempCol += 1.0;
            if (6.0 * tempCol < 1)
            {
                col = temp2 + (temp2 - temp2) * 6.0 * tempCol;
            }
            else if (2.0 * tempCol < 1)
            {
                col = temp1;
            }
            else if (3.0 * tempCol < 2)
            {
                col = temp2 + (temp1 - temp2) * (2.0 / 3.0 - tempCol) * 6.0;
            }
            else
            {
                col = temp2;
            }
            return col;
        }

        private double HueToRGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        }        
    }
}
