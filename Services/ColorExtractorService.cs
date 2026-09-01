using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShowMyMusic.Services
{
    public class ColorExtractorService
    {
        private static readonly ConcurrentDictionary<string, (Color Accent, Color Glow)> _colorCache = new();
        private const int MaxCacheSize = 60;

        public static async Task<(Color Accent, Color Glow)> ExtractColorsAsync(
            BitmapSource? image, 
            string cacheKey, 
            double glowIntensity, 
            Color fallbackColor)
        {
            if (image == null)
            {
                byte glowAlpha = (byte)Math.Clamp((int)(glowIntensity * 255), 0, 255);
                var fallbackAccent = Color.FromRgb(fallbackColor.R, fallbackColor.G, fallbackColor.B);
                return (fallbackAccent, Color.FromArgb(glowAlpha, fallbackAccent.R, fallbackAccent.G, fallbackAccent.B));
            }

            // Include image dimensions in cache key to invalidate on size/stream change
            string fullCacheKey = $"{cacheKey}_{image.PixelWidth}x{image.PixelHeight}";

            if (!string.IsNullOrEmpty(cacheKey) && _colorCache.TryGetValue(fullCacheKey, out var cached))
            {
                byte glowAlpha = (byte)Math.Clamp((int)(glowIntensity * 255), 0, 255);
                return (cached.Accent, Color.FromArgb(glowAlpha, cached.Accent.R, cached.Accent.G, cached.Accent.B));
            }

            return await Task.Run(() =>
            {
                try
                {
                    // 1. Downscale to 48x48 (2304 pixels) for fast, detailed sampling
                    int targetSize = 48;
                    double scaleX = (double)targetSize / Math.Max(1, image.PixelWidth);
                    double scaleY = (double)targetSize / Math.Max(1, image.PixelHeight);

                    var scaled = new TransformedBitmap(image, new ScaleTransform(scaleX, scaleY));
                    var formatted = new FormatConvertedBitmap(scaled, PixelFormats.Bgra32, null, 0);
                    formatted.Freeze();

                    int stride = targetSize * 4;
                    byte[] pixels = new byte[targetSize * stride];
                    formatted.CopyPixels(pixels, stride, 0);

                    // 2. Multi-tier color analysis
                    double bestVibrantScore = -1;
                    Color bestVibrantColor = fallbackColor;

                    double bestMutedScore = -1;
                    Color bestMutedColor = fallbackColor;

                    // Hue histogram buckets (18 buckets, 20 degrees each)
                    double[] hueBucketScores = new double[18];
                    Color[] hueBucketColors = new Color[18];
                    double[] hueBucketMaxScores = new double[18];

                    double totalLuminance = 0;
                    int validPixelCount = 0;

                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        byte b = pixels[i];
                        byte g = pixels[i + 1];
                        byte r = pixels[i + 2];
                        byte a = pixels[i + 3];

                        if (a < 64) continue; // Skip transparent

                        validPixelCount++;
                        RgbToHsl(r, g, b, out double h, out double s, out double l);
                        totalLuminance += l;

                        // Tier 1: Vibrant colored pixels (saturated, mid-range brightness)
                        if (s >= 0.20 && l >= 0.15 && l <= 0.88)
                        {
                            // Favor rich colors over washed-out/dark ones
                            double lumFactor = 1.0 - Math.Abs(l - 0.52) * 1.6;
                            double score = (s * 2.2) + Math.Max(0.1, lumFactor * 1.5);

                            int bucket = (int)Math.Clamp(Math.Floor(h / 20.0), 0, 17);
                            hueBucketScores[bucket] += score;
                            if (score > hueBucketMaxScores[bucket])
                            {
                                hueBucketMaxScores[bucket] = score;
                                hueBucketColors[bucket] = Color.FromRgb(r, g, b);
                            }

                            if (score > bestVibrantScore)
                            {
                                bestVibrantScore = score;
                                bestVibrantColor = Color.FromRgb(r, g, b);
                            }
                        }
                        // Tier 2: Muted / Dark / Light / Pastel colored pixels
                        else if (s >= 0.08 && l >= 0.08 && l <= 0.94)
                        {
                            double score = s + (1.0 - Math.Abs(l - 0.5)) * 0.6;
                            if (score > bestMutedScore)
                            {
                                bestMutedScore = score;
                                bestMutedColor = Color.FromRgb(r, g, b);
                            }
                        }
                    }

                    Color selectedColor;

                    // 3. Selection hierarchy
                    // A) Top dominant hue cluster with multiple vibrant pixels
                    int bestBucketIndex = -1;
                    double maxBucketScore = -1;
                    for (int k = 0; k < 18; k++)
                    {
                        if (hueBucketScores[k] > maxBucketScore && hueBucketMaxScores[k] > 0)
                        {
                            maxBucketScore = hueBucketScores[k];
                            bestBucketIndex = k;
                        }
                    }

                    if (bestBucketIndex >= 0 && hueBucketScores[bestBucketIndex] > 2.0)
                    {
                        selectedColor = hueBucketColors[bestBucketIndex];
                    }
                    else if (bestVibrantScore > 0)
                    {
                        selectedColor = bestVibrantColor;
                    }
                    else if (bestMutedScore > 0)
                    {
                        selectedColor = bestMutedColor;
                    }
                    else
                    {
                        // True monochrome/grayscale album cover
                        double avgLum = validPixelCount > 0 ? totalLuminance / validPixelCount : 0.5;
                        if (avgLum < 0.3)
                            selectedColor = Color.FromRgb(148, 163, 184); // Slate 400
                        else
                            selectedColor = Color.FromRgb(203, 213, 225); // Slate 300
                    }

                    // 4. Boost brightness & saturation for UI visibility & vivid glow
                    RgbToHsl(selectedColor.R, selectedColor.G, selectedColor.B, out double finH, out double finS, out double finL);

                    if (finS > 0.05)
                    {
                        // Ensure good saturation for colorful neon glow
                        finS = Math.Clamp(finS * 1.25, 0.45, 1.0);
                        // Ensure good lightness so glow and accents stand out on dark themes
                        finL = Math.Clamp(finL, 0.48, 0.68);
                        selectedColor = HslToRgb(finH, finS, finL);
                    }
                    else
                    {
                        // Monochrome boost
                        finL = Math.Clamp(finL, 0.60, 0.85);
                        selectedColor = HslToRgb(finH, 0, finL);
                    }

                    byte dynamicGlowAlpha = (byte)Math.Clamp((int)(glowIntensity * 255), 0, 255);
                    var glow = Color.FromArgb(dynamicGlowAlpha, selectedColor.R, selectedColor.G, selectedColor.B);

                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        if (_colorCache.Count > MaxCacheSize)
                        {
                            _colorCache.Clear();
                        }
                        _colorCache[fullCacheKey] = (selectedColor, glow);
                    }

                    return (selectedColor, glow);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ExtractColorsAsync error: {ex.Message}");
                    byte fallbackGlowAlpha = (byte)Math.Clamp((int)(glowIntensity * 255), 0, 255);
                    var safeFallback = Color.FromRgb(fallbackColor.R, fallbackColor.G, fallbackColor.B);
                    return (safeFallback, Color.FromArgb(fallbackGlowAlpha, safeFallback.R, safeFallback.G, safeFallback.B));
                }
            });
        }

        private static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double delta = max - min;

            l = (max + min) / 2.0;

            if (delta == 0)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

                if (max == rd)
                    h = ((gd - bd) / delta + (gd < bd ? 6 : 0)) * 60.0;
                else if (max == gd)
                    h = ((bd - rd) / delta + 2) * 60.0;
                else
                    h = ((rd - gd) / delta + 4) * 60.0;

                if (h < 0) h += 360.0;
                if (h >= 360.0) h -= 360.0;
            }
        }

        private static Color HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                double hNorm = h / 360.0;

                r = HueToRgb(p, q, hNorm + 1.0 / 3.0);
                g = HueToRgb(p, q, hNorm);
                b = HueToRgb(p, q, hNorm - 1.0 / 3.0);
            }

            return Color.FromRgb(
                (byte)Math.Clamp((int)Math.Round(r * 255), 0, 255),
                (byte)Math.Clamp((int)Math.Round(g * 255), 0, 255),
                (byte)Math.Clamp((int)Math.Round(b * 255), 0, 255));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }
    }
}