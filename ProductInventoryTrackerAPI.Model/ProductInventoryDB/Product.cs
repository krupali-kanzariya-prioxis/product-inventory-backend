using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductInventoryTrackerAPI.Model.ProductInventoryDB;

[Index("Sku", Name = "UQ_Products_SKU", IsUnique = true)]
[Index("ProductSid", Name = "UQ_Products_Sid", IsUnique = true)]
public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [StringLength(50)]
    public string ProductSid { get; set; } = null!;

    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Column("SKU")]
    [StringLength(50)]
    public string Sku { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int? SupplierId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    public int CurrentStock { get; set; }

    public int ReorderThreshold { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    [ForeignKey("SupplierId")]
    [InverseProperty("Products")]
    public virtual Supplier? Supplier { get; set; }
}
