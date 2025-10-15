using CGVStore.DAL; // Cần tham chiếu đến DAL Project để sử dụng RevenueDAL và các DTO
using CGVStore.Models; // Cần tham chiếu đến Entity Models
using System;
using System.Collections.Generic;
using System.Linq;
using static CGVStore.DAL.RevenueDAL;

namespace CGVStore.BUS
{
    public class RevenueBUS
    {
        // Khai báo instance của lớp DAL để giao tiếp với database
        private RevenueDAL revenueDAL = new RevenueDAL();

        // =======================================================
        //                 DOANH THU CHI TIẾT (Form6)
        // =======================================================

        /// <summary>
        /// Lấy dữ liệu chi tiết Hóa Đơn đã được tổng hợp (MaHD, Tên KH, Tổng Tiền, Số Lượng Ghế).
        /// </summary>
        /// <returns>List chứa thông tin chi tiết hóa đơn (RevenueDetailDTO).</returns>
        public List<DAL.RevenueDAL.RevenueDetailDTO> LayDuLieuDoanhThuChiTiet()
        {
            // Logic nghiệp vụ trong BUS có thể bao gồm:
            // 1. Gọi hàm từ DAL để lấy dữ liệu đã JOIN/tổng hợp.
            var revenueData = revenueDAL.GetRevenueDetail();

            // 2. Xử lý thêm nếu cần (Ví dụ: tính thêm % khuyến mãi, sắp xếp lại, lọc theo điều kiện phức tạp)
            // ...

            return revenueData;
        }

        // =======================================================
        //                 THỐNG KÊ THEO THỜI GIAN (Form7)
        // =======================================================

        /// <summary>
        /// Lấy dữ liệu thống kê theo tháng (Doanh thu, Ghế, Khách hàng).
        /// </summary>
        /// <returns>Danh sách DTO/ViewModel cho báo cáo thống kê.</returns>
        public List<DAL.RevenueDAL.StatisticalDTO> LayDuLieuThongKeTheoThang()
        {
            // Lấy toàn bộ dữ liệu Hóa Đơn thô từ DAL (đã có Include KhachHang)
            var rawData = revenueDAL.GetHoaDonRawData();

            // Thực hiện tính toán thống kê (GROUP BY) trên bộ nhớ (LINQ to Objects)
            var statisticalData = rawData
                .Where(hd => hd.NgayMua.HasValue) // Chỉ lấy hóa đơn có ngày mua
                .GroupBy(hd => new {
                    Year = hd.NgayMua.Value.Year,
                    Month = hd.NgayMua.Value.Month
                })
                .Select(g => new DAL.RevenueDAL.StatisticalDTO
                {
                    // Định dạng tháng thành "MM/YYYY"
                    Thang = $"{g.Key.Month:00}/{g.Key.Year}",

                    // Tính Tổng Doanh Thu (cần ép kiểu (decimal) nếu TongTien trong Entity là double/float)
                    TongDoanhThu = (decimal)g.Sum(h => h.TongTien ?? 0f),                    // Tính số khách hàng duy nhất (đếm số MaKH khác nhau)
                    SoLuongKhach = g.Select(h => h.MaKH).Distinct().Count(),

                    // Gọi DAL để tính tổng số ghế đã bán trong tháng đó (vì cần truy cập bảng ChiTiet)
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
            // Lấy 3 danh sách dữ liệu thô từ DAL và đóng gói vào container
            return new ReportDataContainer
            {
                HoaDonList = revenueDAL.GetHoaDonRaw(),
                ChiTietList = revenueDAL.GetChiTietRaw(),
                KhachHangList = revenueDAL.GetKhachHangRaw()
            };
        }
        // Thêm vào lớp RevenueBUS
        public int GetGrandTotalDistinctCustomers()
        {
            

            using (Model1 db = new Model1()) // Cần đảm bảo có using CGVStore.Models;
            {
                return db.HoaDons
                         .Select(hd => hd.MaKH)
                         .Distinct()
                         .Count();
            }
        }
        public List<HoaDonSearchResultDTO> TimKiemHoaDonTheoTenKhachHang(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<HoaDonSearchResultDTO>();
            }

            // Gọi DAL. Phương thức này sẽ thực hiện truy vấn với .Contains()
            return revenueDAL.TimKiemHoaDonTheoTenKhachHangDAL(keyword);
        }
    }
}