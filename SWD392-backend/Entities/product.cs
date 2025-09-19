using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD392_backend.Entities;

[Index("Name", Name = "products_name_index")]
public partial class product
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("price")]
    public double Price { get; set; }

    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("stock_in_quantity")]
    public int StockInQuantity { get; set; }

    [Column("rating_average")]
    public double RatingAverage { get; set; } = 0.0;

    [Column("sku")]
    [StringLength(255)]
    public string Sku { get; set; } = null!;

    [Column("discount_price")]
    public double DiscountPrice { get; set; }

    [Column("discount_percent")]
    public double DiscountPercent { get; set; } = 0.0;

    [Column("sold_quantity")]
    public int SoldQuantity { get; set; } = 0;

    [Column("available_quantity")]
    public int AvailableQuantity { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("is_sale")]
    public bool IsSale { get; set; }

    [Column("slug")]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    [Column("categories_id")]
    public int CategoriesId { get; set; }

    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [ForeignKey("CategoriesId")]
    [InverseProperty("products")]
    public virtual category categories { get; set; } = null!;

    [InverseProperty("product")]
    public virtual ICollection<product_attribute> product_attributes { get; set; } = new List<product_attribute>();

    [InverseProperty("products")]
    public virtual ICollection<product_image> product_images { get; set; } = new List<product_image>();

    [InverseProperty("product")]
    public virtual ICollection<product_review> product_reviews { get; set; } = new List<product_review>();

    [InverseProperty("product")]
    public virtual ICollection<orders_detail> orders_details { get; set; } = new List<orders_detail>();

    [ForeignKey("SupplierId")]
    [InverseProperty("products")]
    public virtual supplier supplier { get; set; } = null!;
}
