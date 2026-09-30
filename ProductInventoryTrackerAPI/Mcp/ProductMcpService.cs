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

    public ProductMcpService(IProductRepository productRepository, ProductInventoryDBContext dbContext)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
    }

    public async Task<ProductListToolResult> SearchProductsAsync(string? searchText, bool lowStockOnly, int limit)
    {
        var normalizedLimit = NormalizeLimit(limit);
        var parameters = CreateSearchParameters(searchText, normalizedLimit);

        var page = lowStockOnly
            ? await _productRepository.GetLowStockProductsAsync(parameters)
            : await _productRepository.GetProductsAsync(parameters);

        var products = ExtractProducts(page.Result)
            .Take(normalizedLimit)
            .Select(MapToMcpItem)
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
        var normalizedSid = NormalizeSid(productSid, "product SID");
        var product = await _productRepository.GetProductBySidAsync(normalizedSid);

        if (product == null)
        {
            throw CreateInputException($"No active product was found for SID '{normalizedSid}'.");
        }

        return MapToMcpItem(product);
    }

    public async Task<ProductMcpItem> CreateProductAsync(ProductRequestModel model, CancellationToken cancellationToken)
    {
        ValidateProductRequest(model);
        var created = await _productRepository.AddProductAsync(model);
        return MapToMcpItem(created);
    }

    public async Task<ProductMcpItem> UpdateProductAsync(string productSid, ProductRequestModel model, CancellationToken cancellationToken)
    {
        var normalizedSid = NormalizeSid(productSid, "product SID");
        ValidateProductRequest(model);

        var updated = await _productRepository.UpdateProductAsync(normalizedSid, model);
        if (updated == null)
        {
            throw CreateInputException($"No active product was found for SID '{normalizedSid}'.");
        }

        return MapToMcpItem(updated);
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
        var normalizedSid = NormalizeSid(productSid, "product SID");
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .Include(x => x.StockTransactions)
            .SingleOrDefaultAsync(x => x.ProductSid == normalizedSid && x.Status == (int)StatusTypeDB.Active, cancellationToken);

        if (product == null)
        {
            throw CreateInputException($"No active product was found for SID '{normalizedSid}'.");
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

    private static ProductMcpItem MapToMcpItem(ProductResponseModel product)
    {
        return new ProductMcpItem
        {
            ProductSid = product.ProductSid,
            ProductName = product.ProductName,
            Sku = product.Sku,
            CategoryName = product.CategoryName,
            SupplierName = product.SupplierName,
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
            CategoryName = product.Category?.CategoryName,
            SupplierName = product.Supplier?.SupplierName,
            UnitPrice = product.UnitPrice,
            CurrentStock = product.CurrentStock,
            ReorderThreshold = product.ReorderThreshold,
            IsLowStock = product.CurrentStock <= product.ReorderThreshold,
            LastModifiedAt = product.LastModifiedAt
        };
    }

    private static int NormalizeLimit(int limit)
    {
        if (limit is < 1 or > MaxToolItems)
        {
            throw CreateInputException($"The limit must be between 1 and {MaxToolItems}.");
        }

        return limit;
    }

    private static string NormalizeSid(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw CreateInputException($"The {fieldName} is required.");
        }

        return value.Trim();
    }

    private static void ValidateProductRequest(ProductRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ProductName))
        {
            throw CreateInputException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Sku))
        {
            throw CreateInputException("SKU is required.");
        }

        if (model.UnitPrice <= 0)
        {
            throw CreateInputException("Unit price must be greater than 0.");
        }

        if (model.CurrentStock < 0)
        {
            throw CreateInputException("Current stock cannot be negative.");
        }

        if (model.ReorderThreshold < 0)
        {
            throw CreateInputException("Reorder threshold cannot be negative.");
        }
    }

    private static Exception CreateInputException(string message)
        => new InvalidOperationException(message);
}
