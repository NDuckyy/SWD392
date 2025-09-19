using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD392_backend.Entities;

[Index("ProductsId", "IsMain", Name = "idx_product_images_productid_ismain")]
public partial class product_image
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Column("product_image_url")]
    [StringLength(255)]
    public string ProductImageUrl { get; set; } = null!;

    [Column("is_main")]
    public bool IsMain { get; set; }

    [Column("products_id")]
    public int ProductsId { get; set; }

    [ForeignKey("ProductsId")]
    [InverseProperty("product_images")]
    public virtual product products { get; set; } = null!;
}
