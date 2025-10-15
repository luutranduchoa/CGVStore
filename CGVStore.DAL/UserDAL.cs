using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CGVStore.Models;
using System.Data.Entity;

namespace CGVStore.DAL
{
    // File: CGVStore.DAL/UserDAL.cs
     // Giả định Entity Models ở đây
    public class UserDAL
    {
        // === Logic Đăng nhập (Từ Form2.cs) ===
        public bool KiemTraDangNhap(string username, string password)
        {
            // Toàn bộ truy vấn DB được chuyển vào đây
            using (var db = new Model1())
            {
                var user = db.Users
                             .FirstOrDefault(u => u.TenUser.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                                                  u.MatKhau == password);
                return user != null;
            }
        }

        // === Logic Thêm User (Từ Form4.cs) ===
        public bool IsTenUserExists(string username)
        {
            using (var db = new Model1())
            {
                return db.Users.Any(u => u.TenUser.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        public int GetNextMaUser()
        {
            using (var db = new Model1())
            {
                return db.Users.Any() ? db.Users.Max(u => u.MaUser) + 1 : 1;
            }
        }

        public void ThemUser(User newUser)
        {
            using (var db = new Model1())
            {
                db.Users.Add(newUser);
                db.SaveChanges();
            }
        }
    }
}
