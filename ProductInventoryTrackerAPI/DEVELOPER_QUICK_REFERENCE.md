# Developer Quick Reference - New Code Structure

## Adding a New Entity

Follow this step-by-step guide to add a new entity to the system:

### Step 1: Create Request/Response Models
```
Location: ProductInventoryTrackerAPI.Model/RequestModel
Location: ProductInventoryTrackerAPI.Model/ResponseModel

Pattern:
- {EntityName}RequestModel.cs (for POST/PUT)
- {EntityName}ResponseModel.cs (for GET responses)
```

Example:
```csharp
// ProductRequestModel.cs
public class ProductRequestModel
{
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    // ... other fields
}

// ProductResponseModel.cs
public class ProductResponseModel
{
    public string ProductSid { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    // ... other fields
}
```

### Step 2: Create Validator
```
Location: ProductInventoryTrackerAPI.Model/ValidationClass
File: {EntityName}RequestModelValidator.cs

Pattern:
```csharp
public class ProductRequestModelValidator : AbstractValidator<ProductRequestModel>
{
    public ProductRequestModelValidator()
    {
        RuleFor(p => p.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Max 200 characters.");
    }
}
```

### Step 3: Create Repository Interface
```
Location: ProductInventoryTrackerAPI.Service/Repository/Interfaces
File: I{EntityName}Repository.cs

Pattern:
```csharp
public interface IProductRepository
{
    Task<Page> GetProductsAsync(Dictionary<string, object> parameters);
    Task<ProductResponseModel?> GetProductBySidAsync(string productSid);
    Task<ProductResponseModel> AddProductAsync(ProductRequestModel model);
    Task<ProductResponseModel> UpdateProductAsync(string productSid, ProductRequestModel model);
    Task<bool> DeleteProductAsync(string productSid);
    Task<IEnumerable<SelectListItem>?> DDLProductAsync(
        List<string>? filterSid = null, List<int>? filterId = null);
}
```

### Step 4: Create Repository Implementation
```
Location: ProductInventoryTrackerAPI.Service/Repository/Implementation
File: {EntityName}Repository.cs

Pattern:
```csharp
public class ProductRepository : IProductRepository
{
    private readonly ProductInventoryDBContext _context;
    private readonly ILogger<ProductRepository> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductRepository(
        ProductInventoryDBContext context,
        ILogger<ProductRepository> logger,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Page> GetProductsAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var query = _context.Products
                .Where(x => x.Status == (int)StatusTypeDB.Active)
                .AsQueryable();

            var pageStart = Convert.ToInt32(parameters[Constants.SearchParameters.PageStart]);
            var pageSize = Convert.ToInt32(parameters[Constants.SearchParameters.PageSize]);
            var searchText = parameters[Constants.SearchParameters.SearchText]?.ToString() ?? "%";

            if (searchText != "%")
            {
                query = query.Where(x => x.ProductName.Contains(searchText));
            }

            var total = await query.CountAsync();
            var products = await query
                .Skip((pageStart - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToResponse(p))
                .ToListAsync();

            var page = new Page { Result = products };
            page.Meta.TotalResults = total;
            return page;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching products: {Message}", e.Message);
            throw new HttpStatusCodeException(500, e.Message);
        }
    }

    // ... implement other methods
    
