namespace CGVStore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChiTiet")]
    public partial class ChiTiet
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaCT { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaHD { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaKH { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgayThanhToan { get; set; }

        public int? SoGheNgoi { get; set; }

        public virtual HoaDon HoaDon { get; set; }
    }
}
