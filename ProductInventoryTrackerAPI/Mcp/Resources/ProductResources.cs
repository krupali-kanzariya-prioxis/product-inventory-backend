using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ProductInventoryTrackerAPI.Mcp.Resources;

[McpServerResourceType]
public sealed class ProductResources
{
    private readonly ProductMcpService _productMcpService;

    public ProductResources(ProductMcpService productMcpService)
    {
        _productMcpService = productMcpService;
    }

    [McpServerResource(UriTemplate = "app://products/summary", Name = "products_summary", MimeType = "application/json")]
    [Description("Read this resource for the current high-level inventory state, including total products, low-stock counts, and counts by category.")]
    public Task<ProductSummaryToolResult> GetProductsSummaryAsync(CancellationToken cancellationToken)
    {
        return _productMcpService.GetSummaryResourceAsync(cancellationToken);
    }

    [McpServerResource(UriTemplate = "app://products/{productSid}", Name = "product_details", MimeType = "application/json")]
    [Description("Read this templated resource when you need full details for one product and its recent stock transaction history.")]
    public Task<ProductResourceDetails> GetProductBySidAsync(
        [Description("The product SID for the product to read.")] string productSid,
        CancellationToken cancellationToken)
    {
        return _productMcpService.GetProductResourceAsync(productSid, cancellationToken);
    }

    [McpServerResource(UriTemplate = "app://schema", Name = "product_schema", MimeType = "text/plain")]
    [Description("Read this static resource for a plain-language description of the product inventory domain model and safe MCP usage rules.")]
    public string GetProductSchema()
    {
        return _productMcpService.GetSchemaResourceText();
    }
}
