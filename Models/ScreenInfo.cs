namespace ShowMyMusic.Models
{
    public class ScreenInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        
        // Physical pixels (raw Win32)
        public int PixelX { get; set; }
        public int PixelY { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        
        public int PixelWorkAreaX { get; set; }
        public int PixelWorkAreaY { get; set; }
        public int PixelWorkAreaWidth { get; set; }
        public int PixelWorkAreaHeight { get; set; }
        
        // DPI Scaling
        public double DpiScaleX { get; set; } = 1.0;
        public double DpiScaleY { get; set; } = 1.0;

        // WPF DIPs (Device Independent Pixels)
        public double DipX => PixelX / DpiScaleX;
        public double DipY => PixelY / DpiScaleY;
        public double DipWidth => PixelWidth / DpiScaleX;
        public double DipHeight => PixelHeight / DpiScaleY;
        public double DipWorkAreaX => PixelWorkAreaX / DpiScaleX;
        public double DipWorkAreaY => PixelWorkAreaY / DpiScaleY;
        public double DipWorkAreaWidth => PixelWorkAreaWidth / DpiScaleX;
        public double DipWorkAreaHeight => PixelWorkAreaHeight / DpiScaleY;

        public override string ToString() => DisplayName;
    }
}