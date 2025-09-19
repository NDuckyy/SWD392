using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD392_backend.Entities;

public partial class order
{
    [Column("id")]
    [Key]
    public Guid Id { get; set; }

    [Column("total")]
    public double Total { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("shipping_price")]
    public double ShippingPrice { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; } = null!;

    [Column("area_code")]
    [StringLength(32)]
    public string? AreaCode { get; set; }

    [Column("shipper_id")]
    public int? ShipperId { get; set; }

    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [Column("paid_at", TypeName = "timestamp with time zone")]
    public DateTime? PaidAt { get; set; }

    [Column("deliveried_at", TypeName = "timestamp with time zone")]
    public DateTime? DeliveriedAt { get; set; }

    [InverseProperty("order")]
    public virtual ICollection<orders_detail> orders_details { get; set; } = new List<orders_detail>();

    [ForeignKey("SupplierId")]
    [InverseProperty("orders")]
    public virtual supplier supplier { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("orders")]
    public virtual user user { get; set; } = null!;
}
