using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CGVStore.Models; // Cần thiết để truy cập Entity Models và DbContext (Model1)

namespace CGVStore.DAL
{
    public class TicketDAL
    {
        // =======================================================
        //                 DỮ LIỆU TẢI (LOAD DATA)
        // =======================================================

        /// <summary>
        /// Lấy danh sách mã ghế đã bán từ bảng ChiTiet.
        /// </summary>
        public List<string> LayDanhSachGheDaBan()
        {
            using (var db = new Model1())
            {
                // 1. Lấy giá trị SoGheNgoi (int?)
                // 2. Ép kiểu sang string bằng cách gọi .ToString()
                //    (Vì là int? nên dùng .Value để lấy giá trị int, sau đó ToString())
                return db.ChiTiets
                         .Select(ct => ct.SoGheNgoi.Value.ToString())
                         .ToList();
            }
        }

        /// <summary>
        /// Lấy danh sách Hóa Đơn kèm theo tên Khách Hàng cho DGV.
        /// Giả định mô hình dữ liệu cần trả về đối tượng dễ hiển thị.
        /// </summary>
        public object LayDanhSachHoaDonView()
        {
            using (var db = new Model1())
            {
                // Truy vấn kết hợp HoaDon và KhachHang
                var result = db.HoaDons
                    .Include(hd => hd.KhachHang) // Sử dụng Include để load KhachHang liên quan
                    .OrderByDescending(hd => hd.NgayMua)
                    .Select(hd => new
                    {
                        MaHD = hd.MaHD,
                        MaKH = hd.MaKH,
                        TenKhachHang = hd.KhachHang.TenKH, // Lấy tên Khách hàng
                        SDT = hd.KhachHang.SDT,
                        TongTien = hd.TongTien.GetValueOrDefault(),
                        NgayMua = hd.NgayMua
                    })
                    .ToList();
                return result;
            }
        }

        // =======================================================
        //                  THAO TÁC GHI (CRUD)
        // =======================================================

        /// <summary>
        /// Tìm Khách Hàng theo SĐT, nếu không tồn tại thì tạo Khách Hàng mới.
        /// </summary>
        /// <returns>MaKH (int) của khách hàng.</returns>
        public int GetOrCreateKhachHang(KhachHang khachHang)
        {
            using (var db = new Model1())
            {
                // 1. Tìm khách hàng theo SĐT (giả định SĐT là duy nhất)
                var existingKH = db.KhachHangs.FirstOrDefault(kh => kh.SDT == khachHang.SDT);

                if (existingKH != null)
                {
                    return existingKH.MaKH;
                }

                // 2. Nếu không có, thêm khách hàng mới
                db.KhachHangs.Add(khachHang);
                db.SaveChanges(); // Lưu để lấy MaKH tự động tạo

                return khachHang.MaKH;
            }
        }

        /// <summary>
        /// Tạo mới một Hóa Đơn và trả về MaHD.
        /// </summary>
        /// <returns>MaHD (int) của hóa đơn vừa tạo.</returns>
        public int TaoHoaDon(HoaDon hoaDon)
        {
            using (var db = new Model1())
            {
                db.HoaDons.Add(hoaDon);
                db.SaveChanges(); // Lưu để MaHD được tạo tự động
                return hoaDon.MaHD;
            }
        }

        /// <summary>
        /// Thêm danh sách Chi Tiết Hóa Đơn (ghế ngồi).
        /// </summary>
        public void ThemChiTiet(List<ChiTiet> chiTiets)
        {
            using (var db = new Model1())
            {
                db.ChiTiets.AddRange(chiTiets);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa Hóa Đơn và các Chi Tiết liên quan.
        /// </summary>
        public void XoaHoaDon(int maHD, int maKH)
        {
            using (var db = new Model1())
            {
                // 1. Tìm và xóa Chi Tiết Hóa Đơn liên quan
                var chiTietToDelete = db.ChiTiets.Where(ct => ct.MaHD == maHD).ToList();
                db.ChiTiets.RemoveRange(chiTietToDelete);

                // 2. Tìm và xóa Hóa Đơn
                var hoaDonToDelete = db.HoaDons.FirstOrDefault(hd => hd.MaHD == maHD);
                if (hoaDonToDelete != null)
                {
                    db.HoaDons.Remove(hoaDonToDelete);
                }

                // LƯU Ý: Không nên xóa Khách Hàng (MaKH) trừ khi chắc chắn đây là giao dịch duy nhất
                // Khách Hàng có thể có các hóa đơn khác.

                db.SaveChanges();
            }
        }
    }
}