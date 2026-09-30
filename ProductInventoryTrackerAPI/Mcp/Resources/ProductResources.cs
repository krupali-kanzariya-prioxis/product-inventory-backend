using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;

namespace ProductInventoryTrackerAPI.Mcp.Resources;

[McpServerResourceType]
public sealed class ProductResources
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private readonly ProductMcpService _productMcpService;

    public ProductResources(ProductMcpService productMcpService)
    {
        _productMcpService = productMcpService;
    }

    [McpServerResource(UriTemplate = "app://products/summary", Name = "products_summary", MimeType = "application/json")]
    [Description("Read this resource for the current high-level inventory state, including total products, low-stock counts, and counts by category.")]
    public async Task<string> GetProductsSummaryAsync(CancellationToken cancellationToken)
    {
        var summary = await _productMcpService.GetSummaryResourceAsync(cancellationToken);
        return JsonSerializer.Serialize(summary, JsonOptions);
    }

    [McpServerResource(UriTemplate = "app://products/{productSid}", Name = "product_details", MimeType = "application/json")]
    [Description("Read this templated resource when you need full details for one product and its recent stock transaction history.")]
    public async Task<string> GetProductBySidAsync(
        [Description("The product SID for the product to read.")] string productSid,
        CancellationToken cancellationToken)
    {
        var details = await _productMcpService.GetProductResourceAsync(productSid, cancellationToken);
        return JsonSerializer.Serialize(details, JsonOptions);
    }

    [McpServerResource(UriTemplate = "app://schema", Name = "product_schema", MimeType = "text/plain")]
    [Description("Read this static resource for a plain-language description of the product inventory domain model and safe MCP usage rules.")]
    public string GetProductSchema()
    {
        return _productMcpService.GetSchemaResourceText();
    }
}
