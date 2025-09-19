using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD392_backend.Entities;

[Index("Email", Name = "users_email_key", IsUnique = true)]
public partial class user
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Column("username")]
    [StringLength(255)]
    public string Username { get; set; } = null!;

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [Column("role")]
    public string Role { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("phone")]
    [StringLength(255)]
    public string Phone { get; set; } = null!;

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; } = null!;

    [Column("area_code")]
    [StringLength(32)]
    public string? AreaCode { get; set; }

    [Column("image_url")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [InverseProperty("user")]
    public virtual ICollection<order> orders { get; set; } = new List<order>();

    [InverseProperty("user")]
    public virtual ICollection<product_review> product_reviews { get; set; } = new List<product_review>();

    [InverseProperty("user")]
    public virtual ICollection<supplier> suppliers { get; set; } = new List<supplier>();

    [InverseProperty("user")]
    public virtual shipper? shipper { get; set; }
}
