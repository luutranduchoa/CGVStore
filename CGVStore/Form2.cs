using System;
using System.Linq;
using System.Windows.Forms;
using CGVStore.Models; // Cần thiết để truy cập DbContext (Model1) và Entity (User)

namespace CGVStore
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            // 1. Thiết lập cơ bản cho Form
            this.Text = "Đăng Nhập Hệ Thống CGV";
            this.Name = "FormDangNhap"; // Đặt tên Form để Form1 có thể kiểm tra

            // 2. Thiết lập cho TextBox Mật Khẩu
            this.textBox2.PasswordChar = '•'; // Che mật khẩu

            // 3. Gán sự kiện Click cho các nút
            this.button1.Click += new EventHandler(buttonDangNhap_Click);
            this.button2.Click += new EventHandler(buttonThoat_Click);
            this.linkLabel1.Click += new EventHandler(linkLabelQuenMatKhau_Click);

            // Đặt focus vào ô Tài Khoản khi Form khởi động
            this.Load += (sender, e) => { textBox1.Focus(); };
        }

        // =======================================================
        //                 HÀM XỬ LÝ SỰ KIỆN
        // =======================================================

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Đăng Nhập (button1)
        /// </summary>
        private void buttonDangNhap_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tài khoản và Mật khẩu.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Kiểm tra đăng nhập với Database
            bool loginSuccess = KiemTraDangNhap(username, password);

            if (loginSuccess)
            {
                MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 2. Mở khóa Menu Chính trên Form1 (MDI Parent)
                if (this.MdiParent is Form1 parent)
                {
                    // Đảm bảo Form1.cs đã được sửa để đặt thuộc tính này là public/internal
                    parent.chứcNăngToolStripMenuItem.Enabled = true;
                    parent.đăngNhậpToolStripMenuItem.Enabled = false; // Vô hiệu hóa nút Đăng Nhập
                    parent.Text = $"Hệ Thống Bán Vé CGV - Chào mừng: {username}";
                }

                // 3. Đóng Form Đăng Nhập
                this.Close();
            }
            else
            {
                MessageBox.Show("Tài khoản hoặc Mật khẩu không chính xác.", "Lỗi Đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Clear();
                textBox2.Focus();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thoát (button2)
        /// </summary>
        private void buttonThoat_Click(object sender, EventArgs e)
        {
            // Thoát Form Đăng Nhập (Form con MDI)
            this.Close();
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào Quên Mật Khẩu (linkLabel1)
        /// </summary>
        private void linkLabelQuenMatKhau_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ quản trị viên để đặt lại mật khẩu.", "Hỗ trợ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // =======================================================
        //                 LOGIC NGHIỆP VỤ (Database)
        // =======================================================

        /// <summary>
        /// Hàm kiểm tra Tài khoản và Mật khẩu bằng Entity Framework.
        /// Sử dụng trường MatKhau mới được thêm vào Entity User.
        /// </summary>
        private bool KiemTraDangNhap(string username, string password)
        {
            // CẢNH BÁO: Mật khẩu nên được BĂM (Hash) trước khi lưu/so sánh.
            // Đoạn code này chỉ so sánh mật khẩu dạng chuỗi trần.
            try
            {
                using (var db = new Model1()) // Khởi tạo DbContext
                {
                    // Truy vấn tìm user dựa trên TenUser (không phân biệt chữ hoa/chữ thường) 
                    // và MatKhau (phân biệt chữ hoa/chữ thường)
                    var user = db.Users
                                 .FirstOrDefault(u => u.TenUser.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                                                      u.MatKhau == password);

                    return user != null;
                }
            }
            catch (Exception ex)
            {
                // Xử lý khi có lỗi kết nối database
                MessageBox.Show("Lỗi kết nối hoặc truy vấn cơ sở dữ liệu: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}