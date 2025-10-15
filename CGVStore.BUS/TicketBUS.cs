using System;
using System.Collections.Generic;
using System.Linq;
using CGVStore.DAL; // Cần tham chiếu đến DAL Project
using CGVStore.Models; // Cần tham chiếu đến Entity Models

namespace CGVStore.BUS
{
    // Giả định có TicketDAL để thực hiện các thao tác CRUD cơ bản
    public class TicketBUS
    {
        private TicketDAL ticketDAL = new TicketDAL();
        // Giả định có KhachHangDAL để tìm/tạo khách hàng
        // private KhachHangDAL khachHangDAL = new KhachHangDAL(); 

        // =======================================================
        //                 DỮ LIỆU TẢI (LOAD DATA)
        // =======================================================

        /// <summary>
        /// Tải danh sách các ghế đã được bán (để UI tô màu).
        /// </summary>
        /// <returns>Danh sách Mã ghế (string) đã bán.</returns>
        public List<string> LayDanhSachGheDaBan()
        {
            // Logic nghiệp vụ: (Không có gì phức tạp ngoài việc gọi DAL)
            return ticketDAL.LayDanhSachGheDaBan();
        }

        /// <summary>
        /// Tải danh sách Hóa Đơn và Khách Hàng cho DataGridView.
        /// (Giả định DAL trả về một List các đối tượng có thể hiển thị)
        /// </summary>
        /// <returns>Danh sách đối tượng DTO/ViewModel để hiển thị trên DGV.</returns>
        public object LayDanhSachHoaDonView()
        {
            // Logic nghiệp vụ: (Không có gì phức tạp ngoài việc gọi DAL)
            // DAL đã xử lý JOIN/INCLUDE để lấy dữ liệu cần thiết.
            return ticketDAL.LayDanhSachHoaDonView();
        }

        // =======================================================
        //                  NGHIỆP VỤ (BUSINESS LOGIC)
        // =======================================================

        /// <summary>
        /// Xử lý toàn bộ nghiệp vụ mua vé: Kiểm tra, tạo Khách Hàng, tạo Hóa Đơn, tạo Chi Tiết Hóa Đơn.
        /// </summary>
        /// <param name="tenKH">Tên khách hàng.</param>
        /// <param name="sdt">Số điện thoại khách hàng.</param>
        /// <param name="gioiTinh">Giới tính khách hàng.</param>
        /// <param name="areaID">Mã khu vực (int/string tùy theo Area Model).</param>
        /// <param name="tongTien">Tổng tiền (decimal/double).</param>
        /// <param name="selectedSeats">Danh sách mã ghế đã chọn (vd: "A1", "A2").</param>
        public void XuLyMuaVe(string tenKH, string sdt, string gioiTinh, object areaID, decimal tongTien, List<string> selectedSeats)
        {
            // 1. NGHIỆP VỤ: KIỂM TRA TÍNH HỢP LỆ DỮ LIỆU ĐẦU VÀO
            if (string.IsNullOrEmpty(tenKH) || string.IsNullOrEmpty(sdt))
            {
                throw new ArgumentException("Thông tin Khách hàng (Tên, SĐT) không được để trống.");
            }
            if (tongTien <= 0 || selectedSeats == null || selectedSeats.Count == 0)
            {
                throw new ArgumentException("Tổng tiền phải lớn hơn 0 và phải chọn ít nhất 1 ghế.");
            }

            // LƯU Ý: Thêm kiểm tra định dạng SĐT, định dạng AreaID nếu cần.

            // 2. TẠO/TÌM KHÁCH HÀNG (Gọi DAL)
            var khachHang = new KhachHang
            {
                TenKH = tenKH,
                SDT = sdt,
                GioiTinh = gioiTinh
            };
            int maKH = ticketDAL.GetOrCreateKhachHang(khachHang); // Giả định DAL xử lý việc tìm/tạo và trả về MaKH

            // 3. TẠO HÓA ĐƠN (Gọi DAL)
            var hoaDon = new HoaDon
            {
                MaKH = maKH,
                TongTien = tongTien,
                NgayMua = DateTime.Now,
                AreaID = areaID.ToString() // Giả định AreaID là string như trong Form3.cs
            };
            int maHD = ticketDAL.TaoHoaDon(hoaDon); // Giả định DAL trả về MaHD sau khi SaveChanges

            // 4. TẠO CHI TIẾT HÓA ĐƠN (Gọi DAL)
            var chiTiets = selectedSeats.Select(seat => new ChiTiet
            {
                MaHD = maHD,
                MaKH = maKH,
                SoGheNgoi = seat
            }).ToList();

            ticketDAL.ThemChiTiet(chiTiets);
        }

        /// <summary>
        /// Xử lý nghiệp vụ hủy vé, xóa Hóa Đơn và các Chi Tiết liên quan.
        /// </summary>
        public void XuLyHuyVe(int maHD, int maKH)
        {
            if (maHD <= 0)
            {
                throw new ArgumentException("Mã Hóa Đơn không hợp lệ để hủy.");
            }

            // Logic nghiệp vụ: (Không có gì phức tạp ngoài việc gọi DAL)
            // DAL chịu trách nhiệm xóa ChiTiet trước, sau đó xóa HoaDon.
            ticketDAL.XoaHoaDon(maHD, maKH);
        }
    }
}