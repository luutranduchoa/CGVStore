using System;
using System.Linq;
using System.Windows.Forms;
using CGVStore.Models;
using System.Data.Entity; // Cần thiết cho Include()

namespace CGVStore
{
    public partial class Form6 : Form
    {
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
        /// Tải dữ liệu doanh thu từ bảng HoaDon và ChiTiet lên DataGridView.
        /// </summary>
        private void LoadRevenueData()
        {
            try
            {
                using (var db = new Model1())
                {
                    // 1. Truy vấn các Hóa Đơn và Khách Hàng liên quan
                    var revenueData = db.HoaDons
                        .Include(hd => hd.KhachHang) // Lấy thông tin khách hàng
                        .Select(hd => new
                        {
                            MaHD = hd.MaHD,
                            MaKH = hd.MaKH,
                            TenKH = hd.KhachHang.TenKH,
                            TongTien = hd.TongTien,
                            NgayMua = hd.NgayMua,

                            // 2. Subquery để tính tổng số ghế của mỗi hóa đơn
                            SoLuongGhe = db.ChiTiets
                                           .Count(ct => ct.MaHD == hd.MaHD && ct.MaKH == hd.MaKH)
                        })
                        .OrderByDescending(x => x.NgayMua)
                        .ToList();

                    // Xóa dữ liệu cũ
                    dataGridView1.Rows.Clear();

                    // Thêm dữ liệu vào DataGridView
                    foreach (var item in revenueData)
                    {
                        dataGridView1.Rows.Add(
                            item.TenKH,
                            item.TongTien.GetValueOrDefault().ToString("N0") + " VNĐ", // Định dạng tiền tệ
                            item.NgayMua.GetValueOrDefault().ToString("dd/MM/yyyy"), // Định dạng ngày
                            item.SoLuongGhe
                        );
                    }

                    // Tính Tổng Doanh Thu
                    double? totalRevenue = revenueData.Sum(x => x.TongTien);
                    if (totalRevenue.HasValue)
                    {
                        // Hiển thị tổng doanh thu ở cuối DGV hoặc trong một Label riêng
                        // Ở đây, tôi sẽ thêm nó vào cuối DGV để làm báo cáo tổng hợp đơn giản.
                        dataGridView1.Rows.Add(
                            "TỔNG DOANH THU",
                            totalRevenue.Value.ToString("N0") + " VNĐ",
                            "",
                            ""
                        );
                        // Đánh dấu dòng tổng cộng (ví dụ: in đậm hoặc tô màu)
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.Font = new System.Drawing.Font(dataGridView1.Font, System.Drawing.FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu doanh thu: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}