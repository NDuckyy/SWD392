using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD392_backend.Entities;

public partial class supplier
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("slug")]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    [Column("registered_at", TypeName = "timestamp with time zone")]
    public DateTime RegisteredAt { get; set; }

    [Column("is_verified")]
    public bool IsVerified { get; set; }

    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("image_url")]
    [StringLength(255)]
    public string ImageUrl { get; set; } = null!;
    
    // Thêm các trường mới vào đây
    [Column("front_image")]
    [StringLength(255)]
    public string? FrontImageCCCD { get; set; } // Mặt trước CCCD

    [Column("back_image")]
    [StringLength(255)]
    public string? BackImageCCCD { get; set; } // Mặt sau CCCD

    [InverseProperty("supplier")]
    public virtual ICollection<order> orders { get; set; } = new List<order>();

    [InverseProperty("supplier")]
    public virtual ICollection<product> products { get; set; } = new List<product>();

    [ForeignKey("UserId")]
    [InverseProperty("suppliers")]
    public virtual user user { get; set; } = null!;
}
