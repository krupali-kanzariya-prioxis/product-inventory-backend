using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductInventoryTrackerAPI.Model.ProductInventoryDB;

[Index("SupplierSid", Name = "UQ_Suppliers_Sid", IsUnique = true)]
public partial class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [StringLength(50)]
    public string SupplierSid { get; set; } = null!;

    [StringLength(200)]
    public string SupplierName { get; set; } = null!;

    [StringLength(200)]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    [InverseProperty("Supplier")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
