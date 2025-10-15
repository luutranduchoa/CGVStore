using System;
using System.Collections.Generic;
using System.Linq;
using CGVStore.DAL; // Cần tham chiếu đến DAL Project
using CGVStore.Models; // Cần tham chiếu đến Entity Models

namespace CGVStore.BUS
{
    public class RevenueBUS
    {
        private RevenueDAL revenueDAL = new RevenueDAL();

        // =======================================================
        //                 DOANH THU CHI TIẾT (Form6)
        // =======================================================

        /// <summary>
        /// Lấy dữ liệu chi tiết Hóa Đơn và tính toán Số Lượng Ghế cho mỗi hóa đơn.
        /// </summary>
        /// <returns>List chứa thông tin chi tiết hóa đơn (DTO/ViewModel).</returns>
        public List<RevenueDetailDTO> LayDuLieuDoanhThuChiTiet()
        {
            // Giả định DAL trả về một List<HoaDon> đã được xử lý JOIN/INCLUDE.
            // Nếu DAL trả về dữ liệu thô, BUS sẽ xử lý việc tính toán.

            // Dựa trên Form6.cs, logic tính SoLuongGhe và JOIN KhachHang được thực hiện trong truy vấn LINQ.
            // Để đơn giản, giả định DAL đã trả về list các đối tượng Aggregated

            var revenueData = revenueDAL.GetRevenueDetail();

            // Logic nghiệp vụ: (Nếu cần tính toán thêm trên bộ nhớ RAM)
            // Ví dụ: tính thêm % lợi nhuận, phân loại khách hàng VIP...

            return revenueData;
        }

        // =======================================================
        //                 THỐNG KÊ THEO THỜI GIAN (Form7)
        // =======================================================

        /// <summary>
        /// Lấy dữ liệu thống kê theo tháng (Doanh thu, Ghế, Khách hàng).
        /// </summary>
        /// <returns>Danh sách DTO/ViewModel cho báo cáo thống kê.</returns>
        public List<StatisticalDTO> LayDuLieuThongKeTheoThang()
        {
            // Form7.cs sử dụng logic GROUP BY theo tháng (Year và Month)
            var rawData = revenueDAL.GetHoaDonRawData(); // Lấy tất cả Hóa Đơn và Chi Tiết từ DAL

            var statisticalData = rawData
                .Where(hd => hd.NgayMua.HasValue) // Chỉ lấy hóa đơn có ngày mua
                .GroupBy(hd => new { Year = hd.NgayMua.Value.Year, Month = hd.NgayMua.Value.Month })
                .Select(g => new StatisticalDTO
                {
                    Thang = $"{g.Key.Month}/{g.Key.Year}",
                    TongDoanhThu = g.Sum(h => h.TongTien.GetValueOrDefault()),
                    SoLuongKhach = g.Select(h => h.MaKH).Distinct().Count(), // Tính số khách hàng duy nhất
                    // Giả định DAL có hàm tính tổng số ghế cho một group/tháng
                    SoLuongGhe = revenueDAL.GetTotalSeatsForMonth(g.Key.Year, g.Key.Month)
                })
                .OrderBy(s => s.Thang)
                .ToList();

            return statisticalData;
        }

        // =======================================================
        //                 DỮ LIỆU THÔ CHO BÁO CÁO (FormReport)
        // =======================================================

        /// <summary>
        /// Lấy dữ liệu thô của Hóa Đơn, Khách Hàng, Chi Tiết để cung cấp cho Report Viewer.
        /// </summary>
        public ReportDataContainer LayDuLieuThoChoBaoCao()
        {
            return new ReportDataContainer
            {
                HoaDonList = revenueDAL.GetHoaDonRaw(),
                ChiTietList = revenueDAL.GetChiTietRaw(),
                KhachHangList = revenueDAL.GetKhachHangRaw()
            };
        }
    }
}
// ----------------------------------------------------------------------------------
// CÁC LỚP DATA TRANSFER OBJECT (DTO)/VIEWMODEL DÙNG TRONG BUS (Nên đặt trong 1 file riêng)
// ----------------------------------------------------------------------------------

// LƯU Ý: Các lớp DTO/ViewModel này nên được tạo trong một thư mục/namespace riêng biệt 
// (Ví dụ: CGVStore.Common.DTOs) để tránh phụ thuộc trực tiếp vào Entity Models.

