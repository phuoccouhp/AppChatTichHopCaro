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
        private bool _isTogglingEye = false; // Flag để tránh xóa text khi toggle

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
            InnerTextBox.UseSystemPasswordChar = false; // Mặc định tắt, sẽ bật khi IsPassword = true

            // Chặn phím Enter
            InnerTextBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true; };
            
            // Thêm event KeyPress để đảm bảo UseSystemPasswordChar được bật ngay khi người dùng nhập
            InnerTextBox.KeyPress += InnerTextBox_KeyPress;

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
            IconRight.TabStop = false; // Không can thiệp vào tab navigation
            IconRight.Click += ToggleEye;

            this.Controls.Add(IconLeft);
            this.Controls.Add(InnerTextBox);
            this.Controls.Add(IconRight); // Thêm IconRight sau để nó ở trên cùng, có thể click được
            IconRight.BringToFront(); // Đảm bảo icon ở trên cùng để có thể click được

            // Events
            InnerTextBox.Enter += RemovePlaceholder;
            InnerTextBox.Leave += SetPlaceholder;
            InnerTextBox.TextChanged += InnerTextBox_TextChanged;
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
                IconRight.BringToFront(); // Đảm bảo icon ở trên cùng để có thể click được
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
            get 
            { 
                // Nếu đang là placeholder, trả về rỗng
                if (_isPlaceholder) return "";
                return InnerTextBox.Text; 
            }
            set
            {
                string newText = value ?? "";
                
                if (string.IsNullOrWhiteSpace(newText))
                {
                    // Text rỗng, set placeholder
                    SetPlaceholder(null, null);
                }
                else
                {
                    // Kiểm tra xem text có giống placeholder text không
                    // Nếu giống VÀ chưa có text thực sự từ người dùng (chưa focus vào), coi là placeholder
                    // Nếu khác hoặc đã có text thực sự, coi là text thực sự
                    bool isPlaceholderValue = !string.IsNullOrEmpty(_placeholderText) && newText == _placeholderText;
                    
                    if (isPlaceholderValue && !InnerTextBox.Focused)
                    {
                        // Set như placeholder (khi set từ code, chưa focus)
                        _isPlaceholder = true;
                        InnerTextBox.Text = _placeholderText;
                        InnerTextBox.ForeColor = _placeholderColor;
                        if (_isPasswordMode)
                        {
                            InnerTextBox.UseSystemPasswordChar = false;
                        }
                    }
                    else
                    {
                        // Text thực sự
                        _isPlaceholder = false;
                        InnerTextBox.Text = newText;
                        InnerTextBox.ForeColor = _textColor;
                        if (_isPasswordMode)
                        {
                            InnerTextBox.UseSystemPasswordChar = true;
                        }
                    }
                }
                this.Invalidate();
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
                    // Luôn bật UseSystemPasswordChar nếu không phải placeholder
                    if (!_isPlaceholder && !string.IsNullOrEmpty(InnerTextBox.Text) && InnerTextBox.Text != _placeholderText)
                    {
                        InnerTextBox.UseSystemPasswordChar = true;
                    }
                    // Đảm bảo IconRight hiển thị nếu có IconEnd
                    if (IconRight.Image != null)
                    {
                        IconRight.Visible = true;
                        IconRight.BringToFront(); // Đảm bảo icon ở trên cùng
                    }
                }
                else
                {
                    InnerTextBox.Multiline = true;
                    InnerTextBox.UseSystemPasswordChar = false;
                    // Ẩn IconRight nếu không phải password mode
                    IconRight.Visible = false;
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
            set 
            { 
                IconRight.Image = value; 
                // Đảm bảo IconRight hiển thị khi có image và là password mode
                if (value != null && _isPasswordMode)
                {
                    IconRight.Visible = true;
                    IconRight.BringToFront(); // Đảm bảo icon ở trên cùng
                }
                CenterContent(); 
            }
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
        private void InnerTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Khi người dùng nhập ký tự, nếu là password mode, đảm bảo UseSystemPasswordChar được bật
            if (_isPasswordMode)
            {
                // Nếu đang là placeholder, xóa nó trước
                if (_isPlaceholder)
                {
                    InnerTextBox.Text = "";
                    _isPlaceholder = false;
                    InnerTextBox.ForeColor = _textColor;
                }
                // Bật UseSystemPasswordChar ngay lập tức
                InnerTextBox.UseSystemPasswordChar = true;
            }
        }

        private void ToggleEye(object sender, EventArgs e)
        {
            // Chỉ toggle nếu là password mode
            if (!_isPasswordMode) return;
            
            // Đặt flag để tránh sự kiện Leave xóa text
            _isTogglingEye = true;
            
            // Lưu text hiện tại (nếu không phải placeholder)
            string currentText = "";
            bool wasPlaceholder = _isPlaceholder;
            if (!_isPlaceholder)
            {
                currentText = InnerTextBox.Text;
            }
            
            // Nếu đang là placeholder, xóa nó trước khi toggle
            if (_isPlaceholder)
            {
                InnerTextBox.Text = "";
                InnerTextBox.ForeColor = _textColor;
                _isPlaceholder = false;
            }
            
            // Toggle password visibility
            InnerTextBox.UseSystemPasswordChar = !InnerTextBox.UseSystemPasswordChar;
            
            // Khôi phục text ngay lập tức nếu có (đảm bảo không bị mất)
            if (!wasPlaceholder && !string.IsNullOrEmpty(currentText))
            {
                InnerTextBox.Text = currentText;
            }
            
            // Đảm bảo TextBox vẫn focus để người dùng có thể tiếp tục nhập
            InnerTextBox.Focus();
            
            // Tắt flag sau một khoảng thời gian ngắn (sử dụng Application.DoEvents để xử lý các sự kiện đang chờ)
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 50; // 50ms
            timer.Tick += (s, args) =>
            {
                _isTogglingEye = false;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void InnerTextBox_TextChanged(object sender, EventArgs e)
        {
            // Khi người dùng thay đổi text, nếu text khác placeholder text, coi là text thực sự
            if (!string.IsNullOrEmpty(InnerTextBox.Text) && InnerTextBox.Text != _placeholderText)
            {
                // Text thực sự từ người dùng
                _isPlaceholder = false;
                InnerTextBox.ForeColor = _textColor;
                if (_isPasswordMode)
                {
                    InnerTextBox.UseSystemPasswordChar = true;
                }
            }
            else if (string.IsNullOrWhiteSpace(InnerTextBox.Text) || InnerTextBox.Text == _placeholderText)
            {
                // Có thể là placeholder, nhưng chỉ set nếu không đang focus (người dùng đang nhập)
                if (!InnerTextBox.Focused)
                {
                    _isPlaceholder = true;
                    InnerTextBox.ForeColor = _placeholderColor;
                    if (_isPasswordMode)
                    {
                        InnerTextBox.UseSystemPasswordChar = false;
                    }
                }
            }
        }

        private void RemovePlaceholder(object sender, EventArgs e)
        {
            if (_isPlaceholder)
            {
                InnerTextBox.Text = "";
                InnerTextBox.ForeColor = _textColor;
                _isPlaceholder = false;
                // QUAN TRỌNG: Bật UseSystemPasswordChar ngay khi người dùng click vào ô nhập
                if (_isPasswordMode)
                {
                    InnerTextBox.UseSystemPasswordChar = true;
                }
            }
            else
            {
                // Nếu không phải placeholder nhưng là password mode, đảm bảo UseSystemPasswordChar được bật
                if (_isPasswordMode && !string.IsNullOrEmpty(InnerTextBox.Text) && InnerTextBox.Text != _placeholderText)
                {
                    InnerTextBox.UseSystemPasswordChar = true;
                }
            }
            this.Invalidate();
        }

        private void SetPlaceholder(object sender, EventArgs e)
        {
            // Không set placeholder nếu đang toggle eye (tránh xóa mật khẩu)
            if (_isTogglingEye) return;
            
            // Chỉ set placeholder nếu text rỗng hoặc giống placeholder text VÀ không đang focus
            if (string.IsNullOrWhiteSpace(InnerTextBox.Text) || (InnerTextBox.Text == _placeholderText && !InnerTextBox.Focused))
            {
                _isPlaceholder = true;
                InnerTextBox.Text = _placeholderText;
                InnerTextBox.ForeColor = _placeholderColor;
                InnerTextBox.UseSystemPasswordChar = false;
                if (!InnerTextBox.Multiline) InnerTextBox.Multiline = true;
                CenterContent();
            }
            else if (!string.IsNullOrEmpty(InnerTextBox.Text) && InnerTextBox.Text != _placeholderText)
            {
                // Có text thực sự
                _isPlaceholder = false;
                InnerTextBox.ForeColor = _textColor;
                // Nếu có text thực sự và là password mode, đảm bảo UseSystemPasswordChar được bật
                if (_isPasswordMode)
                {
                    InnerTextBox.UseSystemPasswordChar = true;
                }
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