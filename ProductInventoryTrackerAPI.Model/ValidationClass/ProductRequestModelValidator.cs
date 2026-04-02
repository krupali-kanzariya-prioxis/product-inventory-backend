using FluentValidation;
using ProductInventoryTrackerAPI.Model.RequestModel;

namespace ProductInventoryTrackerAPI.Model.ValidationClass
{
    public class ProductRequestModelValidator : AbstractValidator<ProductRequestModel>
    {
        public ProductRequestModelValidator()
        {
            RuleFor(p => p.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(p => p.Sku)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(p => p.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0.");

            RuleFor(p => p.CurrentStock)
                .GreaterThanOrEqualTo(0).WithMessage("Current stock cannot be negative.");

            RuleFor(p => p.ReorderThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Reorder threshold cannot be negative.");
        }
    }
}
