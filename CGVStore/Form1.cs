using System;
using System.Windows.Forms;
using CGVStore.Models; // Để sử dụng các models nếu cần
using CGVStore;

namespace CGVStore
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // 1. Thiết lập Form1 làm MDI Container
            this.IsMdiContainer = true;

            // 2. Gán các sự kiện Click cho ToolStripMenuItem
            this.đăngNhậpToolStripMenuItem.Click += đăngNhậpToolStripMenuItem_Click;
            this.thoátToolStripMenuItem.Click += thoátToolStripMenuItem_Click;
            this.bánVéToolStripMenuItem.Click += bánVéToolStripMenuItem_Click;
            this.xemDoanhThuToolStripMenuItem.Click += xemDoanhThuToolStripMenuItem_Click;
            this.báoCáoThốngKêToolStripMenuItem.Click += báoCáoThốngKêToolStripMenuItem_Click;

            // Tùy chọn: Khóa các chức năng chính cho đến khi đăng nhập
            this.chứcNăngToolStripMenuItem.Enabled = false;
        }

        // =======================================================
        //                 HỆ THỐNG
        // =======================================================

        /// <summary>
        /// Mở Form Đăng Nhập (Form2)
        /// </summary>
        private void đăngNhậpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem Form2 đã được mở chưa để tránh mở nhiều lần
            foreach (Form f in this.MdiChildren)
            {
                if (f.Name == "FormDangNhap")
                {
                    f.Activate();
                    return;
                }
            }

            // Tạo và hiển thị Form Đăng Nhập (Form2)H
            try
            {
                Form FormDangNhap = new Form2(); // Form2: Đăng nhập
                FormDangNhap.Name = "FormDangNhap";
                FormDangNhap.Text = "Đăng Nhập Hệ Thống";
                FormDangNhap.MdiParent = this;
                FormDangNhap.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở Form Đăng Nhập: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Thoát ứng dụng
        /// </summary>
        private void thoátToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát ứng dụng không?",
                "Xác nhận Thoát",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // =======================================================
        //                 CHỨC NĂNG
        // =======================================================

        /// <summary>
        /// Mở Form Chính Quản Lý Bán Vé (Form5)
        /// </summary>
        private void bánVéToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Form5: form chính quản lý bán vé
            OpenChildForm(typeof(Form5), "FormBanVeChinh", "Quản Lý Bán Vé Chính");
        }

        /// <summary>
        /// Mở Form Khu Vực (Form3)
        /// </summary>
        private void xemDoanhThuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6 revenueForm = new Form6();
            revenueForm.StartPosition = FormStartPosition.CenterScreen; // Đặt giữa màn hình
            revenueForm.ShowDialog(); // Hoặc Show() tùy thuộc vào yêu cầu của bạn
        }

        /// <summary>
        /// Mở Form Tạo Tài Khoản Mới (Form4)
        /// </summary>
        private void báoCáoThốngKêToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form7 statisticsForm = new Form7();
            statisticsForm.StartPosition = FormStartPosition.CenterScreen; // Đặt giữa màn hình
            statisticsForm.ShowDialog(); // Mở Form ở dạng modal
        }

        // =======================================================
        //                 HÀM HỖ TRỢ
        // =======================================================

        /// <summary>
        /// Hàm chung để mở các Form con trong MDI Parent
        /// </summary>
        /// <param name="formType">Kiểu (Type) của Form cần mở.</param>
        /// <param name="formName">Tên định danh của Form.</param>
        /// <param name="formText">Tiêu đề của Form.</param>
        private void OpenChildForm(Type formType, string formName, string formText)
        {
            // 1. Kiểm tra xem Form đã mở chưa
            foreach (Form f in this.MdiChildren)
            {
                if (f.Name == formName)
                {
                    f.Activate();
                    return;
                }
            }

            // 2. Kiểm tra xem chức năng đã được mở khóa chưa
            if (!this.chứcNăngToolStripMenuItem.Enabled)
            {
                MessageBox.Show("Vui lòng đăng nhập trước khi sử dụng các chức năng này.", "Truy cập bị từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Tạo và hiển thị Form mới
            try
            {
                Form childForm = (Form)Activator.CreateInstance(formType);
                childForm.Name = formName;
                childForm.Text = formText;
                childForm.MdiParent = this;
                childForm.Show();
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu thiếu file Form con
                MessageBox.Show($"Lỗi khi mở Form {formName}. Bạn đã tạo Form này chưa? Lỗi chi tiết: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}