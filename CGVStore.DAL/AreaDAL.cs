using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CGVStore.Models;
using System.Data.Entity;

namespace CGVStore.DAL
{
    // File: CGVStore.DAL/AreaDAL.cs
    
    public class AreaDAL
    {
        // === Logic Thêm Khu Vực (Từ Form3.cs) ===
        public bool KiemTraIDTonTai(int areaID)
        {
            using (var db = new Model1())
            {
                return db.Areas.Any(a => a.AreaID == areaID);
            }
        }

        public void ThemKhuVuc(Area newArea)
        {
            using (var db = new Model1())
            {
                db.Areas.Add(newArea);
                db.SaveChanges();
            }
        }

        // === Logic Tải Khu Vực (Từ Form5.cs - LoadArea) ===
        public List<Area> LayTatCaKhuVuc()
        {
            using (var db = new Model1())
            {
                return db.Areas.ToList();
            }
        }
    }
}
