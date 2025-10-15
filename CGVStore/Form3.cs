using System;
using System.Linq;
using System.Windows.Forms;
using CGVStore.Models; // Cần thiết để truy cập Entity Area và DbContext Model1

namespace CGVStore
{
    public partial class Form3 : Form
    {
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

            // 4. Mã số (AreaID) không được tự động tạo, nên phải cho phép người dùng nhập.
            // Có thể thêm logic kiểm tra xem nó có phải là số hay không.
        }

        public Action<object, object> SaveAreaClick { get; internal set; }

        // =======================================================
        //                 HÀM XỬ LÝ SỰ KIỆN
        // =======================================================

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thêm (button1)
        /// </summary>
        private void buttonThem_Click(object sender, EventArgs e)
        {
            // 1. Lấy dữ liệu từ Form
            if (!int.TryParse(textBox1.Text.Trim(), out int areaID))
            {
                MessageBox.Show("Mã Số Khu Vực phải là một số nguyên hợp lệ.", "Lỗi Dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Focus();
                return;
            }

            string areaName = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(areaName))
            {
                MessageBox.Show("Tên Khu Vực không được để trống.", "Lỗi Dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Focus();
                return;
            }

            // 2. Gọi hàm thêm vào Database
            ThemKhuVucVaoDatabase(areaID, areaName);
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào nút Thoát (button2)
        /// </summary>
        private void buttonThoat_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng Form Khu Vực (Form con MDI)
        }

        // =======================================================
        //                 LOGIC NGHIỆP VỤ (Database)
        // =======================================================

        /// <summary>
        /// Thực hiện thêm mới một Khu Vực vào cơ sở dữ liệu
        /// </summary>
        private void ThemKhuVucVaoDatabase(int areaID, string areaName)
        {
            try
            {
                using (var db = new Model1()) // Khởi tạo DbContext
                {
                    // 1. Kiểm tra AreaID đã tồn tại chưa
                    if (db.Areas.Any(a => a.AreaID == areaID))
                    {
                        MessageBox.Show($"Mã Khu Vực ({areaID}) đã tồn tại. Vui lòng chọn mã khác.", "Lỗi Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox1.Focus();
                        return;
                    }

                    // 2. Tạo đối tượng Area mới
                    var newArea = new Area
                    {
                        AreaID = areaID,
                        AreaName = areaName
                    };

                    // 3. Thêm vào DbSet và lưu thay đổi
                    db.Areas.Add(newArea);
                    db.SaveChanges();

                    MessageBox.Show($"Đã thêm Khu Vực: {areaName} (ID: {areaID}) thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 4. Làm sạch Form sau khi thêm thành công
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox1.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm Khu Vực vào cơ sở dữ liệu: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}