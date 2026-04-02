using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProductInventoryTrackerAPI.Model.ProductInventoryDB;

public partial class ProductInventoryDBContext : DbContext
{
    public ProductInventoryDBContext(DbContextOptions<ProductInventoryDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ReorderThreshold).HasDefaultValue(10);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Category).WithMany(p => p.Products).HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products).HasConstraintName("FK_Products_Suppliers");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.StockTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransactions_Products");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Role).HasDefaultValue("User");
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
