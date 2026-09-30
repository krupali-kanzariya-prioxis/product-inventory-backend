using System.ComponentModel;
using ModelContextProtocol.Server;
using ProductInventoryTrackerAPI.Model.RequestModel;

namespace ProductInventoryTrackerAPI.Mcp.Tools;

[McpServerToolType]
public sealed class ProductTools
{
    private readonly ProductMcpService _productMcpService;

    public ProductTools(ProductMcpService productMcpService)
    {
        _productMcpService = productMcpService;
    }

    [McpServerTool(Name = "search_products", ReadOnly = true)]
    [Description("Use this tool to search the existing active products in the inventory system. Prefer this before asking for product details or before deciding which product SID to use in a later tool call.")]
    public Task<ProductListToolResult> SearchProductsAsync(
        [Description("Optional free-text search that matches product list results, such as part of the product name or SKU.")] string? searchText = null,
        [Description("Set to true to return only products that are currently at or below their reorder threshold.")] bool lowStockOnly = false,
        [Description("Maximum number of products to return. Use a small value for concise answers. Allowed range is 1 to 50.")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        return _productMcpService.SearchProductsAsync(searchText, lowStockOnly, limit, cancellationToken);
    }

    [McpServerTool(Name = "get_product", ReadOnly = true)]
    [Description("Use this tool when you already know a product SID and need the current details for one specific product.")]
    public Task<ProductMcpItem> GetProductAsync(
        [Description("The product SID, such as a value beginning with PRD, returned by the API or another MCP tool.")] string productSid,
        CancellationToken cancellationToken)
    {
        return _productMcpService.GetProductBySidAsync(productSid, cancellationToken);
    }

    [McpServerTool(Name = "create_product", Destructive = false)]
    [Description("Use this tool to create a new product in the inventory system after the user has confirmed the write action.")]
    public Task<ProductMcpItem> CreateProductAsync(
        [Description("The human-readable product name.")] string productName,
        [Description("The unique SKU for the new product.")] string sku,
        [Description("Optional product description.")] string? description,
        [Description("Optional category SID to associate with the product.")] string? categorySid,
        [Description("Optional supplier SID to associate with the product.")] string? supplierSid,
        [Description("The unit price. Must be greater than zero.")] decimal unitPrice,
        [Description("The initial stock quantity. Must be zero or greater.")] int currentStock,
        [Description("The reorder threshold used for low-stock checks. Must be zero or greater.")] int reorderThreshold,
        CancellationToken cancellationToken)
    {
        return _productMcpService.CreateProductAsync(new ProductRequestModel
        {
            ProductName = productName,
            Sku = sku,
            Description = description,
            CategorySid = categorySid,
            SupplierSid = supplierSid,
            UnitPrice = unitPrice,
            CurrentStock = currentStock,
            ReorderThreshold = reorderThreshold
        }, cancellationToken);
    }

    [McpServerTool(Name = "update_product", Destructive = false, Idempotent = true)]
    [Description("Use this tool to partially update an existing product after the user has confirmed the write action. Call get_product first to inspect the current values, then only provide the fields you want to change; omitted fields keep their current values.")]
    public Task<ProductMcpItem> UpdateProductAsync(
        [Description("The product SID for the item to update.")] string productSid,
        [Description("Optional new human-readable product name. Leave this blank to keep the current value.")] string productName = "",
        [Description("Optional new unique SKU for the product. Leave this blank to keep the current value.")] string sku = "",
        [Description("Optional updated product description. Leave this blank to keep the current value.")] string description = "",
        [Description("Optional category SID to associate with the product. Leave this blank to keep the current category.")] string categorySid = "",
        [Description("Optional supplier SID to associate with the product. Leave this blank to keep the current supplier.")] string supplierSid = "",
        [Description("Optional updated unit price. Use -1 to keep the current value. Any other value must be greater than zero.")] decimal unitPrice = -1,
        [Description("Optional updated stock quantity. Use -1 to keep the current value. Any other value must be zero or greater.")] int currentStock = -1,
        [Description("Optional updated reorder threshold. Use -1 to keep the current value. Any other value must be zero or greater.")] int reorderThreshold = -1,
        CancellationToken cancellationToken = default)
    {
        return _productMcpService.UpdateProductAsync(productSid, new ProductPartialUpdateRequest
        {
            ProductName = string.IsNullOrWhiteSpace(productName) ? null : productName,
            Sku = string.IsNullOrWhiteSpace(sku) ? null : sku,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            CategorySid = string.IsNullOrWhiteSpace(categorySid) ? null : categorySid,
            SupplierSid = string.IsNullOrWhiteSpace(supplierSid) ? null : supplierSid,
            UnitPrice = unitPrice < 0 ? null : unitPrice,
            CurrentStock = currentStock < 0 ? null : currentStock,
            ReorderThreshold = reorderThreshold < 0 ? null : reorderThreshold
        }, cancellationToken);
    }

    [McpServerTool(Name = "get_inventory_summary", ReadOnly = true)]
    [Description("Use this tool to get a concise inventory summary, including low-stock counts and product counts by category.")]
    public Task<ProductSummaryToolResult> GetInventorySummaryAsync(CancellationToken cancellationToken)
    {
        return _productMcpService.GetInventorySummaryAsync(cancellationToken);
    }
}
