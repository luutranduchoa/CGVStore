using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CGVStore.Models;

namespace CGVStore.DAL
{
    public class RevenueDAL
    {
        // =======================================================
        //                 DOANH THU CHI TIẾT (Form6)
        // =======================================================

        /// <summary>
        /// Lấy dữ liệu chi tiết Hóa Đơn, bao gồm tên khách hàng và tính toán số lượng ghế đã bán trong cùng truy vấn.
        /// </summary>
        /// <returns>List DTO/ViewModel chứa dữ liệu đã tổng hợp.</returns>
        public List<RevenueDetailDTO> GetRevenueDetail()
        {
            using (var db = new Model1())
            {
                // Tái tạo logic truy vấn từ Form6.cs
                var revenueData = db.HoaDons
                    .Include(hd => hd.KhachHang) // Lấy thông tin khách hàng
                    .Select(hd => new RevenueDetailDTO
                    {
                        MaHD = hd.MaHD,
                        TenKH = hd.KhachHang.TenKH,
                        TongTien = (decimal)(hd.TongTien ?? 0f),
                        NgayMua = hd.NgayMua,

                        // Subquery để tính tổng số ghế của mỗi hóa đơn
                        SoLuongGhe = db.ChiTiets
                                       .Count(ct => ct.MaHD == hd.MaHD)
                    })
                    .OrderByDescending(d => d.NgayMua)
                    .ToList();

                return revenueData;
            }
        }

        // =======================================================
        //                 THỐNG KÊ THEO THỜI GIAN (Form7)
        // =======================================================

        /// <summary>
        /// Lấy toàn bộ dữ liệu Hóa Đơn cho BUS xử lý thống kê.
        /// </summary>
        public List<HoaDon> GetHoaDonRawData()
        {
            using (var db = new Model1())
            {
                // Dùng .Include() để tránh N+1 Select khi BUS truy cập KhachHang
                return db.HoaDons.Include(hd => hd.KhachHang).ToList();
            }
        }

        /// <summary>
        /// Tính tổng số ghế đã bán trong một tháng/năm cụ thể.
        /// (Hàm này được BUS gọi để bổ sung dữ liệu thống kê)
        /// </summary>
        public int GetTotalSeatsForMonth(int year, int month)
        {
            using (var db = new Model1())
            {
                // Tìm MaHD của các hóa đơn trong tháng đó
                var maHDs = db.HoaDons
                            .Where(hd => hd.NgayMua.HasValue &&
                                         hd.NgayMua.Value.Year == year &&
                                         hd.NgayMua.Value.Month == month)
                            .Select(hd => hd.MaHD)
                            .ToList();

                // Đếm Chi Tiết (ghế) dựa trên danh sách MaHD
                return db.ChiTiets.Count(ct => maHDs.Contains(ct.MaHD));
            }
        }
        // KHÁI NIỆM: Logic này nằm trong file RevenueDAL.cs

        public List<HoaDonSearchResultDTO> TimKiemHoaDonTheoTenKhachHangDAL(string keyword)
        {
            using (var db = new Model1())
            {
                // Sử dụng .Contains(keyword) để thực hiện tìm kiếm một phần
                var result = db.HoaDons
                               .Where(hd => hd.KhachHang != null &&
                                            hd.KhachHang.TenKH.Contains(keyword)) // <--- Đây là logic tìm kiếm một phần
                               .Select(hd => new HoaDonSearchResultDTO
                               {
                                   MaHD = hd.MaHD,
                                   TenKH = hd.KhachHang.TenKH,
                                   NgayMua = hd.NgayMua,
                                   TongTien = (decimal?)hd.TongTien
                               })
                               .ToList();
                return result;
            }
        }

        // =======================================================
        //                 DỮ LIỆU THÔ CHO BÁO CÁO (FormReport)
        // =======================================================

        public List<HoaDon> GetHoaDonRaw()
        {
            using (var db = new Model1())
            {
                return db.HoaDons.ToList();
            }
        }

        public List<ChiTiet> GetChiTietRaw()
        {
            using (var db = new Model1())
            {
                // Giả định ChiTiet là tên Entity
                return db.ChiTiets.ToList();
            }
        }

        public List<KhachHang> GetKhachHangRaw()
        {
            using (var db = new Model1())
            {
                return db.KhachHangs.ToList();
            }
        }
        public class RevenueDetailDTO
        {
            public int MaHD { get; set; }
            public string TenKH { get; set; }
            public decimal TongTien { get; set; }
            public DateTime? NgayMua { get; set; }
            public int SoLuongGhe { get; set; }
        }

        public class StatisticalDTO
        {
            public string Thang { get; set; } // Định dạng "MM/YYYY"
            public decimal TongDoanhThu { get; set; }
            public int SoLuongGhe { get; set; }
            public int SoLuongKhach { get; set; }
        }

        public class ReportDataContainer
        {
            public List<HoaDon> HoaDonList { get; set; }
            public List<ChiTiet> ChiTietList { get; set; }
            public List<KhachHang> KhachHangList { get; set; }
        }
        public class HoaDonSearchResultDTO
        {
            public int MaHD { get; set; }
            public string TenKH { get; set; } // Cần có Tên Khách Hàng để hiển thị
            public DateTime? NgayMua { get; set; }
            public decimal? TongTien { get; set; }
        }
    }
}