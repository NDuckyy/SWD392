using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWD392_backend.Entities
{
    public partial class shipper
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("area_code")]
        [StringLength(32)]
        public string AreaCode { get; set; } = null!;

        [ForeignKey("UserId")]
        [InverseProperty("shipper")]
        public virtual user user { get; set; } = null!;
    }
}
