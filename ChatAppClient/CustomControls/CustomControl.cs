using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient
{
    public class RoundedTextBox : Panel
    {
        public TextBox InnerTextBox = new TextBox();
        public PictureBox IconLeft = new PictureBox();
        public PictureBox IconRight = new PictureBox();

        // --- Cấu hình màu sắc ---
        private Color _boxColor = Color.FromArgb(245, 245, 245);
        private Color _focusBorderColor = Color.FromArgb(0, 132, 255);
        private int _borderRadius = 15;

        // --- Biến trạng thái ---
        private string _placeholderText = "";
        private bool _isPlaceholder = false;
        private Color _placeholderColor = Color.Gray;
        private Color _textColor = Color.Black;
        private bool _isPasswordMode = false;

        public RoundedTextBox()
        {
            // 1. Cấu hình đồ họa
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            this.Size = new Size(300, 45);
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(10);

            // 2. Cấu hình TextBox
            InnerTextBox.BorderStyle = BorderStyle.None;
            InnerTextBox.Font = new Font("Segoe UI", 11F);
            InnerTextBox.BackColor = _boxColor;
            InnerTextBox.ForeColor = _textColor;
            InnerTextBox.Multiline = true; // Mặc định bật để chữ đẹp

            // Chặn phím Enter
            InnerTextBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true; };

            // 3. Icon Trái
            IconLeft.Size = new Size(24, 24);
            IconLeft.SizeMode = PictureBoxSizeMode.Zoom;
            IconLeft.BackColor = Color.Transparent;
            IconLeft.Visible = false;
            IconLeft.Click += (s, e) => InnerTextBox.Focus();

            // 4. Icon Phải (Dùng ảnh con mắt của bạn)
            IconRight.Size = new Size(24, 24);
            IconRight.SizeMode = PictureBoxSizeMode.Zoom;
            IconRight.BackColor = Color.Transparent;
            IconRight.Visible = false;
            IconRight.Cursor = Cursors.Hand;
            IconRight.Click += ToggleEye;

            this.Controls.Add(IconLeft);
            this.Controls.Add(IconRight);
            this.Controls.Add(InnerTextBox);

            // Events
            InnerTextBox.Enter += RemovePlaceholder;
            InnerTextBox.Leave += SetPlaceholder;
            this.Click += (s, e) => InnerTextBox.Focus();
            this.Resize += (s, e) => CenterContent();

            // [QUAN TRỌNG] Tính toán ngay khi khởi tạo
            CenterContent();
        }

        // --- [FIX] SỰ KIỆN QUAN TRỌNG ĐỂ KHÔNG BỊ LỆCH LÚC ĐẦU ---
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Ngay khi control được tạo ra trên màn hình -> Tính lại vị trí ngay lập tức
            CenterContent();
        }

        // --- HÀM TÍNH TOÁN VỊ TRÍ ---
        private void CenterContent()
        {
            // 1. Vị trí Icon Trái
            int iconY = (this.Height - 24) / 2 + 4;
            if (IconLeft.Image != null)
            {
                IconLeft.Visible = true;
                IconLeft.Location = new Point(10, iconY);
            }
            else IconLeft.Visible = false;

            // 2. Vị trí Icon Phải
            if (IconRight.Image != null && _isPasswordMode)
            {
                IconRight.Visible = true;
                IconRight.Location = new Point(this.Width - 34, iconY);
            }
            else IconRight.Visible = false;

            // 3. Vị trí TextBox (FIX LỖI ĐÈ CHỮ TẠI ĐÂY)
            // Kiểm tra trực tiếp Image != null thay vì Visible (vì Visible có thể bị false lúc load)
            int leftMargin = (IconLeft.Image != null) ? 45 : 15;
            int rightMargin = (IconRight.Image != null && _isPasswordMode) ? 40 : 15;

            InnerTextBox.Width = this.Width - leftMargin - rightMargin;

            if (InnerTextBox.Multiline)
            {
                InnerTextBox.Height = 30;
                InnerTextBox.Location = new Point(leftMargin, (this.Height - 30) / 2 + 4);
            }
            else
            {
                InnerTextBox.Location = new Point(leftMargin, (this.Height - InnerTextBox.PreferredHeight) / 2);
            }
        }
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string? Text
        {
            get { return InnerTextBox.Text; }
            set
            {
                InnerTextBox.Text = value ?? "";
                // Khi gán text bằng code, cần kiểm tra placeholder
                SetPlaceholder(null, null);
            }
        }
        [Category("Custom Properties")]
        public bool IsPassword
        {
            get { return _isPasswordMode; }
            set
            {
                _isPasswordMode = value;
                if (value)
                {
                    InnerTextBox.Multiline = false;
                    if (!_isPlaceholder) InnerTextBox.UseSystemPasswordChar = true;
                }
                else
                {
                    InnerTextBox.Multiline = true;
                    InnerTextBox.UseSystemPasswordChar = false;
                }
                CenterContent();
            }
        }

        [Category("Custom Properties")]
        public Image Icon
        {
            get { return IconLeft.Image; }
            set
            {
                IconLeft.Image = value;
                // Gọi hàm tính toán ngay khi gán ảnh
                CenterContent();
                this.Invalidate();
            }
        }

        [Category("Custom Properties")]
        public Image IconEnd
        {
            get { return IconRight.Image; }
            set { IconRight.Image = value; CenterContent(); }
        }

        [Category("Custom Properties")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _placeholderText = char.ToUpper(value[0]) + value.Substring(1);
                else
                    _placeholderText = value;
                SetPlaceholder(null, null);
            }
        }

        // --- Logic Mắt & Placeholder ---
        private void ToggleEye(object sender, EventArgs e)
        {
            if (_isPlaceholder) return;
            InnerTextBox.UseSystemPasswordChar = !InnerTextBox.UseSystemPasswordChar;
        }

        private void RemovePlaceholder(object sender, EventArgs e)
        {
            if (_isPlaceholder)
            {
                InnerTextBox.Text = "";
                InnerTextBox.ForeColor = _textColor;
                _isPlaceholder = false;
                if (_isPasswordMode) InnerTextBox.UseSystemPasswordChar = true;
            }
            this.Invalidate();
        }

        private void SetPlaceholder(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InnerTextBox.Text))
            {
                _isPlaceholder = true;
                InnerTextBox.Text = _placeholderText;
                InnerTextBox.ForeColor = _placeholderColor;
                InnerTextBox.UseSystemPasswordChar = false;
                if (!InnerTextBox.Multiline) InnerTextBox.Multiline = true;
                CenterContent();
            }
            this.Invalidate();
        }

        // --- Vẽ Giao Diện ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle rect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);
                path.AddArc(rect.X, rect.Y, _borderRadius, _borderRadius, 180, 90);
                path.AddArc(rect.Right - _borderRadius, rect.Y, _borderRadius, _borderRadius, 270, 90);
                path.AddArc(rect.Right - _borderRadius, rect.Bottom - _borderRadius, _borderRadius, _borderRadius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - _borderRadius, _borderRadius, _borderRadius, 90, 90);
                path.CloseAllFigures();

                using (SolidBrush brush = new SolidBrush(_boxColor))
                    e.Graphics.FillPath(brush, path);

                if (InnerTextBox.Focused)
                {
                    using (Pen pen = new Pen(_focusBorderColor, 2f))
                        e.Graphics.DrawPath(pen, path);
                }
            }
        }
    }
    // ==================== CODE MỚI ĐÃ CHỈNH SỬA ====================
    // Điều khiển Button tùy chỉnh sử dụng Region để cắt góc
    public class RoundedButton : Button
    {
        // Bán kính bo góc. Đặt bằng chiều cao để tạo nút tròn 2 đầu (hình viên thuốc)
        private int _borderRadius = 50;

        public RoundedButton()
        {
            // Cấu hình cơ bản cho nút phẳng
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0; // Tắt viền của FlatStyle
            this.Size = new Size(350, 50); // Kích thước mặc định giống nút Login
            this.BackColor = Color.FromArgb(12, 42, 82); // Màu xanh đậm mặc định
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.Cursor = Cursors.Hand; // Đổi con trỏ chuột thành hình bàn tay

            // Sự kiện Resize để cập nhật lại vùng cắt khi kích thước nút thay đổi
            this.Resize += (s, e) => { SetButtonRegion(); };

            // Gọi lần đầu để thiết lập hình dạng
            SetButtonRegion();
        }

        // Phương thức tạo GraphicsPath hình bo tròn và gán cho Region của nút
        private void SetButtonRegion()
        {
            if (this.Width == 0 || this.Height == 0) return;

            Rectangle rect = this.ClientRectangle;
            GraphicsPath path = new GraphicsPath();

            // Để tạo nút tròn 2 đầu như mẫu, bán kính nên bằng chiều cao của nút
            int radius = this.Height;

            // Thêm các cung tròn vào path
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Góc trên-trái
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Góc trên-phải
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Góc dưới-phải
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Góc dưới-trái
            path.CloseFigure();

            // Gán path làm vùng hiển thị (Region) cho nút.
            // Phần nằm ngoài path này sẽ bị cắt bỏ (trong suốt).
            this.Region = new Region(path);
        }

        // Ghi đè OnPaint để bật chế độ khử răng cưa, giúp mép nút mịn hơn
        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Gọi base.OnPaint để hệ thống tự vẽ màu nền, text và hình ảnh
            // Nó chỉ vẽ được trong vùng Region đã định nghĩa ở trên.
            base.OnPaint(pevent);
        }
    }
    // ============================================================
}