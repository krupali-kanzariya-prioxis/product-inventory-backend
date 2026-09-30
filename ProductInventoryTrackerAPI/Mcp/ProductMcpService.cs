using ModelContextProtocol;
using ProductInventoryTrackerAPI.Common;
using ProductInventoryTrackerAPI.Model.CommonModel;
using ProductInventoryTrackerAPI.Model.ProductInventoryDB;
using ProductInventoryTrackerAPI.Model.RequestModel;
using ProductInventoryTrackerAPI.Model.ResponseModel;
using ProductInventoryTrackerAPI.Service.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static ProductInventoryTrackerAPI.Common.Enums;

namespace ProductInventoryTrackerAPI.Mcp;

public sealed class ProductMcpService
{
    private const int MaxToolItems = 50;
    private readonly IProductRepository _productRepository;
    private readonly ProductInventoryDBContext _dbContext;
    private readonly ILogger<ProductMcpService> _logger;

    public ProductMcpService(
        IProductRepository productRepository,
        ProductInventoryDBContext dbContext,
        ILogger<ProductMcpService> logger)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ProductListToolResult> SearchProductsAsync(string? searchText, bool lowStockOnly, int limit, CancellationToken cancellationToken)
    {
        var normalizedLimit = NormalizeLimit(limit, "search_products");
        var parameters = CreateSearchParameters(searchText, normalizedLimit);

        var page = lowStockOnly
            ? await _productRepository.GetLowStockProductsAsync(parameters)
            : await _productRepository.GetProductsAsync(parameters);

        var results = ExtractProducts(page.Result)
            .Take(normalizedLimit)
            .ToList();

        var productEntities = await LoadActiveProductsBySidAsync(results.Select(x => x.ProductSid), cancellationToken);
        var products = results
            .Select(result => productEntities.TryGetValue(result.ProductSid, out var entity)
                ? MapToMcpItem(result, entity)
                : MapToMcpItem(result))
            .ToList();

        return new ProductListToolResult
        {
            ReturnedCount = products.Count,
            TotalResults = page.Meta.TotalResults,
            IsTruncated = page.Meta.TotalResults > products.Count,
            Items = products
        };
    }

    public async Task<ProductMcpItem> GetProductBySidAsync(string productSid, CancellationToken cancellationToken)
    {
        var normalizedSid = NormalizeSid(productSid, "product SID", "get_product");
        var product = await LoadActiveProductAsync(normalizedSid, cancellationToken, "get_product");
        return MapToMcpItem(product);
    }

    public async Task<ProductMcpItem> CreateProductAsync(ProductRequestModel model, CancellationToken cancellationToken)
    {
        ValidateProductRequest(model, "create_product");

        var created = await _productRepository.AddProductAsync(model);
        var product = await LoadActiveProductAsync(created.ProductSid, cancellationToken, "create_product");

        _logger.LogInformation(
            "MCP write {Operation} for product {ProductSid}. Fields changed: {ChangedFields}",
            "create_product",
            product.ProductSid,
            string.Join(", ", GetCreateFieldNames(model)));

        return MapToMcpItem(product);
    }

    public async Task<ProductMcpItem> UpdateProductAsync(string productSid, ProductPartialUpdateRequest changes, CancellationToken cancellationToken)
    {
        var normalizedSid = NormalizeSid(productSid, "product SID", "update_product");
        EnsureUpdateHasChanges(changes);

        var existingProduct = await LoadActiveProductAsync(normalizedSid, cancellationToken, "update_product");
        var mergedRequest = MergeUpdate(existingProduct, changes);
        ValidateProductRequest(mergedRequest, "update_product");

        var updated = await _productRepository.UpdateProductAsync(normalizedSid, mergedRequest);
        if (updated == null)
        {
            throw CreateInputException("update_product", $"No active product was found for SID '{normalizedSid}'. Use search_products to find a valid SID.");
        }

        var refreshedProduct = await LoadActiveProductAsync(updated.ProductSid, cancellationToken, "update_product");

        _logger.LogInformation(
            "MCP write {Operation} for product {ProductSid}. Fields changed: {ChangedFields}",
            "update_product",
            refreshedProduct.ProductSid,
            string.Join(", ", GetChangedFieldNames(changes)));

        return MapToMcpItem(refreshedProduct);
    }

    public async Task<ProductSummaryToolResult> GetInventorySummaryAsync(CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Status == (int)StatusTypeDB.Active)
            .ToListAsync(cancellationToken);

