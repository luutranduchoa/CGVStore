using Microsoft.Reporting.WinForms;
using System;
using System.Linq;
using System.Windows.Forms;
using CGVStore.Models;

namespace CGVStore
{
    public partial class FormReport : Form
    {
        public FormReport()
        {
            InitializeComponent();
        }

        private void FormReport_Load(object sender, EventArgs e)
        {
            try
            {
                using (var db = new Model1())
                {
                    // Lấy dữ liệu thô từ DB (chưa format)
                    var hoaDonRaw = db.HoaDons
                        .Select(h => new { h.MaHD, h.MaKH, h.NgayMua, h.TongTien })
                        .ToList();                     // <-- EF thực thi ở đây

                    // Sau khi đã ở bộ nhớ, mới format ngày/tháng
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

                    reportViewer1.LocalReport.ReportEmbeddedResource = "CGVStore.Reports.ReportHoaDon.rdlc";
                    reportViewer1.LocalReport.DataSources.Clear();
                    reportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WinForms.ReportDataSource("DataSetHoaDon", hoaDonList));
                    reportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WinForms.ReportDataSource("DataSetHoaDonChiTiet", chiTietList));
                    reportViewer1.LocalReport.DataSources.Add(
                        new Microsoft.Reporting.WinForms.ReportDataSource("DataSetKhachHang", khachHangList));
                    reportViewer1.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo: " + ex.Message);
            }
        }
    }
}
