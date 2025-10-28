using System.Drawing;

namespace ChatAppClient.Helpers
{
    // Phải là 'public static class'
    public static class AppColors
    {
        public static Color Primary = Color.FromArgb(0, 145, 255);
        public static Color LightGray = Color.FromArgb(240, 240, 240);
        public static Color Background = Color.FromArgb(229, 221, 213);
        public static Color FormBackground = Color.White;
        public static Color TextPrimary = Color.Black;
        public static Color TextSecondary = Color.Gray;
        public static Color Online = Color.LawnGreen;
        public static Color Offline = Color.Gray;
    }
}