        return new ProductSummaryToolResult
        {
            TotalActiveProducts = products.Count,
            LowStockProducts = products.Count(x => x.CurrentStock <= x.ReorderThreshold),
            TotalUnitsInStock = products.Sum(x => x.CurrentStock),
            TotalInventoryValue = products.Sum(x => x.UnitPrice * x.CurrentStock),
            ProductsByCategory = products
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Category?.CategoryName) ? "Unassigned" : x.Category.CategoryName)
                .OrderByDescending(x => x.Count())
                .ThenBy(x => x.Key)
                .Select(x => new CategoryCountItem
                {
                    CategoryName = x.Key,
                    Count = x.Count()
                })
                .ToList()
        };
    }

    public async Task<ProductSummaryToolResult> GetSummaryResourceAsync(CancellationToken cancellationToken)
        => await GetInventorySummaryAsync(cancellationToken);

    public async Task<ProductResourceDetails> GetProductResourceAsync(string productSid, CancellationToken cancellationToken)
    {
        var normalizedSid = NormalizeSid(productSid, "product SID", "product_details");
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .Include(x => x.StockTransactions)
            .SingleOrDefaultAsync(x => x.ProductSid == normalizedSid && x.Status == (int)StatusTypeDB.Active, cancellationToken);

        if (product == null)
        {
            throw CreateInputException("product_details", $"No active product was found for SID '{normalizedSid}'. Use search_products to find a valid SID.");
        }

        return new ProductResourceDetails
        {
            Product = MapToMcpItem(product),
            RecentTransactions = product.StockTransactions
                .Where(x => x.Status == (int)StatusTypeDB.Active)
                .OrderByDescending(x => x.TransactionDate)
                .Take(5)
                .Select(x => new ProductTransactionItem
                {
                    StockTransactionSid = x.StockTransactionSid,
                    TransactionType = x.TransactionType,
                    Quantity = x.Quantity,
                    Notes = x.Notes,
                    TransactionDate = x.TransactionDate
                })
                .ToList()
        };
    }

    public string GetSchemaResourceText()
    {
        return """
Product inventory domain
- Product SID is the stable public identifier for MCP calls.
- Product fields: product name, SKU, description, category SID, supplier SID, unit price, current stock, reorder threshold.
- Status 1 means active.
- Low stock means current stock is less than or equal to reorder threshold.
- Write tools in this demo create and update products only. Delete operations are intentionally excluded from MCP.
""";
    }

    private static Dictionary<string, object> CreateSearchParameters(string? searchText, int pageSize)
    {
        return new Dictionary<string, object>
        {
            [Constants.SearchParameters.PageStart] = 1,
            [Constants.SearchParameters.PageSize] = pageSize,
            [Constants.SearchParameters.SortColumn] = "LastModifiedAt",
            [Constants.SearchParameters.SortOrder] = "DESC",
            [Constants.SearchParameters.SearchText] = string.IsNullOrWhiteSpace(searchText)
                ? "%"
                : searchText.Trim(),
            [Constants.SearchParameters.Filters] = "1 = 1 AND"
        };
    }

    private static IReadOnlyList<ProductResponseModel> ExtractProducts(object result)
    {
        return result switch
        {
            IReadOnlyList<ProductResponseModel> typedList => typedList,
            IEnumerable<ProductResponseModel> enumerable => enumerable.ToList(),
            _ => []
        };
    }

    private async Task<Dictionary<string, Product>> LoadActiveProductsBySidAsync(IEnumerable<string> productSids, CancellationToken cancellationToken)
    {
        var sidList = productSids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (sidList.Count == 0)
        {
            return new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
        }

        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .Where(x => x.Status == (int)StatusTypeDB.Active && sidList.Contains(x.ProductSid))
            .ToDictionaryAsync(x => x.ProductSid, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    private async Task<Product> LoadActiveProductAsync(string productSid, CancellationToken cancellationToken, string operationName)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .SingleOrDefaultAsync(x => x.ProductSid == productSid && x.Status == (int)StatusTypeDB.Active, cancellationToken);

        if (product == null)
        {
            throw CreateInputException(operationName, $"No active product was found for SID '{productSid}'. Use search_products to find a valid SID.");
        }

        return product;
    }

    private ProductRequestModel MergeUpdate(Product existingProduct, ProductPartialUpdateRequest changes)
    {
        ValidateOptionalLookupSid(changes.CategorySid, "categorySid", "update_product");
        ValidateOptionalLookupSid(changes.SupplierSid, "supplierSid", "update_product");

        return new ProductRequestModel
        {
            ProductName = changes.ProductName ?? existingProduct.ProductName,
            Sku = changes.Sku ?? existingProduct.Sku,
            Description = changes.Description ?? existingProduct.Description,
            CategorySid = changes.CategorySid ?? existingProduct.Category?.CategorySid,
            SupplierSid = changes.SupplierSid ?? existingProduct.Supplier?.SupplierSid,
            UnitPrice = changes.UnitPrice ?? existingProduct.UnitPrice,
            CurrentStock = changes.CurrentStock ?? existingProduct.CurrentStock,
            ReorderThreshold = changes.ReorderThreshold ?? existingProduct.ReorderThreshold
        };
    }

    private static ProductMcpItem MapToMcpItem(ProductResponseModel product, Product? productEntity = null)
    {
        return new ProductMcpItem
        {
            ProductSid = product.ProductSid,
            ProductName = product.ProductName,
            Sku = product.Sku,
            Description = productEntity?.Description ?? product.Description,
            CategorySid = productEntity?.Category?.CategorySid,
            SupplierSid = productEntity?.Supplier?.SupplierSid,
            CategoryName = product.CategoryName ?? productEntity?.Category?.CategoryName,
            SupplierName = product.SupplierName ?? productEntity?.Supplier?.SupplierName,
            UnitPrice = product.UnitPrice,
            CurrentStock = product.CurrentStock,
            ReorderThreshold = product.ReorderThreshold,
            IsLowStock = product.CurrentStock <= product.ReorderThreshold,
            LastModifiedAt = product.LastModifiedAt
        };
    }

    private static ProductMcpItem MapToMcpItem(Product product)
    {
        return new ProductMcpItem
        {
            ProductSid = product.ProductSid,
            ProductName = product.ProductName,
            Sku = product.Sku,
            Description = product.Description,
            CategorySid = product.Category?.CategorySid,
            SupplierSid = product.Supplier?.SupplierSid,
            CategoryName = product.Category?.CategoryName,
            SupplierName = product.Supplier?.SupplierName,
            UnitPrice = product.UnitPrice,
            CurrentStock = product.CurrentStock,
            ReorderThreshold = product.ReorderThreshold,
            IsLowStock = product.CurrentStock <= product.ReorderThreshold,
            LastModifiedAt = product.LastModifiedAt
        };
    }

    private int NormalizeLimit(int limit, string operationName)
    {
        if (limit is < 1 or > MaxToolItems)
        {
            throw CreateInputException(operationName, $"The limit must be between 1 and {MaxToolItems}. Provide a value in that range and try again.");
        }

        return limit;
    }

    private string NormalizeSid(string? value, string fieldName, string operationName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw CreateInputException(operationName, $"The {fieldName} is required. Provide a non-empty value. Use search_products to find a valid SID.");
        }

        return value.Trim();
    }

    private void EnsureUpdateHasChanges(ProductPartialUpdateRequest changes)
    {
        if (changes.ProductName == null &&
            changes.Sku == null &&
            changes.Description == null &&
            changes.CategorySid == null &&
            changes.SupplierSid == null &&
            changes.UnitPrice == null &&
            changes.CurrentStock == null &&
            changes.ReorderThreshold == null)
        {
            throw CreateInputException("update_product", "No update fields were provided. Call get_product to confirm the current values, then send only the fields you want to change.");
        }
    }

    private void ValidateOptionalLookupSid(string? value, string fieldName, string operationName)
    {
        if (value != null && string.IsNullOrWhiteSpace(value))
        {
            throw CreateInputException(operationName, $"If you provide {fieldName}, it must be a non-empty SID. Omit the field to keep the current value.");
        }
    }

    private void ValidateProductRequest(ProductRequestModel model, string operationName)
    {
        if (string.IsNullOrWhiteSpace(model.ProductName))
        {
            throw CreateInputException(operationName, "Product name is required. Provide a non-empty productName value.");
        }

        if (string.IsNullOrWhiteSpace(model.Sku))
        {
            throw CreateInputException(operationName, "SKU is required. Provide a non-empty sku value.");
        }

        if (model.UnitPrice <= 0)
        {
            throw CreateInputException(operationName, "Unit price must be greater than 0. Provide a positive unitPrice value.");
        }

        if (model.CurrentStock < 0)
        {
            throw CreateInputException(operationName, "Current stock cannot be negative. Provide 0 or a larger currentStock value.");
        }

        if (model.ReorderThreshold < 0)
        {
            throw CreateInputException(operationName, "Reorder threshold cannot be negative. Provide 0 or a larger reorderThreshold value.");
        }
    }

    private static IEnumerable<string> GetCreateFieldNames(ProductRequestModel model)
    {
        yield return nameof(model.ProductName);
        yield return nameof(model.Sku);
        yield return nameof(model.UnitPrice);
        yield return nameof(model.CurrentStock);
        yield return nameof(model.ReorderThreshold);

        if (model.Description != null)
        {
            yield return nameof(model.Description);
        }

        if (!string.IsNullOrWhiteSpace(model.CategorySid))
        {
            yield return nameof(model.CategorySid);
        }

        if (!string.IsNullOrWhiteSpace(model.SupplierSid))
        {
            yield return nameof(model.SupplierSid);
        }
    }

    private static IEnumerable<string> GetChangedFieldNames(ProductPartialUpdateRequest changes)
    {
        if (changes.ProductName != null)
        {
            yield return nameof(changes.ProductName);
        }

        if (changes.Sku != null)
        {
            yield return nameof(changes.Sku);
        }

        if (changes.Description != null)
        {
            yield return nameof(changes.Description);
        }

        if (changes.CategorySid != null)
        {
            yield return nameof(changes.CategorySid);
        }

        if (changes.SupplierSid != null)
        {
            yield return nameof(changes.SupplierSid);
        }

        if (changes.UnitPrice != null)
        {
            yield return nameof(changes.UnitPrice);
        }

        if (changes.CurrentStock != null)
        {
            yield return nameof(changes.CurrentStock);
        }

        if (changes.ReorderThreshold != null)
        {
            yield return nameof(changes.ReorderThreshold);
        }
    }

    private McpException CreateInputException(string operationName, string message)
    {
        _logger.LogWarning("Invalid MCP input for {Operation}: {Message}", operationName, message);
        return new McpException(message);
    }
}
