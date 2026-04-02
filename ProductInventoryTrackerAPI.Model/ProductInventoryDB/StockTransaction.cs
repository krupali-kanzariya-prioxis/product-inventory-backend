using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductInventoryTrackerAPI.Model.ProductInventoryDB;

[Index("StockTransactionSid", Name = "UQ_StockTransactions_Sid", IsUnique = true)]
public partial class StockTransaction
{
    [Key]
    public int StockTransactionId { get; set; }

    [StringLength(50)]
    public string StockTransactionSid { get; set; } = null!;

    public int ProductId { get; set; }

    [StringLength(10)]
    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime TransactionDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("StockTransactions")]
    public virtual Product Product { get; set; } = null!;
}
