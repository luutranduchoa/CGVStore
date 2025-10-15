using System;
using System.Windows.Forms;
using CGVStore.BUS; // Cần thiết để truy cập lớp UserBUS
using CGVStore.Models;

namespace CGVStore
{
    public partial class Form4 : Form
    {
        // Khai báo và khởi tạo instance của lớp BUS
        private UserBUS userBUS = new UserBUS();

        public Form4()
        {
            InitializeComponent();

            // 1. Thiết lập cơ bản cho Form
            this.Text = "Quản Lý Người Dùng - Tạo Tài Khoản";
            this.Name = "FormTaoTaiKhoan";

            // 2. Thiết lập cho TextBox Mật Khẩu
            // Giả định: textBox2 là Mật khẩu, textBox3 là Xác nhận Mật khẩu
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
        /// Xử lý sự kiện khi nhấp vào nút Đăng Ký (Thêm User) (button1).
        /// Nhiệm vụ chính là gọi BUS và bắt lỗi.
        /// </summary>
        private void buttonDangKy_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu thô từ UI
            string username = textBox1.Text.Trim();
            string password = textBox2.Text; // Giữ nguyên khoảng trắng nếu có (mật khẩu)
            string confirmPassword = textBox3.Text; // Giữ nguyên khoảng trắng nếu có (mật khẩu)

            // Gọi hàm xử lý và bắt lỗi
            XuLyThemUser(username, password, confirmPassword);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thoát (button2)
        /// </summary>
        private void buttonThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // =======================================================
        //                 LOGIC GỌI BUS VÀ HIỂN THỊ THÔNG BÁO
        // =======================================================

        /// <summary>
        /// Gọi lớp BUS để thực hiện thêm User và hiển thị kết quả/lỗi.
        /// </summary>
        private void XuLyThemUser(string username, string password, string confirmPassword)
        {
            try
            {
                // Gọi hàm xử lý nghiệp vụ chính trong lớp BUS
                userBUS.XuLyThemUser(username, password, confirmPassword);

                // --- Xử lý khi thành công ---
                MessageBox.Show($"Đã tạo Tài khoản '{username}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Làm sạch Form sau khi thêm thành công
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox1.Focus();
            }
            // Bắt các lỗi do BUS ném ra để thông báo chính xác cho người dùng
            catch (ArgumentException ex)
            {
                // Lỗi dữ liệu rỗng hoặc mật khẩu không khớp
                MessageBox.Show(ex.Message, "Lỗi Nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Đặt focus tùy thuộc vào loại lỗi
                if (ex.Message.Contains("không được để trống"))
                    textBox1.Focus(); // Giả định lỗi rỗng thường bắt đầu ở ô đầu tiên
                else if (ex.Message.Contains("không khớp"))
                    textBox3.Focus(); // Đặt focus vào ô xác nhận mật khẩu
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi trùng tên tài khoản
                MessageBox.Show(ex.Message, "Lỗi Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
            }
            catch (Exception ex)
            {
                // Bắt lỗi hệ thống hoặc database (từ DAL)
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi Database/Hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}