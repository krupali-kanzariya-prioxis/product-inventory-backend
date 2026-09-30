using System.ComponentModel;

namespace ProductInventoryTrackerAPI.Mcp;

public sealed class ProductListToolResult
{
    [Description("The number of products returned in this response.")]
    public int ReturnedCount { get; set; }

    [Description("The total number of matching products reported by the existing API paging metadata.")]
    public int TotalResults { get; set; }

    [Description("True when the result was capped to keep the response model-friendly.")]
    public bool IsTruncated { get; set; }

    [Description("A concise list of matching products.")]
    public IReadOnlyList<ProductMcpItem> Items { get; set; } = [];
}

public sealed class ProductMcpItem
{
    [Description("The product SID used by the API and MCP tools.")]
    public string ProductSid { get; set; } = string.Empty;

    [Description("The product name.")]
    public string ProductName { get; set; } = string.Empty;

    [Description("The product SKU.")]
    public string Sku { get; set; } = string.Empty;

    [Description("The category name, when one is assigned.")]
    public string? CategoryName { get; set; }

    [Description("The supplier name, when one is assigned.")]
    public string? SupplierName { get; set; }

    [Description("The current unit price.")]
    public decimal UnitPrice { get; set; }

    [Description("The current on-hand stock quantity.")]
    public int CurrentStock { get; set; }

    [Description("The reorder threshold used to detect low-stock items.")]
    public int ReorderThreshold { get; set; }

    [Description("True when current stock is at or below the reorder threshold.")]
    public bool IsLowStock { get; set; }

    [Description("The last modification time in UTC, when available.")]
    public DateTime? LastModifiedAt { get; set; }
}

public sealed class ProductSummaryToolResult
{
    [Description("The total number of active products.")]
    public int TotalActiveProducts { get; set; }

    [Description("The number of active products that are currently low on stock.")]
    public int LowStockProducts { get; set; }

    [Description("The total on-hand units across all active products.")]
    public int TotalUnitsInStock { get; set; }

    [Description("The total inventory value based on unit price multiplied by current stock.")]
    public decimal TotalInventoryValue { get; set; }

    [Description("Counts grouped by category name.")]
    public IReadOnlyList<CategoryCountItem> ProductsByCategory { get; set; } = [];
}

public sealed class CategoryCountItem
{
    [Description("The category name. Unassigned is used when a product has no category.")]
    public string CategoryName { get; set; } = string.Empty;

    [Description("The number of active products in the category.")]
    public int Count { get; set; }
}

public sealed class ProductResourceDetails
{
    [Description("A concise product summary.")]
    public ProductMcpItem Product { get; set; } = new();

    [Description("The five most recent stock transactions for this product.")]
    public IReadOnlyList<ProductTransactionItem> RecentTransactions { get; set; } = [];
}

public sealed class ProductTransactionItem
{
    [Description("The stock transaction SID.")]
    public string StockTransactionSid { get; set; } = string.Empty;

    [Description("The transaction type, such as IN or OUT.")]
    public string TransactionType { get; set; } = string.Empty;

    [Description("The quantity changed by the transaction.")]
    public int Quantity { get; set; }

    [Description("Optional human notes recorded with the transaction.")]
    public string? Notes { get; set; }

    [Description("The transaction date in UTC or local database time as stored.")]
    public DateTime TransactionDate { get; set; }
}
