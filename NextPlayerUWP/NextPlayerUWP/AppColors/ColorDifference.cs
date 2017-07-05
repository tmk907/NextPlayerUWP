using Colourful;
using Colourful.Conversion;
using Colourful.Difference;
using System.Collections.Generic;
using Windows.UI;

namespace NextPlayerUWP.AppColors
{
    public class ColorDifference
    {
        public Color GetClosestColor(Color current)
        {
            AppAccentColors aac = new AppAccentColors();
            Color closest;
            var colors = aac.GetWin10AccentColors();
            double min = 10000;
            foreach (var color in colors)
            {
                var d = CalculateDifference94(color.R, color.G, color.B, current.R, current.G, current.B);
                if (d < min)
                {
                    min = d;
                    closest = color;
                }
            }
            return closest;
        }

        public Color GetClosestColor(Color current, IEnumerable<Color> colors)
        {
            AppAccentColors aac = new AppAccentColors();
            Color closest;
            double min = 10000;
            foreach (var color in colors)
            {
                var d = CalculateDifference94(color.R, color.G, color.B, current.R, current.G, current.B);
                if (d < min)
                {
                    min = d;
                    closest = color;
                }
            }
            return closest;
        }

        private double CalculateDifference94(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            RGBColor rgb1 = RGBColor.FromRGB8bit((byte)r1, (byte)g1, (byte)b1);
            RGBColor rgb2 = RGBColor.FromRGB8bit((byte)r2, (byte)g2, (byte)b2);


            ColourfulConverter converter = new ColourfulConverter();

            LabColor lab1 = converter.ToLab(rgb1);
            LabColor lab2 = converter.ToLab(rgb2);

            CIE94ColorDifference diff = new CIE94ColorDifference();
            var dif = diff.ComputeDifference(lab1, lab2);
            return dif;
        }

        private double CalculateDifference2000(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            RGBColor rgb1 = RGBColor.FromRGB8bit((byte)r1, (byte)g1, (byte)b1);
            RGBColor rgb2 = RGBColor.FromRGB8bit((byte)r2, (byte)g2, (byte)b2);

            ColourfulConverter converter = new ColourfulConverter();

            LabColor lab1 = converter.ToLab(rgb1);
            LabColor lab2 = converter.ToLab(rgb2);

            CIEDE2000ColorDifference diff = new CIEDE2000ColorDifference();
            var dif = diff.ComputeDifference(lab1, lab2);
            return dif;
        }     
    }
}
