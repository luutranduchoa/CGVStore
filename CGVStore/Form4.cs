using System;
using System.Linq;
using System.Windows.Forms;
using CGVStore.Models; // Cần thiết để truy cập Entity User và DbContext Model1

namespace CGVStore
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();

            // 1. Thiết lập cơ bản cho Form
            this.Text = "Quản Lý Người Dùng - Tạo Tài Khoản";
            this.Name = "FormTaoTaiKhoan";

            // 2. Thiết lập cho TextBox Mật Khẩu
            this.textBox2.PasswordChar = '•';
            this.textBox3.PasswordChar = '•';

            // 3. Gán sự kiện Click cho các nút
            this.button1.Click += new EventHandler(buttonDangKy_Click);
            this.button2.Click += new EventHandler(buttonThoat_Click);

            // 4. Đặt focus vào ô Tài Khoản khi Form khởi động
            this.Load += (sender, e) => { textBox1.Focus(); };
        }

        // =======================================================
        //                 HÀM XỬ LÝ SỰ KIỆN
        // =======================================================

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Đăng Ký (Thêm User) (button1)
        /// </summary>
        private void buttonDangKy_Click(object sender, EventArgs e)
        {
            // 1. Lấy dữ liệu từ Form
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();
            string confirmPassword = textBox3.Text.Trim();

            // 2. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tài khoản, Mật khẩu và Xác nhận Mật khẩu.", "Lỗi Dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải chứa ít nhất 6 ký tự.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu và Xác nhận Mật khẩu không khớp.", "Lỗi Xác nhận", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.Clear();
                textBox3.Focus();
                return;
            }

            // 3. Gọi hàm thêm vào Database
            ThemUserVaoDatabase(username, password);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thoát (button2)
        /// </summary>
        private void buttonThoat_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng Form Tạo Tài Khoản (Form con MDI)
        }

        // =======================================================
        //                 LOGIC NGHIỆP VỤ (Database)
        // =======================================================

        /// <summary>
        /// Thực hiện thêm mới một User vào cơ sở dữ liệu
        /// </summary>
        private void ThemUserVaoDatabase(string username, string password)
        {
            try
            {
                using (var db = new Model1()) // Khởi tạo DbContext
                {
                    // 1. Kiểm tra TenUser đã tồn tại chưa
                    if (db.Users.Any(u => u.TenUser.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"Tài khoản '{username}' đã tồn tại. Vui lòng chọn tên khác.", "Lỗi Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox1.Focus();
                        return;
                    }

                    // 2. Tự động tìm MaUser lớn nhất và tăng thêm 1
                    int nextMaUser = 1;
                    if (db.Users.Any())
                    {
                        nextMaUser = db.Users.Max(u => u.MaUser) + 1;
                    }

                    // 3. Tạo đối tượng User mới
                    var newUser = new User
                    {
                        MaUser = nextMaUser,
                        TenUser = username,
                        MatKhau = password // LƯU Ý: Trong thực tế, cần mã hóa mật khẩu trước khi lưu
                    };

                    // 4. Thêm vào DbSet và lưu thay đổi
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show($"Đã tạo Tài khoản '{username}' thành công! (Mã User: {nextMaUser})", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 5. Làm sạch Form sau khi thêm thành công
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox1.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo Tài khoản vào cơ sở dữ liệu: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}