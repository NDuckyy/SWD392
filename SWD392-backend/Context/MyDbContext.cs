using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Entities;
using SWD392_backend.Entities.Enums;

namespace SWD392_backend.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<order> orders { get; set; }

    public virtual DbSet<orders_detail> orders_details { get; set; }

    public virtual DbSet<product> products { get; set; }

    public virtual DbSet<product_attribute> product_attributes { get; set; }

    public virtual DbSet<product_image> product_images { get; set; }

    public virtual DbSet<product_review> product_reviews { get; set; }

    public virtual DbSet<shipper> shipper { get; set; }

    public virtual DbSet<supplier> suppliers { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<OrderStatus>("order_status");

        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");
        });

        modelBuilder.Entity<order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.supplier).WithMany(p => p.orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_supplier");

            entity.HasOne(d => d.user).WithMany(p => p.orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_user");
        });

        modelBuilder.Entity<orders_detail>(entity =>
        {

            entity.HasKey(e => e.Id).HasName("orders_detail_pkey");

            entity.HasOne(d => d.product).WithMany(p => p.orders_details)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_detail_order");

            entity.Property(e => e.Status)
                .HasColumnType("order_status");
        });

        modelBuilder.Entity<product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.HasOne(d => d.categories).WithMany(p => p.products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_categories");

            entity.HasOne(d => d.supplier).WithMany(p => p.products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_suppliers");

            entity.Property(p => p.RatingAverage)
                .HasDefaultValue(0.0);

            entity.Property(p => p.DiscountPercent)
                .HasDefaultValue(0.0);

            entity.Property(p => p.SoldQuantity)
                .HasDefaultValue(0);
        });

        modelBuilder.Entity<product_attribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_attribute_pkey");

            entity.HasOne(d => d.product).WithMany(p => p.product_attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_attribute_product");
        });

        modelBuilder.Entity<product_image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_images_pkey");

            entity.HasOne(d => d.products).WithMany(p => p.product_images)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_images_products");
        });

        modelBuilder.Entity<product_review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_reviews_pkey");

            entity.HasOne(d => d.product).WithMany(p => p.product_reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_reviews_product");

            entity.HasOne(d => d.user).WithMany(p => p.product_reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_reviews_user");
        });

        modelBuilder.Entity<shipper>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shipper_pkey");

            entity.HasOne(d => d.user).WithOne(p => p.shipper)
                .HasForeignKey<shipper>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_shipper_user");
        });

        modelBuilder.Entity<supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("suppliers_pkey");

            entity.HasOne(d => d.user).WithMany(p => p.suppliers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .OnDelete(DeleteBehavior.Cascade)  // Cascade delete instead of ClientSetNull
                .HasConstraintName("fk_suppliers_user");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
        });
        modelBuilder.HasSequence("product_images_seq");
        modelBuilder.HasSequence("products_id_seq");
        modelBuilder.HasSequence("products_images_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
