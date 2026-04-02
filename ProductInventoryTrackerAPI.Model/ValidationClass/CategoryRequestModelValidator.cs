using FluentValidation;
using ProductInventoryTrackerAPI.Model.RequestModel;

namespace ProductInventoryTrackerAPI.Model.ValidationClass
{
    public class CategoryRequestModelValidator : AbstractValidator<CategoryRequestModel>
    {
        public CategoryRequestModelValidator()
        {
            RuleFor(c => c.CategoryName)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

            RuleFor(c => c.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }
    }
}