    private static ProductResponseModel MapToResponse(Product product)
    {
        return new ProductResponseModel
        {
            ProductSid = product.ProductSid,
            ProductName = product.ProductName,
            // ... other mappings
        };
    }
}
```

### Step 5: Create Controller
```
Location: Controllers
File: {EntityName}sController.cs

Pattern:
```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductsController : BaseController
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<Page>> GetAllProducts([FromQuery] SearchRequestModel model)
    {
        try
        {
            var parameters = FillParamesFromModel(model);
            var list = await _productRepository.GetProductsAsync(parameters);
            return Ok(BindSearchResult(list, model, "Product List"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            return StatusCode(400, $"Bad Request Error: {ex.Message}");
        }
    }

    [HttpGet("{productSid}")]
    public async Task<ActionResult<ProductResponseModel>> GetProductBySid(string productSid)
    {
        try
        {
            var product = await _productRepository.GetProductBySidAsync(productSid);
            if (product == null)
                throw new HttpStatusCodeException(404, $"Product with SID '{productSid}' not found.");
            return Ok(product);
        }
        catch (HttpStatusCodeException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product");
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // ... implement other endpoints (Add, Update, Delete, DDL)
}
```

### Step 6: Register in Program.cs
```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

---

## Common Patterns Used

### UnitOfWork Pattern
```csharp
// Get single item
var product = await _unitOfWork
    .GetRepository<Product>()
    .SingleOrDefaultAsync(x => x.ProductSid == sid);

// Get all items
var products = await _unitOfWork
    .GetRepository<Product>()
    .GetAllAsync(x => x.Status == (int)StatusTypeDB.Active);

// Insert
await _unitOfWork.GetRepository<Product>().InsertAsync(entity);
await _unitOfWork.CommitAsync();

// Update
_unitOfWork.GetRepository<Product>().Update(entity);
await _unitOfWork.CommitAsync();
```

### Error Handling Pattern
```csharp
try
{
    // Logic here
    return Ok(result);
}
catch (HttpStatusCodeException ex)
{
    return StatusCode(ex.StatusCode, ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Detailed error message: {Message}", ex.Message);
    return StatusCode(500, $"Unexpected error: {ex.Message}");
}
```

### Soft Delete Pattern
```csharp
entity.Status = (int)StatusTypeDB.Delete;
entity.LastModifiedAt = DateTime.UtcNow;
_unitOfWork.GetRepository<Entity>().Update(entity);
await _unitOfWork.CommitAsync();
```

### Pagination Pattern
```csharp
var page = new Page { Result = items };
page.Meta.TotalResults = total;
return page;

// In controller
var parameters = FillParamesFromModel(model);
var list = await _repository.GetItemsAsync(parameters);
return Ok(BindSearchResult(list, model, "Item List"));
```

### Validation Pattern
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);

// Validator will automatically validate via FluentValidation
```

---

## Important Constants

### Search Parameters (Used in FillParamesFromModel)
```csharp
Constants.SearchParameters.PageStart      // Page number
Constants.SearchParameters.PageSize       // Records per page
Constants.SearchParameters.SearchText     // Search keyword
Constants.SearchParameters.SortColumn     // Column to sort by
Constants.SearchParameters.SortOrder      // ASC or DESC
Constants.SearchParameters.Filters        // JSON filters
```

### Status Values
```csharp
StatusTypeDB.Active = 1       // Active record
StatusTypeDB.Delete = 3       // Soft deleted
```

### SID Prefixes
```
Categories:  "CAT" + 13 char hex
Products:    "PRD" + 13 char hex
Suppliers:   "SUP" + 13 char hex
Users:       "USR" + 13 char hex
Transactions:"STX" + 13 char hex
```

---

## BaseController Available Methods

### FillParamesFromModel
```csharp
// Extracts and validates search parameters
var parameters = FillParamesFromModel(searchModel);

// Returns Dictionary with:
// - PageStart
// - PageSize
// - SearchText
// - SortColumn
// - SortOrder
// - Filters
```

### BindSearchResult
```csharp
// Updates pagination metadata
var result = BindSearchResult(page, searchModel, "Entity List");

// Sets:
// - meta.page
// - meta.page_size
// - meta.total_results
// - meta.total_page_num
// - meta.url
// - meta.first_page_url
// - meta.next_page_url
// - meta.previous_page_url
// - meta.key
```

### ToEscapeXml
```csharp
// Escapes XML special characters for safety
string escaped = ToEscapeXml(userInput);
```

### GetFilterConditionFromModel
```csharp
// Converts JSON filters to SQL conditions
string condition = GetFilterConditionFromModel(jsonFilterString);
```

---

## Endpoint Naming Convention

For entity {Entity}:

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/{entities}` | List all with pagination |
| GET | `/api/{entities}/{sid}` | Get by SID |
| POST | `/api/{entities}/Add{Entity}` | Create new |
| POST | `/api/{entities}/Update{Entity}/{sid}` | Update by SID |
| DELETE | `/api/{entities}/{sid}` | Soft delete by SID |
| GET | `/api/{entities}/DDL{Entity}` | Get dropdown list |

Example for Products:
- GET `/api/products`
- GET `/api/products/{sid}`
- POST `/api/products/AddProduct`
- POST `/api/products/UpdateProduct/{sid}`
- DELETE `/api/products/{sid}`
- GET `/api/products/DDLProduct`

---

## Common Mistakes to Avoid

❌ **DO NOT:**
- Use `_context.SaveChangesAsync()` directly - Use `_unitOfWork.CommitAsync()`
- Return null for error cases - Throw `HttpStatusCodeException`
- Skip logging - Always log errors with context
- Forget to check `ModelState.IsValid` in controllers
- Use hardcoded strings - Use `Constants` class
- Forget `.Where(x => x.Status == (int)StatusTypeDB.Active)` in queries

✅ **DO:**
- Use UnitOfWork for all database operations
- Use HttpStatusCodeException for API errors
- Log all exceptions with context
- Validate ModelState in controllers
- Use Constants for magic values
- Filter soft-deleted records in queries

---

## Testing Endpoints

### List Example
```bash
curl "http://localhost:5000/api/categories?page=1&pageSize=10&searchText=electronics&sortColumn=CreatedAt&sortOrder=DESC"
```

### Get by SID Example
```bash
curl "http://localhost:5000/api/categories/CAT1A2B3C4D5E6F7"
```

### Add Example
```bash
curl -X POST "http://localhost:5000/api/categories/AddCategory" \
  -H "Content-Type: application/json" \
  -d '{"categoryName":"Electronics","description":"Electronic devices"}'
```

### Update Example
```bash
curl -X POST "http://localhost:5000/api/categories/UpdateCategory/CAT1A2B3C4D5E6F7" \
  -H "Content-Type: application/json" \
  -d '{"categoryName":"Updated Name","description":"Updated description"}'
```

### Delete Example
```bash
curl -X DELETE "http://localhost:5000/api/categories/CAT1A2B3C4D5E6F7"
```

### Dropdown Example
```bash
curl "http://localhost:5000/api/categories/DDLCategory"
```

---

**Reference Version:** 1.0
**Last Updated:** 2024
**Framework:** .NET 10 with UnitOfWork Pattern
