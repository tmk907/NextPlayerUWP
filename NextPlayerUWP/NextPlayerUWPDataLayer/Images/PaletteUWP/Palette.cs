using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace NextPlayerUWPDataLayer.Images.PaletteUWP
{
    public class Palette
    {
        static readonly int DEFAULT_RESIZE_BITMAP_AREA = 112 * 112;
        static readonly int DEFAULT_CALCULATE_NUMBER_COLORS = 16;

        static readonly float MIN_CONTRAST_TITLE_TEXT = 3.0f;
        static readonly float MIN_CONTRAST_BODY_TEXT = 4.5f;

        public static Builder From(WriteableBitmap bitmap)
        {
            return new Builder(bitmap);
        }

        public static Builder2 From(BitmapDecoder decoder)
        {
            return new Builder2(decoder);
        }

        private readonly List<Swatch> mSwatches;
        private readonly List<Target> mTargets;

        private readonly Dictionary<Target, Swatch> mSelectedSwatches;
        private readonly Dictionary<int,bool> mUsedColors;

        private readonly Swatch mDominantSwatch;

        public Palette(List<Swatch> swatches, List<Target> targets)
        {
            mSwatches = swatches;
            mTargets = targets;

            mUsedColors = new Dictionary<int, bool>();
            mSelectedSwatches = new Dictionary<Target, Swatch>();

            mDominantSwatch = FindDominantSwatch();
        }

        #region Getters
        /**
         * Returns all of the swatches which make up the palette.
         */
        //@NonNull
        public IReadOnlyCollection<Swatch> GetSwatches()
        {
            return mSwatches.AsReadOnly();
        }

        /**
         * Returns the targets used to generate this palette.
         */
        //@NonNull
        public IReadOnlyCollection<Target> GetTargets()
        {
            return mTargets.AsReadOnly();
        }

        /**
         * Returns the most vibrant swatch in the palette. Might be null.
         *
         * @see Target#VIBRANT
         */
        //@Nullable
        public Swatch GetVibrantSwatch()
        {
            return GetSwatchForTarget(Target.VIBRANT);
        }

        /**
         * Returns a light and vibrant swatch from the palette. Might be null.
         *
         * @see Target#LIGHT_VIBRANT
         */
        //@Nullable
        public Swatch GetLightVibrantSwatch()
        {
            return GetSwatchForTarget(Target.LIGHT_VIBRANT);
        }

        /**
         * Returns a dark and vibrant swatch from the palette. Might be null.
         *
         * @see Target#DARK_VIBRANT
         */
        //@Nullable
        public Swatch GetDarkVibrantSwatch()
        {
            return GetSwatchForTarget(Target.DARK_VIBRANT);
        }

        /**
         * Returns a muted swatch from the palette. Might be null.
         *
         * @see Target#MUTED
         */
        //@Nullable
        public Swatch GetMutedSwatch()
        {
            return GetSwatchForTarget(Target.MUTED);
        }

        /**
         * Returns a muted and light swatch from the palette. Might be null.
         *
         * @see Target#LIGHT_MUTED
         */
        //@Nullable
        public Swatch GetLightMutedSwatch()
        {
            return GetSwatchForTarget(Target.LIGHT_MUTED);
        }

        /**
         * Returns a muted and dark swatch from the palette. Might be null.
         *
         * @see Target#DARK_MUTED
         */
        //@Nullable
        public Swatch GetDarkMutedSwatch()
        {
            return GetSwatchForTarget(Target.DARK_MUTED);
        }

        /**
         * Returns the most vibrant color in the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getVibrantSwatch()
         */
        //@ColorInt
        public int GetVibrantColor(int defaultColor)
        {
            return GetColorForTarget(Target.VIBRANT, defaultColor);
        }

        /**
         * Returns a light and vibrant color from the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getLightVibrantSwatch()
         */
        //@ColorInt
        public int GetLightVibrantColor(int defaultColor)
        {
            return GetColorForTarget(Target.LIGHT_VIBRANT, defaultColor);
        }

        /**
         * Returns a dark and vibrant color from the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getDarkVibrantSwatch()
         */
        //@ColorInt
        public int GetDarkVibrantColor(int defaultColor)
        {
            return GetColorForTarget(Target.DARK_VIBRANT, defaultColor);
        }

        /**
         * Returns a muted color from the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getMutedSwatch()
         */
        //@ColorInt
        public int GetMutedColor(int defaultColor)
        {
            return GetColorForTarget(Target.MUTED, defaultColor);
        }

        /**
         * Returns a muted and light color from the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getLightMutedSwatch()
         */
        //@ColorInt
        public int GetLightMutedColor(int defaultColor)
        {
            return GetColorForTarget(Target.LIGHT_MUTED, defaultColor);
        }

        /**
         * Returns a muted and dark color from the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getDarkMutedSwatch()
         */
        //@ColorInt
        public int GetDarkMutedColor(int defaultColor)
        {
            return GetColorForTarget(Target.DARK_MUTED, defaultColor);
        }

        /**
         * Returns the selected swatch for the given target from the palette, or {@code null} if one
         * could not be found.
         */
        //@Nullable
        public Swatch GetSwatchForTarget(Target target)
        {
            return mSelectedSwatches[target];
        }

        /**
         * Returns the selected color for the given target from the palette as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         */
        //@ColorInt
        public int GetColorForTarget(Target target, int defaultColor)
        {
            Swatch swatch = GetSwatchForTarget(target);
            return swatch != null ? swatch.GetRgb() : defaultColor;
        }

        /**
         * Returns the dominant swatch from the palette.
         *
         * <p>The dominant swatch is defined as the swatch with the greatest population (frequency)
         * within the palette.</p>
         */
        //@Nullable
        public Swatch GetDominantSwatch()
        {
            return mDominantSwatch;
        }

        /**
         * Returns the color of the dominant swatch from the palette, as an RGB packed int.
         *
         * @param defaultColor value to return if the swatch isn't available
         * @see #getDominantSwatch()
         */
        //@ColorInt
        public int GetDominantColor(int defaultColor)
        {
            return mDominantSwatch != null ? mDominantSwatch.GetRgb() : defaultColor;
        }
        #endregion

        void Generate()
        {
            // We need to make sure that the scored targets are generated first. This is so that
            // inherited targets have something to inherit from
            for (int i = 0, count = mTargets.Count; i < count; i++)
            {
                Target target = mTargets[i];
                target.NormalizeWeights();
                mSelectedSwatches.Add(target, GenerateScoredTarget(target));
            }
            // We now clear out the used colors
            mUsedColors.Clear();
        }

        private Swatch GenerateScoredTarget(Target target)
        {
            Swatch maxScoreSwatch = GetMaxScoredSwatchForTarget(target);
            if (maxScoreSwatch != null && target.IsExclusive())
            {
                // If we have a swatch, and the target is exclusive, add the color to the used list
                mUsedColors.Add(maxScoreSwatch.GetRgb(), true);
            }
            return maxScoreSwatch;
        }

        private Swatch GetMaxScoredSwatchForTarget(Target target)
        {
            float maxScore = 0;
            Swatch maxScoreSwatch = null;
            for (int i = 0, count = mSwatches.Count; i < count; i++)
            {
                Swatch swatch = mSwatches[i];
                if (ShouldBeScoredForTarget(swatch, target))
                {
                    float score = GenerateScore(swatch, target);
                    if (maxScoreSwatch == null || score > maxScore)
                    {
                        maxScoreSwatch = swatch;
                        maxScore = score;
                    }
                }
            }
            return maxScoreSwatch;
        }

        private bool ShouldBeScoredForTarget(Swatch swatch, Target target)
        {
            // Check whether the HSL values are within the correct ranges, and this color hasn't
            // been used yet.
            float[] hsl = swatch.GetHsl();
            bool a = false;
            mUsedColors.TryGetValue(swatch.GetRgb(), out a);
            return hsl[1] >= target.GetMinimumSaturation() && hsl[1] <= target.GetMaximumSaturation()
                    && hsl[2] >= target.GetMinimumLightness() && hsl[2] <= target.GetMaximumLightness()
                    && !a;
        }

        private float GenerateScore(Swatch swatch, Target target)
        {
            float[] hsl = swatch.GetHsl();

            float saturationScore = 0;
            float luminanceScore = 0;
            float populationScore = 0;

            int maxPopulation = mDominantSwatch != null ? mDominantSwatch.GetPopulation() : 1;

            if (target.GetSaturationWeight() > 0)
            {
                saturationScore = target.GetSaturationWeight()
                        * (1f - Math.Abs(hsl[1] - target.GetTargetSaturation()));
            }
            if (target.GetLightnessWeight() > 0)
            {
                luminanceScore = target.GetLightnessWeight()
                        * (1f - Math.Abs(hsl[2] - target.GetTargetLightness()));
            }
            if (target.GetPopulationWeight() > 0)
            {
                populationScore = target.GetPopulationWeight()
                        * (swatch.GetPopulation() / (float)maxPopulation);
            }

            return saturationScore + luminanceScore + populationScore;
        }

        private Swatch FindDominantSwatch()
        {
            int maxPop = Int32.MinValue;
            Swatch maxSwatch = null;
            for (int i = 0, count = mSwatches.Count; i < count; i++)
            {
                Swatch swatch = mSwatches[i];
                if (swatch.GetPopulation() > maxPop)
                {
                    maxSwatch = swatch;
                    maxPop = swatch.GetPopulation();
                }
            }
            return maxSwatch;
        }

        /// <summary>
        /// Represents a color swatch generated from an image's palette.
        /// </summary>
        public sealed class Swatch
        {
            private readonly int mRed, mGreen, mBlue;
            private readonly int mRgb;
            private readonly int mPopulation;

            private bool mGeneratedTextColors;
            private int mTitleTextColor;
            private int mBodyTextColor;

            private float[] mHsl;

            public Swatch(int color, int population)
            {
                mRed = ColorHelpers.Red(color);
                mGreen = ColorHelpers.Green(color);
                mBlue = ColorHelpers.Blue(color);
                mRgb = color;
                mPopulation = population;
            }

            public Swatch(float[] hsl, int population) : this(ColorHelpers.HSLToColor(hsl), population)
            {
                mHsl = hsl;
            }

            public Swatch(int red, int green, int blue, int population)
            {
                mRed = red;
                mGreen = green;
                mBlue = blue;
                mRgb = ColorHelpers.RGB(red, green, blue);
                mPopulation = population;
            }

            public int GetRgb()
            {
                return mRgb;
            }

            /// <summary>
            /// Return this swatch's HSL values.
            /// hsv[0] is Hue[0.. 360)
            /// hsv[1] is Saturation[0...1]
            /// hsv[2] is Lightness[0...1]
            /// </summary>
            /// <returns></returns>
            public float[] GetHsl()
            {
                if (mHsl == null)
                {
                    mHsl = new float[3];
                }
                ColorHelpers.RGBToHSL(mRed, mGreen, mBlue, mHsl);
                return mHsl;
            }

            /// <summary>
            /// return the number of pixels represented by this swatch
            /// </summary>
            /// <returns></returns>
            public int GetPopulation()
            {
                return mPopulation;
            }

            /**
             * Returns an appropriate color to use for any 'title' text which is displayed over this
             * {@link Swatch}'s color. This color is guaranteed to have sufficient contrast.
             */
            //@ColorInt
            public int getTitleTextColor()
            {
                ensureTextColorsGenerated();
                return mTitleTextColor;
            }

            /**
             * Returns an appropriate color to use for any 'body' text which is displayed over this
             * {@link Swatch}'s color. This color is guaranteed to have sufficient contrast.
             */
            //@ColorInt
            public int getBodyTextColor()
            {
                ensureTextColorsGenerated();
                return mBodyTextColor;
            }

            private void ensureTextColorsGenerated()
            {
                if (!mGeneratedTextColors)
                {
                    // First check white, as most colors will be dark
                    int lightBodyAlpha = ColorHelpers.CalculateMinimumAlpha(
                            ColorHelpers.WHITE, mRgb, MIN_CONTRAST_BODY_TEXT);
                    int lightTitleAlpha = ColorHelpers.CalculateMinimumAlpha(
                            ColorHelpers.WHITE, mRgb, MIN_CONTRAST_TITLE_TEXT);

                    if (lightBodyAlpha != -1 && lightTitleAlpha != -1)
                    {
                        // If we found valid light values, use them and return
                        mBodyTextColor = ColorHelpers.SetAlphaComponent(ColorHelpers.WHITE, lightBodyAlpha);
                        mTitleTextColor = ColorHelpers.SetAlphaComponent(ColorHelpers.WHITE, lightTitleAlpha);
                        mGeneratedTextColors = true;
                        return;
                    }

                    int darkBodyAlpha = ColorHelpers.CalculateMinimumAlpha(
                            ColorHelpers.BLACK, mRgb, MIN_CONTRAST_BODY_TEXT);
                    int darkTitleAlpha = ColorHelpers.CalculateMinimumAlpha(
                            ColorHelpers.BLACK, mRgb, MIN_CONTRAST_TITLE_TEXT);

                    if (darkBodyAlpha != -1 && darkBodyAlpha != -1)
                    {
                        // If we found valid dark values, use them and return
                        mBodyTextColor = ColorHelpers.SetAlphaComponent(ColorHelpers.BLACK, darkBodyAlpha);
                        mTitleTextColor = ColorHelpers.SetAlphaComponent(ColorHelpers.BLACK, darkTitleAlpha);
                        mGeneratedTextColors = true;
                        return;
                    }

                    // If we reach here then we can not find title and body values which use the same
                    // lightness, we need to use mismatched values
                    mBodyTextColor = lightBodyAlpha != -1
                            ? ColorHelpers.SetAlphaComponent(ColorHelpers.WHITE, lightBodyAlpha)
                            : ColorHelpers.SetAlphaComponent(ColorHelpers.BLACK, darkBodyAlpha);
                    mTitleTextColor = lightTitleAlpha != -1
                            ? ColorHelpers.SetAlphaComponent(ColorHelpers.WHITE, lightTitleAlpha)
                            : ColorHelpers.SetAlphaComponent(ColorHelpers.BLACK, darkTitleAlpha);
                    mGeneratedTextColors = true;
                }
            }
        }

        /// <summary>
        /// Builder class for generating Palette instances.
        /// </summary>
        public sealed class Builder
        {
            private readonly WriteableBitmap mWriteableBitmap;

            private readonly List<Target> mTargets = new List<Target>();

            private int mMaxColors = DEFAULT_CALCULATE_NUMBER_COLORS;
            private int mResizeArea = DEFAULT_RESIZE_BITMAP_AREA;
            private int mResizeMaxDimension = -1;

            private readonly List<Filter> mFilters = new List<Filter>();

            /// <summary>
            /// Construct a new Builder using a source WriteableBitmap
            /// </summary>
            /// <param name="bitmap"></param>
            public Builder(WriteableBitmap bitmap)
            {
                if (bitmap == null)
                {
                    throw new Exception("WriteableBitmap is not valid");
                }
                mFilters.Add(DEFAULT_FILTER);
                mWriteableBitmap = bitmap;

                // Add the default targets
                mTargets.Add(Target.LIGHT_VIBRANT);
                mTargets.Add(Target.VIBRANT);
                mTargets.Add(Target.DARK_VIBRANT);
                mTargets.Add(Target.LIGHT_MUTED);
                mTargets.Add(Target.MUTED);
                mTargets.Add(Target.DARK_MUTED);
            }

            /**
             * Generate and return the {@link Palette} synchronously.
             */
            public Palette Generate()
            {
                List<Swatch> swatches;
                // We have a WriteableBitmap so we need to use quantization to reduce the number of colors

               
                // First we'll scale down the bitmap if needed
                WriteableBitmap bitmap = ScaleBitmapDown(mWriteableBitmap);
                // Now generate a quantizer from the WriteableBitmap
                ColorCutQuantizer quantizer = new ColorCutQuantizer(
                        GetPixelsFromBitmap(bitmap),
                        mMaxColors,
                        (mFilters.Count == 0) ? null : mFilters.ToArray());
               
                swatches = quantizer.GetQuantizedColors();
               
                // Now create a Palette instance
                Palette p = new Palette(swatches, mTargets);
                // And make it generate itself
                p.Generate();
                return p;
            }

            private int[] GetPixelsFromBitmap(WriteableBitmap bitmap)
            {
                byte[] array = bitmap.ToByteArray();
                int[] subsetPixels = new int[bitmap.PixelWidth * bitmap.PixelHeight];

                for(int i = 0; i < subsetPixels.Length - 1; i++)
                {
                    subsetPixels[i] = ColorHelpers.ARGB(array[i * 4 + 3], array[i * 4 + 2], array[i * 4 + 1], array[i * 4]);
                }
                return subsetPixels;
            }

            private WriteableBitmap ScaleBitmapDown(WriteableBitmap bitmap)
            {
                double scaleRatio = -1;

                if (mResizeArea > 0)
                {
                    int bitmapArea = bitmap.PixelWidth * bitmap.PixelHeight;
                    if (bitmapArea > mResizeArea)
                    {
                        scaleRatio = Math.Sqrt(mResizeArea / (double)bitmapArea);
                    }
                }
                else if (mResizeMaxDimension > 0)
                {
                    int maxDimension = Math.Max(bitmap.PixelWidth, bitmap.PixelHeight);
                    if (maxDimension > mResizeMaxDimension)
                    {
                        scaleRatio = mResizeMaxDimension / (double)maxDimension;
                    }
                }

                if (scaleRatio <= 0)
                {
                    // Scaling has been disabled or not needed so just return the WriteableBitmap
                    return bitmap;
                }

                return bitmap.Resize(
                        (int)Math.Ceiling(bitmap.PixelWidth * scaleRatio),
                        (int)Math.Ceiling(bitmap.PixelHeight * scaleRatio),
                        WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            /**
             * Set the maximum number of colors to use in the quantization step when using a
             * {@link android.graphics.WriteableBitmap} as the source.
             * <p>
             * Good values for depend on the source image type. For landscapes, good values are in
             * the range 10-16. For images which are largely made up of people's faces then this
             * value should be increased to ~24.
             */
            //@NonNull
            public Builder maximumColorCount(int colors)
            {
                mMaxColors = colors;
                return this;
            }

            /**
             * Set the resize value when using a {@link android.graphics.WriteableBitmap} as the source.
             * If the bitmap's largest dimension is greater than the value specified, then the bitmap
             * will be resized so that its largest dimension matches {@code maxDimension}. If the
             * bitmap is smaller or equal, the original is used as-is.
             *
             * //@Deprecated Using {@link #resizeWriteableBitmapArea(int)} is preferred since it can handle
             * abnormal aspect ratios more gracefully.
             *
             * @param maxDimension the number of pixels that the max dimension should be scaled down to,
             *                     or any value <= 0 to disable resizing.
             */
            //@NonNull
            //@Deprecated
            public Builder resizeWriteableBitmapSize(int maxDimension)
            {
                mResizeMaxDimension = maxDimension;
                mResizeArea = -1;
                return this;
            }

            /**
             * Set the resize value when using a {@link android.graphics.WriteableBitmap} as the source.
             * If the bitmap's area is greater than the value specified, then the bitmap
             * will be resized so that its area matches {@code area}. If the
             * bitmap is smaller or equal, the original is used as-is.
             * <p>
             * This value has a large effect on the processing time. The larger the resized image is,
             * the greater time it will take to generate the palette. The smaller the image is, the
             * more detail is lost in the resulting image and thus less precision for color selection.
             *
             * @param area the number of pixels that the intermediary scaled down WriteableBitmap should cover,
             *             or any value <= 0 to disable resizing.
             */
            //@NonNull
            public Builder resizeWriteableBitmapArea(int area)
            {
                mResizeArea = area;
                mResizeMaxDimension = -1;
                return this;
            }

            /**
             * Clear all added filters. This includes any default filters added automatically by
             * {@link Palette}.
             */
            //@NonNull
            public Builder clearFilters()
            {
                mFilters.Clear();
                return this;
            }

            /**
             * Add a filter to be able to have fine grained control over which colors are
             * allowed in the resulting palette.
             *
             * @param filter filter to add.
             */
            //@NonNull
            public Builder addFilter(Filter filter)
            {
                if (filter != null)
                {
                    mFilters.Add(filter);
                }
                return this;
            }

            /**
             * Add a target profile to be generated in the palette.
             *
             * <p>You can retrieve the result via {@link Palette#getSwatchForTarget(Target)}.</p>
             */
            //@NonNull
            public Builder addTarget(Target target)
            {
                if (!mTargets.Contains(target))
                {
                    mTargets.Add(target);
                }
                return this;
            }

            /**
             * Clear all added targets. This includes any default targets added automatically by
             * {@link Palette}.
             */
            //@NonNull
            public Builder clearTargets()
            {
                if (mTargets != null)
                {
                    mTargets.Clear();
                }
                return this;
            }
        }

        public sealed class Builder2
        {
            private readonly BitmapDecoder mDecoder;

            private readonly List<Target> mTargets = new List<Target>();

            private int mMaxColors = DEFAULT_CALCULATE_NUMBER_COLORS;
            private int mResizeArea = DEFAULT_RESIZE_BITMAP_AREA;
            private int mResizeMaxDimension = -1;

            private readonly List<Filter> mFilters = new List<Filter>();

            public Builder2(BitmapDecoder decoder)
            {
                if (decoder == null)
                {
                    throw new Exception("BitmapDecoder is not valid");
                }
                mFilters.Add(DEFAULT_FILTER);
                mDecoder = decoder;

                // Add the default targets
                mTargets.Add(Target.LIGHT_VIBRANT);
                mTargets.Add(Target.VIBRANT);
                mTargets.Add(Target.DARK_VIBRANT);
                mTargets.Add(Target.LIGHT_MUTED);
                mTargets.Add(Target.MUTED);
                mTargets.Add(Target.DARK_MUTED);
            }

            /**
             * Generate and return the {@link Palette} synchronously.
             */
            public async Task<Palette> Generate()
            {
                List<Swatch> swatches;
                // We have a WriteableBitmap so we need to use quantization to reduce the number of colors

                var pixels = await ScaleDownAndGetPixels(mDecoder);
                ColorCutQuantizer quantizer = new ColorCutQuantizer(
                        pixels,
                        mMaxColors,
                        (mFilters.Count == 0) ? null : mFilters.ToArray());
                swatches = quantizer.GetQuantizedColors();

                // Now create a Palette instance
                Palette p = new Palette(swatches, mTargets);
                // And make it generate itself
                p.Generate();
                return p;
            }

            private async Task<int[]> ScaleDownAndGetPixels(BitmapDecoder decoder)
            {
                double scaleRatio = -1;

                if (mResizeArea > 0)
                {
                    uint bitmapArea = decoder.PixelWidth * decoder.PixelHeight;
                    if (bitmapArea > mResizeArea)
                    {
                        scaleRatio = Math.Sqrt(mResizeArea / (double)bitmapArea);
                    }
                }
                else if (mResizeMaxDimension > 0)
                {
                    int maxDimension = Math.Max((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    if (maxDimension > mResizeMaxDimension)
                    {
                        scaleRatio = mResizeMaxDimension / (double)maxDimension;
                    }
                }


                PixelDataProvider pixelsData;

                if (scaleRatio > 0)
                {
                    BitmapTransform bt = new BitmapTransform();
                    bt.ScaledHeight = (uint)Math.Ceiling(decoder.PixelHeight * scaleRatio);
                    bt.ScaledWidth = (uint)Math.Ceiling(decoder.PixelWidth * scaleRatio);
                    pixelsData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight, bt,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage);
                }
                else
                {
                    pixelsData = await decoder.GetPixelDataAsync();
                }

                var pixels = pixelsData.DetachPixelData();
                int[] subsetPixels = new int[pixels.Length / 4];

                for (int i = 0; i < subsetPixels.Length - 1; i++)
                {
                    subsetPixels[i] = ColorHelpers.ARGB(pixels[i * 4 + 3], pixels[i * 4 + 2], pixels[i * 4 + 1], pixels[i * 4]);
                }
                return subsetPixels;
            }


            /**
             * Set the maximum number of colors to use in the quantization step when using a
             * {@link android.graphics.WriteableBitmap} as the source.
             * <p>
             * Good values for depend on the source image type. For landscapes, good values are in
             * the range 10-16. For images which are largely made up of people's faces then this
             * value should be increased to ~24.
             */
            //@NonNull
            public Builder2 maximumColorCount(int colors)
            {
                mMaxColors = colors;
                return this;
            }

            /**
             * Set the resize value when using a {@link android.graphics.WriteableBitmap} as the source.
             * If the bitmap's area is greater than the value specified, then the bitmap
             * will be resized so that its area matches {@code area}. If the
             * bitmap is smaller or equal, the original is used as-is.
             * <p>
             * This value has a large effect on the processing time. The larger the resized image is,
             * the greater time it will take to generate the palette. The smaller the image is, the
             * more detail is lost in the resulting image and thus less precision for color selection.
             *
             * @param area the number of pixels that the intermediary scaled down WriteableBitmap should cover,
             *             or any value <= 0 to disable resizing.
             */
            //@NonNull
            public Builder2 resizeWriteableBitmapArea(int area)
            {
                mResizeArea = area;
                mResizeMaxDimension = -1;
                return this;
            }

            /**
             * Clear all added filters. This includes any default filters added automatically by
             * {@link Palette}.
             */
            //@NonNull
            public Builder2 clearFilters()
            {
                mFilters.Clear();
                return this;
            }

            /**
             * Add a filter to be able to have fine grained control over which colors are
             * allowed in the resulting palette.
             *
             * @param filter filter to add.
             */
            //@NonNull
            public Builder2 addFilter(Filter filter)
            {
                if (filter != null)
                {
                    mFilters.Add(filter);
                }
                return this;
            }

            /**
             * Add a target profile to be generated in the palette.
             *
             * <p>You can retrieve the result via {@link Palette#getSwatchForTarget(Target)}.</p>
             */
            //@NonNull
            public Builder2 addTarget(Target target)
            {
                if (!mTargets.Contains(target))
                {
                    mTargets.Add(target);
                }
                return this;
            }

            /**
             * Clear all added targets. This includes any default targets added automatically by
             * {@link Palette}.
             */
            //@NonNull
            public Builder2 clearTargets()
            {
                if (mTargets != null)
                {
                    mTargets.Clear();
                }
                return this;
            }
        }

        /**
         * A Filter provides a mechanism for exercising fine-grained control over which colors
         * are valid within a resulting {@link Palette}.
         */
        public interface Filter
        {
            /**
             * Hook to allow clients to be able filter colors from resulting palette.
             *
             * @param rgb the color in RGB888.
             * @param hsl HSL representation of the color.
             *
             * @return true if the color is allowed, false if not.
             *
             * @see Builder#addFilter(Filter)
             */
            bool IsAllowed(int rgb, float[] hsl);
        }

        /**
         * The default filter.
         */
        static readonly Filter DEFAULT_FILTER = new DefaultFilter();
        
        private class DefaultFilter : Filter
        {
            private const float BLACK_MAX_LIGHTNESS = 0.05f;
            private const float WHITE_MIN_LIGHTNESS = 0.95f;

            public bool IsAllowed(int rgb, float[] hsl)
            {
                return !IsWhite(hsl) && !IsBlack(hsl) && !IsNearRedILine(hsl);
            }

            /**
             * @return true if the color represents a color which is close to black.
             */
            private bool IsBlack(float[] hslColor)
            {
                return hslColor[2] <= BLACK_MAX_LIGHTNESS;
            }

            /**
             * @return true if the color represents a color which is close to white.
             */
            private bool IsWhite(float[] hslColor)
            {
                return hslColor[2] >= WHITE_MIN_LIGHTNESS;
            }

            /**
             * @return true if the color lies close to the red side of the I line.
             */
            private bool IsNearRedILine(float[] hslColor)
            {
                return hslColor[0] >= 10f && hslColor[0] <= 37f && hslColor[1] <= 0.82f;
            }
        }
    }
}
