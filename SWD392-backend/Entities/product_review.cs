using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD392_backend.Entities;

public partial class product_review
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("rating")]
    public int Rating { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("product_reviews")]
    public virtual product product { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("product_reviews")]
    public virtual user user { get; set; } = null!;
}
