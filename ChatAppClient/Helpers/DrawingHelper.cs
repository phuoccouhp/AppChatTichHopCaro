using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.Helpers
{
    // Phải là 'public static class'
    public static class DrawingHelper
    {
        public static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ApplyRoundedCorners(Control control, int radius)
        {
            if (control == null || control.IsDisposed || control.Width == 0 || control.Height == 0) return;

            Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
            using (GraphicsPath path = CreateRoundedRectPath(rect, radius))
            {
                control.Region = new Region(path);
            }
        }
    }
}