using System.Collections.Generic;

namespace ShowMyMusic.Models
{
    public class PresetTheme
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GlassStyle GlassStyle { get; set; }
        public double CornerRadiusCardPercent { get; set; }
        public double CornerRadiusArtPercent { get; set; }
        public double CardOpacity { get; set; }
        public double BlurRadius { get; set; }
        public bool UseAdaptiveColor { get; set; }
        public string BackgroundColor { get; set; } = "#1A1A24";
        public string AccentColor { get; set; } = "#8B5CF6";
        public bool AdaptiveGlowEnabled { get; set; }
        public double GlowIntensity { get; set; }
        public double GlowRadius { get; set; }
        public double BorderWidth { get; set; }
        public string BorderColor { get; set; } = "#33FFFFFF";

        public static List<PresetTheme> GetBuiltInPresets()
        {
            return new List<PresetTheme>
            {
                new PresetTheme
                {
                    Name = "🍏 iOS Liquid Glass",
                    Description = "Apple-inspired frosted glass with specular highlight reflection and adaptive glow",
                    GlassStyle = GlassStyle.iOSFrostedGlass,
                    CornerRadiusCardPercent = 50.0,
                    CornerRadiusArtPercent = 35.0,
                    CardOpacity = 0.85,
                    BlurRadius = 35.0,
                    UseAdaptiveColor = true,
                    BackgroundColor = "#1A1A26",
                    AccentColor = "#A855F7",
                    AdaptiveGlowEnabled = true,
                    GlowIntensity = 0.75,
                    GlowRadius = 30.0,
                    BorderWidth = 1.5,
                    BorderColor = "#55FFFFFF"
                },
                new PresetTheme
                {
                    Name = "⬛ Dynamic Island OLED",
                    Description = "Deep OLED pitch-black aesthetic with a smooth pill-shaped corner curve",
                    GlassStyle = GlassStyle.Solid,
                    CornerRadiusCardPercent = 50.0,
                    CornerRadiusArtPercent = 50.0,
                    CardOpacity = 1.0,
                    BlurRadius = 0.0,
                    UseAdaptiveColor = true,
                    BackgroundColor = "#000000",
                    AccentColor = "#38BDF8",
                    AdaptiveGlowEnabled = true,
                    GlowIntensity = 0.45,
                    GlowRadius = 18.0,
                    BorderWidth = 1.0,
                    BorderColor = "#262626"
                },
                new PresetTheme
                {
                    Name = "🪟 Windows 11 Acrylic",
                    Description = "Modern Fluent Design acrylic glass with subtle gradient and polished borders",
                    GlassStyle = GlassStyle.Win11Acrylic,
                    CornerRadiusCardPercent = 28.0,
                    CornerRadiusArtPercent = 20.0,
                    CardOpacity = 0.88,
                    BlurRadius = 25.0,
                    UseAdaptiveColor = true,
                    BackgroundColor = "#1E1E2C",
                    AccentColor = "#60A5FA",
                    AdaptiveGlowEnabled = false,
                    GlowIntensity = 0.35,
                    GlowRadius = 15.0,
                    BorderWidth = 1.0,
                    BorderColor = "#30FFFFFF"
                },
                new PresetTheme
                {
                    Name = "⚡ Cyberpunk Neon Glow",
                    Description = "High-voltage cyberpunk theme with high-intensity neon glow and dark background",
                    GlassStyle = GlassStyle.DarkGlass,
                    CornerRadiusCardPercent = 14.0,
                    CornerRadiusArtPercent = 8.0,
                    CardOpacity = 0.94,
                    BlurRadius = 20.0,
                    UseAdaptiveColor = false,
                    BackgroundColor = "#0B071A",
                    AccentColor = "#F43F5E",
                    AdaptiveGlowEnabled = true,
                    GlowIntensity = 1.0,
                    GlowRadius = 40.0,
                    BorderWidth = 2.0,
                    BorderColor = "#F43F5E"
                },
                new PresetTheme
                {
                    Name = "❄️ Apple Light Frosted",
                    Description = "Bright frosted glass with specular reflections and dark typography",
                    GlassStyle = GlassStyle.LightGlass,
                    CornerRadiusCardPercent = 48.0,
                    CornerRadiusArtPercent = 32.0,
                    CardOpacity = 0.90,
                    BlurRadius = 30.0,
                    UseAdaptiveColor = true,
                    BackgroundColor = "#F1F5F9",
                    AccentColor = "#7C3AED",
                    AdaptiveGlowEnabled = true,
                    GlowIntensity = 0.35,
                    GlowRadius = 20.0,
                    BorderWidth = 1.5,
                    BorderColor = "#80FFFFFF"
                },
                new PresetTheme
                {
                    Name = "🎯 Minimalist Matte",
                    Description = "Clean matte dark surface without glow for a distraction-free experience",
                    GlassStyle = GlassStyle.Solid,
                    CornerRadiusCardPercent = 18.0,
                    CornerRadiusArtPercent = 14.0,
                    CardOpacity = 1.0,
                    BlurRadius = 0.0,
                    UseAdaptiveColor = false,
                    BackgroundColor = "#181822",
                    AccentColor = "#A1A1AA",
                    AdaptiveGlowEnabled = false,
                    GlowIntensity = 0.0,
                    GlowRadius = 0.0,
                    BorderWidth = 1.0,
                    BorderColor = "#2A2A3C"
                }
            };
        }

        public void ApplyTo(AppSettings settings)
        {
            settings.GlassStyle = GlassStyle;
            settings.CornerRadiusCardPercent = CornerRadiusCardPercent;
            settings.CornerRadiusArtPercent = CornerRadiusArtPercent;
            settings.CardOpacity = CardOpacity;
            settings.BlurRadius = BlurRadius;
            settings.UseAdaptiveColor = UseAdaptiveColor;
            settings.CustomBackgroundColor = BackgroundColor;
            settings.CustomAccentColor = AccentColor;
            settings.AdaptiveGlowEnabled = AdaptiveGlowEnabled;
            settings.GlowIntensity = GlowIntensity;
            settings.GlowRadius = GlowRadius;
            settings.BorderWidth = BorderWidth;
            settings.BorderColor = BorderColor;
            settings.ActivePresetName = Name;
        }
    }
}