using ProductInventoryTrackerAPI.Model.ProductInventoryDB;
using ProductInventoryTrackerAPI.Model.SpDbContext;
using ProductInventoryTrackerAPI.Service.Repository.Interfaces;
using ProductInventoryTrackerAPI.Service.Repository.Implementation;
using ProductInventoryTrackerAPI.Service.UnitOfWork;
using ProductInventoryTrackerAPI.Helper;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using ProductInventoryTrackerAPI.Model.RequestModel;
using ProductInventoryTrackerAPI.Model.ValidationClass;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration["DbConnectionString"];

// Add DbContext
builder.Services.AddDbContext<ProductInventoryDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ProductSpContext>(options =>
    options.UseSqlServer(connectionString));

// Add UnitOfWork
builder.Services.AddUnitOfWork<ProductInventoryDBContext>();

// Add Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<ProductInventoryTrackerAPI.Mcp.ProductMcpService>();

// Add Validators
builder.Services.AddValidatorsFromAssemblyContaining(typeof(CategoryRequestModelValidator));
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000", "http://localhost:54108")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapMcp("/mcp");
app.Run();
