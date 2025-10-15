using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace CGVStore.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<ChiTiet> ChiTiets { get; set; }
        public virtual DbSet<HoaDon> HoaDons { get; set; }
        public virtual DbSet<KhachHang> KhachHangs { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HoaDon>()
                .HasMany(e => e.ChiTiets)
                .WithRequired(e => e.HoaDon)
                .HasForeignKey(e => new { e.MaHD, e.MaKH })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<KhachHang>()
                .HasMany(e => e.HoaDons)
                .WithRequired(e => e.KhachHang)
                .WillCascadeOnDelete(false);
        }
    }
}
