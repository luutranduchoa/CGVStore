using System;
using System.Linq;
using System.Windows.Forms;
using CGVStore.BUS; // Cần thiết để truy cập DbContext (Model1) và Entity (User)

namespace CGVStore
{
    public partial class Form2 : Form
    {
        private UserBUS userBUS = new UserBUS();
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
            string password = textBox2.Text;

            try
            {
                // **GỌI BUS** để xử lý nghiệp vụ Đăng Nhập
                if (userBUS.XuLyDangNhap(username, password))
                {
                    MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Logic mở khóa chức năng trên Form1 (vẫn giữ ở UI)
                    if (this.MdiParent is Form1 mainForm)
                    {
                        mainForm.chứcNăngToolStripMenuItem.Enabled = true;
                        mainForm.đăngNhậpToolStripMenuItem.Enabled = false;
                    }
                    this.Close();
                }
                else
                {
                    // Lỗi trả về từ DAL/BUS (Sai tài khoản/mật khẩu)
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Focus();
                }
            }
            catch (ArgumentException ex) // Bắt lỗi nghiệp vụ (vd: thiếu input)
            {
                MessageBox.Show(ex.Message, "Lỗi Nhập Liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex) // Bắt lỗi khác (vd: lỗi kết nối database)
            {
                MessageBox.Show("Đã xảy ra lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}