using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

[System.ComponentModel.DesignerCategory("Code")]
public class UnderlinedTextBox : UserControl
{
    private readonly TextBox txtInput;
    private Color lineColor = Color.Gray;
    private Color focusLineColor = Color.DeepSkyBlue;
    private bool isFocused = false;

    // khoảng cách nhỏ giữa đáy control và gạch (dùng để căn caret)
    // giảm value xuống (0,1,2...) để caret sát gạch hơn
    private int gap = 1;

    [Category("Custom")]
    public Color LineColor
    {
        get => lineColor;
        set { lineColor = value; Invalidate(); }
    }

    [Category("Custom")]
    public Color FocusLineColor
    {
        get => focusLineColor;
        set { focusLineColor = value; Invalidate(); }
    }

    [Category("Custom")]
    public override string Text
    {
        get => txtInput.Text;
        set => txtInput.Text = value;
    }

    public int Gap
    {
        get => gap;
        set { gap = Math.Max(0, value); AdjustLayout(); }
    }

    public UnderlinedTextBox()
    {
        txtInput = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10f),
            BackColor = SystemColors.Window,
            ForeColor = Color.Black,
            ShortcutsEnabled = true
        };

        txtInput.GotFocus += (s, e) => { isFocused = true; Invalidate(); };
        txtInput.LostFocus += (s, e) => { isFocused = false; Invalidate(); };

        Controls.Add(txtInput);

        // những event để tự canh khi đổi size/font/parent
        Resize += (s, e) => AdjustLayout();
        txtInput.FontChanged += (s, e) => AdjustLayout();
        HandleCreated += (s, e) => AdjustLayout();

        // mặc định cao hơi thấp để caret gần gạch; m có thể set cao hơn trong Designer
        Height = 26;
        BackColor = Color.Transparent;
        AdjustLayout();
    }

    private void AdjustLayout()
    {
        if (txtInput == null) return;

        // đo chiều cao text thực tế theo font
        int textHeight = TextRenderer.MeasureText("Hg", txtInput.Font).Height;

        // set textbox height đúng bằng textHeight (không dư)
        txtInput.Height = textHeight;

        // đặt textbox sao cho caret nằm ngay trên gạch:
        // ý: đặt top = controlHeight - textHeight - gap - 1 (tối thiểu 0)
        int top = Math.Max(0, Height - textHeight - gap - 1);
        txtInput.Location = new Point(0, top);

        txtInput.Width = Math.Max(0, Width - 1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using (Pen pen = new Pen(isFocused ? focusLineColor : lineColor, 2))
        {
            e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        // đồng bộ màu nền TextBox với form hoặc parent để không thấy ô trắng
        txtInput.BackColor = Parent?.BackColor ?? SystemColors.Window;
        BackColor = txtInput.BackColor;
        AdjustLayout();
    }
}
