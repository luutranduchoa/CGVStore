using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using CGVStore.BUS; // Cần tham chiếu đến Project BUS

namespace CGVStore
{
    public partial class Form6 : Form
    {
        // Khai báo đối tượng BUS
        private readonly RevenueBUS revenueBUS = new RevenueBUS();

        public Form6()
        {
            InitializeComponent();
            this.Load += Form6_Load;
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            LoadRevenueData();
        }

        /// <summary>
        /// Tải dữ liệu doanh thu chi tiết từ RevenueBUS lên DataGridView.
        /// </summary>
        private void LoadRevenueData()
        {
            try
            {
                // Gọi BUS để lấy dữ liệu đã được tổng hợp/xử lý nghiệp vụ.
                // Giả định RevenueBUS.LayDuLieuDoanhThuChiTiet() trả về một danh sách DTO/ViewModel
                // có các thuộc tính: TenKH, TongTien, NgayMua, SoLuongGhe.
                dynamic revenueData = revenueBUS.LayDuLieuDoanhThuChiTiet();

                // Xóa dữ liệu cũ
                dataGridView1.Rows.Clear();

                // Kiểm tra và thêm cột nếu chưa có (Tùy thuộc vào cấu hình của Form)
                if (dataGridView1.Columns.Count < 4 || dataGridView1.Columns[0].Name != "colTenKH")
                {
                    dataGridView1.Columns.Clear();
                    dataGridView1.Columns.Add("colTenKH", "Tên Khách Hàng");
                    dataGridView1.Columns.Add("colTongTien", "Tổng Tiền (VNĐ)");
                    dataGridView1.Columns.Add("colNgayMua", "Ngày Mua");
                    dataGridView1.Columns.Add("colSoLuongGhe", "Số Lượng Ghế");
                }


                // Biến tính tổng doanh thu
                double totalRevenue = 0;

                // Thêm dữ liệu vào DataGridView
                foreach (var item in revenueData)
                {
                    // Lấy giá trị tiền tệ (cần đảm bảo thuộc tính TongTien có sẵn)
                    double tongTien = (double)item.TongTien.GetValueOrDefault();
                    totalRevenue += tongTien;

                    dataGridView1.Rows.Add(
                        item.TenKH,
                        tongTien.ToString("N0") + " VNĐ", // Định dạng tiền tệ
                        ((DateTime)item.NgayMua).ToString("dd/MM/yyyy"), // Định dạng ngày
                        item.SoLuongGhe
                    );
                }

                // Tính và hiển thị Tổng Doanh Thu
                if (totalRevenue > 0)
                {
                    // Hiển thị tổng doanh thu ở cuối DGV
                    dataGridView1.Rows.Add(
                        "TỔNG DOANH THU",
                        totalRevenue.ToString("N0") + " VNĐ",
                        "",
                        ""
                    );
                    // Đánh dấu dòng tổng cộng
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.Font = new System.Drawing.Font(dataGridView1.Font, System.Drawing.FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi từ tầng BUS hoặc lỗi hiển thị
                MessageBox.Show("Lỗi khi tải dữ liệu doanh thu: " + ex.Message, "Lỗi Nghiệp Vụ/Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}