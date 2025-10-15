using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using CGVStore.BUS; // Cần tham chiếu đến Project BUS
using System.IO;

namespace CGVStore
{
    public partial class Form7 : Form
    {
        // Khai báo đối tượng BUS
        private readonly RevenueBUS revenueBUS = new RevenueBUS();

        public Form7()
        {
            InitializeComponent();
            this.Load += Form7_Load;
            this.Text = "BÁO CÁO THỐNG KÊ DOANH THU THEO THÁNG";

            // Gán tên cột (Giữ nguyên cấu hình từ Designer)
            if (dataGridView1.Columns.Count >= 4)
            {
                dataGridView1.Columns[0].Name = "colThang";
                dataGridView1.Columns[1].Name = "colTongDoanhThu";
                dataGridView1.Columns[2].Name = "colSoLuongGhe";
                dataGridView1.Columns[3].Name = "colSoLuongKhach";
            }
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            // Reset cột về trạng thái thống kê mặc định
            ResetDataGridViewHeadersForStatisticalData();
            LoadStatisticalData();
        }

        /// <summary>
        /// Tải dữ liệu thống kê theo tháng từ RevenueBUS.
        /// </summary>
        private void LoadStatisticalData()
        {
            try
            {
                // Gọi BUS để lấy dữ liệu đã được nhóm/tính toán (Sử dụng dynamic/var)
                var monthlyStatistics = revenueBUS.LayDuLieuThongKeTheoThang();

                dataGridView1.Rows.Clear();

                // --- HIỂN THỊ DỮ LIỆU ---
                foreach (var month in monthlyStatistics)
                {
                    // Fix 1: Bỏ GetValueOrDefault() vì giả định TongDoanhThu trong DTO là non-nullable decimal sau khi aggregation
                    dataGridView1.Rows.Add(
                        month.Thang,
                        month.TongDoanhThu.ToString("N0") + " VNĐ", // Đã sửa
                        month.SoLuongGhe,
                        month.SoLuongKhach
                    );
                }

                // --- THÊM DÒNG TỔNG CỘNG ---
                if (monthlyStatistics.Any())
                {
                    // Fix 2: Thay decimal? thành decimal cho tổng cộng để tránh lỗi (vì Sum trên list non-nullable trả về non-nullable)
                    decimal grandTotalRevenue = monthlyStatistics.Sum(x => x.TongDoanhThu);
                    int grandTotalSeats = monthlyStatistics.Sum(x => x.SoLuongGhe);

                    // Giữ nguyên: Chờ phương thức được thêm vào BUS
                    int grandTotalCustomers = revenueBUS.GetGrandTotalDistinctCustomers();

                    dataGridView1.Rows.Add(
                        "TỔNG CỘNG",
                        grandTotalRevenue.ToString("N0") + " VNĐ", // Đã sửa
                        grandTotalSeats,
                        grandTotalCustomers
                    );

                    // Định dạng dòng Tổng cộng
                    var lastRow = dataGridView1.Rows[dataGridView1.Rows.Count - 1];
                    lastRow.DefaultCellStyle.BackColor = System.Drawing.Color.LightSalmon;
                    lastRow.DefaultCellStyle.Font = new System.Drawing.Font(dataGridView1.Font, System.Drawing.FontStyle.Bold);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu thống kê: " + ex.Message, "Lỗi Nghiệp Vụ/Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Thiết lập lại tiêu đề cột cho chế độ xem thống kê.
        /// </summary>
        private void ResetDataGridViewHeadersForStatisticalData()
        {
            if (dataGridView1.Columns.Count >= 4)
            {
                dataGridView1.Columns[0].HeaderText = "Tháng/Năm";
                dataGridView1.Columns[1].HeaderText = "Tổng Doanh Thu (VNĐ)";
                dataGridView1.Columns[2].HeaderText = "Tổng Số Ghế";
                dataGridView1.Columns[3].HeaderText = "Tổng Số Khách";
            }
        }

        private void buttonExportToExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Gọi BUS để lấy dữ liệu thô phục vụ Report Viewer
                var reportData = revenueBUS.LayDuLieuThoChoBaoCao();

                // Xử lý định dạng ngày tháng sang string cho ReportDataSource
                var hoaDonListForReport = reportData.HoaDonList
                    .Select(h => new
                    {
                        h.MaHD,
                        h.MaKH,
                        NgayMua = h.NgayMua.HasValue ? h.NgayMua.Value.ToString("dd/MM/yyyy") : "",
                        h.TongTien
                    })
                    .ToList();


                // --- Tạo LocalReport tạm để render file Excel ---
                LocalReport report = new LocalReport();
                report.ReportEmbeddedResource = "CGVStore.Reports.ReportHoaDon.rdlc";

                report.DataSources.Clear();
                report.DataSources.Add(new ReportDataSource("DataSetHoaDon", hoaDonListForReport));
                report.DataSources.Add(new ReportDataSource("DataSetHoaDonChiTiet", reportData.ChiTietList));
                report.DataSources.Add(new ReportDataSource("DataSetKhachHang", reportData.KhachHangList));

                // --- Render sang Excel ---
                byte[] bytes = report.Render(format: "EXCELOPENXML");

                // --- Hỏi người dùng nơi lưu ---
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                    sfd.FileName = "BaoCaoHoaDon.xlsx";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(sfd.FileName, bytes);
                        MessageBox.Show("Xuất báo cáo Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonTimKiem_Click(object sender, EventArgs e)
        {
            string keyword = textBoxTimKiem.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng cần tìm!", "Thông báo",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Nếu rỗng, tải lại dữ liệu thống kê mặc định
                ResetDataGridViewHeadersForStatisticalData();
                LoadStatisticalData();
                return;
            }

            try
            {
                // Gọi BUS để thực hiện tìm kiếm chi tiết hóa đơn theo tên khách hàng
                var result = revenueBUS.TimKiemHoaDonTheoTenKhachHang(keyword);

                // Cập nhật tiêu đề cột để hiển thị kết quả tìm kiếm chi tiết
                if (dataGridView1.Columns.Count >= 4)
                {
                    dataGridView1.Columns[0].HeaderText = "Mã HĐ";
                    dataGridView1.Columns[1].HeaderText = "Tổng Tiền (VNĐ)";
                    dataGridView1.Columns[2].HeaderText = "Ngày Mua";
                    dataGridView1.Columns[3].HeaderText = "Tên Khách Hàng";
                }

                dataGridView1.Rows.Clear();

                if (result.Any())
                {
                    foreach (var hd in result)
                    {
                        dataGridView1.Rows.Add(
                            hd.MaHD,
                             (hd.TongTien ?? 0m).ToString("N0") + " VNĐ",
                            hd.NgayMua.HasValue ? hd.NgayMua.Value.ToString("dd/MM/yyyy") : "",
                            hd.TenKH
                        );
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy khách hàng nào trùng tên \"" + keyword + "\"!",
                                     "Kết quả trống", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonThongTin_Click(object sender, EventArgs e)
        {
            FormReport frm = new FormReport();
            frm.ShowDialog();
        }

        private void buttonThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form7_Load_1(object sender, EventArgs e)
        {
            // No action needed here
        }
    }
}