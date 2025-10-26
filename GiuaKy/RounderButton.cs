using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

[DesignerCategory("Code")]
public class RoundedButton : Button
{
    private Color normalColor = Color.FromArgb(0, 120, 215);
    private Color hoverColor = Color.FromArgb(0, 150, 255);
    private Color currentColor;

    [Category("Rounded Button")]
    public Color NormalColor
    {
        get => normalColor;
        set { normalColor = value; currentColor = value; Invalidate(); }
    }

    [Category("Rounded Button")]
    public Color HoverColor
    {
        get => hoverColor;
        set { hoverColor = value; Invalidate(); }
    }

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        BackColor = normalColor;
        currentColor = normalColor;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        Size = new Size(160, 50);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        currentColor = hoverColor;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        currentColor = normalColor;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (SolidBrush brush = new SolidBrush(currentColor))
        using (GraphicsPath path = GetRoundRegion(ClientRectangle))
        {
            e.Graphics.FillPath(brush, path);
        }

        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Region = new Region(GetRoundRegion(ClientRectangle));
        Invalidate();
    }

    private GraphicsPath GetRoundRegion(Rectangle rect)
    {
        int radius = rect.Height / 2;
        int diameter = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
