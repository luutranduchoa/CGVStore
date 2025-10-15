namespace CGVStore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MaUser { get; set; }

        [StringLength(500)]
        public string TenUser { get; set; }

        [StringLength(60)]
        public string MatKhau { get; set; }
    }
}
