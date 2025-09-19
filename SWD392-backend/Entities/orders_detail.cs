using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Entities.Enums;

namespace SWD392_backend.Entities;

[Table("orders_detail")]
public partial class orders_detail
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("price")]
    public double Price { get; set; }

    [Column("discount_percent")]
    public double DiscountPercent { get; set; }

    [Column("note")]
    public string Note { get; set; } = null!;

    [Column("status")]
    public OrderStatus Status { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("orders_details")]
    public virtual order order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("orders_details")]
    public virtual product product { get; set; } = null!;
}
