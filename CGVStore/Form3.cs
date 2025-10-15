using System;
using System.Windows.Forms;
using CGVStore.BUS; // Cần thiết để truy cập lớp AreaBUS

namespace CGVStore
{
    public partial class Form3 : Form
    {
        // Khai báo và khởi tạo instance của lớp BUS
        private AreaBUS areaBUS = new AreaBUS();

        public Form3()
        {
            InitializeComponent();

            // 1. Thiết lập cơ bản cho Form
            this.Text = "Quản Lý Khu Vực";
            this.Name = "FormKhuVuc";

            // 2. Gán sự kiện Click cho các nút
            this.button1.Click += new EventHandler(buttonThem_Click);
            this.button2.Click += new EventHandler(buttonThoat_Click);

            // 3. Đặt focus vào ô Mã Số khi Form khởi động
            this.Load += (sender, e) => { textBox1.Focus(); };

            // Loại bỏ dòng thừa: public Action<object, object> SaveAreaClick { get; internal set; }
        }

        // =======================================================
        //                 HÀM XỬ LÝ SỰ KIỆN
        // =======================================================

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thêm (button1).
        /// Nhiệm vụ chính là gọi BUS và bắt lỗi.
        /// </summary>
        private void buttonThem_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu thô từ UI
            string areaIDText = textBox1.Text.Trim();
            string areaName = textBox2.Text.Trim();

            // GỌI BUS ĐỂ XỬ LÝ
            XuLyThemKhuVuc(areaIDText, areaName);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thoát (button2)
        /// </summary>
        private void buttonThoat_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng Form Khu Vực
        }

        // =======================================================
        //                 LOGIC GỌI BUS VÀ HIỂN THỊ THÔNG BÁO
        // =======================================================

        /// <summary>
        /// Gọi lớp BUS để thực hiện thêm Khu Vực và hiển thị kết quả/lỗi.
        /// </summary>
        private void XuLyThemKhuVuc(string areaIDText, string areaName)
        {
            try
            {
                // Gọi hàm xử lý nghiệp vụ chính trong lớp BUS
                areaBUS.XuLyThemKhuVuc(areaIDText, areaName);

                // --- Xử lý khi thành công ---
                MessageBox.Show($"Đã thêm Khu Vực: {areaName} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Làm sạch Form sau khi thêm thành công
                textBox1.Clear();
                textBox2.Clear();
                textBox1.Focus();
            }
            // Bắt các lỗi do BUS ném ra để thông báo chính xác cho người dùng
            catch (ArgumentException ex)
            {
                // Lỗi dữ liệu rỗng hoặc số không dương
                MessageBox.Show(ex.Message, "Lỗi Nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Xác định focus dựa trên lỗi
                if (ex.Message.Contains("Mã số") || ex.Message.Contains("dương"))
                    textBox1.Focus();
                else
                    textBox2.Focus();
            }
            catch (FormatException ex)
            {
                // Lỗi ép kiểu từ chuỗi sang số nguyên (Mã Khu Vực không phải là số)
                MessageBox.Show(ex.Message, "Lỗi Định dạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Focus();
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi trùng lặp ID
                MessageBox.Show(ex.Message, "Lỗi Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
            }
            catch (Exception ex)
            {
                // Bắt lỗi chung (ví dụ: lỗi Database/kết nối từ DAL)
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi Database/Hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}