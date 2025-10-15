using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CGVStore.DAL;
using CGVStore.Models;

namespace CGVStore.BUS
{
    // File: CGVStore.BUS/UserBUS.cs
     // Cần thiết để tạo đối tượng User
    public class UserBUS
    {
        private UserDAL userDAL = new UserDAL();

        // === Logic Đăng nhập (Cho Form2.cs) ===
        public bool XuLyDangNhap(string username, string password)
        {
            // 1. Logic Nghiệp Vụ: Kiểm tra tính hợp lệ dữ liệu
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Tên đăng nhập và mật khẩu không được để trống.");
            }

            // 2. Gọi DAL để truy cập dữ liệu
            return userDAL.KiemTraDangNhap(username, password);
        }

        // === Logic Thêm User (Cho Form4.cs) ===
        public void XuLyThemUser(string username, string password, string confirmPassword)
        {
            // 1. Logic Nghiệp Vụ: Kiểm tra tính hợp lệ dữ liệu
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                throw new ArgumentException("Các trường không được để trống.");
            }

            if (password != confirmPassword)
            {
                throw new ArgumentException("Mật khẩu xác nhận không khớp.");
            }

            // 2. Gọi DAL: Kiểm tra trùng tên
            if (userDAL.IsTenUserExists(username))
            {
                throw new InvalidOperationException($"Tên tài khoản '{username}' đã tồn tại.");
            }

            // 3. Gọi DAL: Lấy mã mới và lưu
            var newUser = new User
            {
                MaUser = userDAL.GetNextMaUser(), // Lấy mã mới từ DAL
                TenUser = username,
                MatKhau = password // LƯU Ý: Cần mã hóa trong thực tế
            };

            userDAL.ThemUser(newUser);
        }
    }
}
