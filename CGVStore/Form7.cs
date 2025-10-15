using CGVStore.Models;
using Microsoft.Reporting.WinForms;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CGVStore
{
    public partial class Form7 : Form
    {
        public Form7()
        {
            InitializeComponent();
            this.Load += Form7_Load;
            this.Text = "BÁO CÁO THỐNG KÊ DOANH THU THEO THÁNG";

            // Gán lại tên cột cho dễ quản lý code (nên làm trong constructor)
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
            LoadStatisticalData();
        }

        /// <summary>
        /// Tải dữ liệu thống kê (Doanh thu, Ghế, Khách hàng) theo tháng.
        /// Sử dụng phương pháp tải dữ liệu thô và tính toán trong bộ nhớ (LINQ to Objects) để tránh lỗi EF.
        /// </summary>
        private void LoadStatisticalData()
        {
            try
            {
                using (var db = new Model1())
                {
                    // --- BƯỚC 1: TRUY VẤN VÀ TẢI DỮ LIỆU THÔ VÀO BỘ NHỚ ---
                    var rawData = (from hd in db.HoaDons
                                   where hd.NgayMua.HasValue
                                   select new
                                   {
                                       NgayMua = hd.NgayMua.Value,
                                       hd.MaKH,
                                       hd.MaHD,
                                       hd.TongTien,
                                       // Tính số lượng ghế cho từng hóa đơn ngay trong truy vấn DB
                                       // (Sử dụng subquery an toàn)
                                       SoGhe = db.ChiTiets.Count(ct => ct.MaHD == hd.MaHD && ct.MaKH == hd.MaKH)
                                   })
                                  .ToList(); // <-- EF thực thi TẠI ĐÂY (Chuyển sang LINQ to Objects)

                    // --- BƯỚC 2: NHÓM VÀ TÍNH TOÁN TRONG BỘ NHỚ C# (AN TOÀN HƠN) ---
                    var monthlyStatistics = rawData
                        .GroupBy(x => new
                        {
                            Month = x.NgayMua.Month,
                            Year = x.NgayMua.Year
                        })
                        .Select(g => new
                        {
                            ThangNam = g.Key.Month + "/" + g.Key.Year,
                            TongDoanhThu = g.Sum(x => x.TongTien),
                            SoLuongGhe = g.Sum(x => x.SoGhe),
                            // Tính số lượng khách hàng riêng biệt trong nhóm (LINQ to Objects)
                            customerCount = g.Select(x => x.MaKH).Distinct().Count()
                        })
                        .OrderBy(x => x.ThangNam) // Sắp xếp theo thứ tự thời gian
                        .ToList();


                    dataGridView1.Rows.Clear();

                    // --- BƯỚC 3: HIỂN THỊ DỮ LIỆU ---
                    foreach (var month in monthlyStatistics)
                    {
                        dataGridView1.Rows.Add(
                            month.ThangNam,
                            month.TongDoanhThu.GetValueOrDefault().ToString("N0") + " VNĐ",
                            month.SoLuongGhe,
                            month.customerCount
                        );
                    }

                    // --- BƯỚC 4: THÊM DÒNG TỔNG CỘNG ---
                    if (monthlyStatistics.Any())
                    {
                        // Tính toán tổng cộng từ dữ liệu thô (raw data)
                        double? grandTotalRevenue = monthlyStatistics.Sum(x => x.TongDoanhThu);
                        int grandTotalSeats = rawData.Sum(x => x.SoGhe);
                        int grandTotalCustomers = rawData.Select(x => x.MaKH).Distinct().Count();

                        dataGridView1.Rows.Add(
                            "TỔNG CỘNG",
                            grandTotalRevenue.GetValueOrDefault().ToString("N0") + " VNĐ",
                            grandTotalSeats,
                            grandTotalCustomers
                        );

                        // Định dạng dòng Tổng cộng
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.BackColor = System.Drawing.Color.LightSalmon;
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.Font = new System.Drawing.Font(dataGridView1.Font, System.Drawing.FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex)
            {
                // Giữ lại MessageBox để bắt lỗi nếu có vấn đề khác (ví dụ: lỗi kết nối DB)
                MessageBox.Show("Lỗi khi tải dữ liệu thống kê: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form7_Load_1(object sender, EventArgs e)
        {

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

        private void buttonExportToExcel_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = new Model1())
                {
                    // --- Lấy dữ liệu từ DB giống như trong FormReport ---
                    var hoaDonRaw = db.HoaDons
                        .Select(h => new { h.MaHD, h.MaKH, h.NgayMua, h.TongTien })
                        .ToList();

                    var hoaDonList = hoaDonRaw
                        .Select(h => new
                        {
                            h.MaHD,
                            h.MaKH,
                            NgayMua = h.NgayMua.HasValue ? h.NgayMua.Value.ToString("dd/MM/yyyy") : "",
                            h.TongTien
                        })
                        .ToList();

                    var chiTietList = db.ChiTiets
                        .Select(ct => new { ct.MaHD, ct.MaKH, ct.SoGheNgoi })
                        .ToList();
                    var khachHangList = db.KhachHangs
                       .Select(kh => new { kh.MaKH, kh.TenKH, kh.DiaChi, kh.SDT })
                       .ToList();

                    // --- Tạo LocalReport tạm để render file Excel ---
                    LocalReport report = new LocalReport();
                    report.ReportEmbeddedResource = "CGVStore.Reports.ReportHoaDon.rdlc";

                    report.DataSources.Clear();
                    report.DataSources.Add(new ReportDataSource("DataSetHoaDon", hoaDonList));
                    report.DataSources.Add(new ReportDataSource("DataSetHoaDonChiTiet", chiTietList));
                    report.DataSources.Add(new ReportDataSource("DataSetKhachHang", khachHangList));

                    // --- Render sang Excel ---
                    byte[] bytes = report.Render(format: "EXCELOPENXML"); // -> .xlsx format

                    // --- Hỏi người dùng nơi lưu ---
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                        sfd.FileName = "BaoCaoHoaDon.xlsx";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            System.IO.File.WriteAllBytes(sfd.FileName, bytes);
                            MessageBox.Show("Xuất báo cáo Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
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
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    // Tìm hóa đơn theo tên khách hàng (bỏ qua hoa đơn không có khách)
                    var result = (from hd in db.HoaDons
                                  where hd.KhachHang != null &&
                                        hd.KhachHang.TenKH.Contains(keyword)
                                  select new
                                  {
                                      hd.MaHD,
                                      hd.KhachHang.TenKH,
                                      hd.NgayMua,
                                      hd.TongTien
                                  }).ToList();

                    // Hiển thị kết quả
                    dataGridView1.Rows.Clear();

                    if (result.Any())
                    {
                        foreach (var hd in result)
                        {
                            dataGridView1.Rows.Add(
                                hd.MaHD,
                                hd.TongTien.GetValueOrDefault().ToString("N0") + " VNĐ",
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}