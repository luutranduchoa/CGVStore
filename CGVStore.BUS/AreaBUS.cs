using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CGVStore.DAL;
using CGVStore.Models;
namespace CGVStore.BUS
{
    // File: CGVStore.BUS/AreaBUS.cs
    public class AreaBUS
    {
        private AreaDAL areaDAL = new AreaDAL();

        // === Logic Thêm Khu Vực (Cho Form3.cs) ===
        public void XuLyThemKhuVuc(string areaIDText, string areaName)
        {
            // 1. Logic Nghiệp Vụ: Kiểm tra rỗng
            if (string.IsNullOrEmpty(areaIDText) || string.IsNullOrEmpty(areaName))
            {
                // Vẫn giữ kiểm tra rỗng, ném lỗi để UI bắt
                throw new ArgumentException("Mã số và Tên Khu Vực không được để trống.");
            }

            // =================================================================
            // 2. LOGIC ÉP KIỂU VÀ KIỂM TRA ĐỊNH DẠNG (BƯỚC MỚI)
            // =================================================================
            int areaIDNumber;

            // Thử ép kiểu từ chuỗi (string) sang số nguyên (int)
            if (!int.TryParse(areaIDText, out areaIDNumber))
            {
                // Nếu không phải là số hợp lệ, ném lỗi để UI bắt và hiển thị
                throw new FormatException("Mã Khu Vực phải là một số nguyên hợp lệ.");
            }

            // =================================================================
            // 3. LOGIC NGHIỆP VỤ (Vẫn giữ ở BUS)
            // =================================================================

            // Kiểm tra AreaID có hợp lệ sau khi đã chuyển đổi
            if (areaIDNumber <= 0)
            {
                throw new ArgumentException("Mã Khu Vực phải là một số nguyên dương.");
            }

            // 4. Kiểm tra trùng lặp ID (Gọi DAL, sử dụng giá trị int đã chuyển đổi)
            if (areaDAL.KiemTraIDTonTai(areaIDNumber)) // Cần đảm bảo hàm DAL nhận int
            {
                throw new InvalidOperationException($"Mã Khu Vực ({areaIDNumber}) đã tồn tại. Vui lòng chọn mã khác.");
            }

            // 5. Tạo đối tượng và gọi DAL để lưu
            var newArea = new Area
            {
                AreaID = areaIDNumber, // Gán giá trị int đã chuyển đổi thành công
                AreaName = areaName
            };

            areaDAL.ThemKhuVuc(newArea);
        }

        // === Logic Tải Khu Vực (Cho Form5.cs) ===
        public List<Area> LayDanhSachKhuVuc()
        {
            return areaDAL.LayTatCaKhuVuc();
        }
    }
}
