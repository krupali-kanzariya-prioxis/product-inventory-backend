using FluentValidation;
using ProductInventoryTrackerAPI.Model.RequestModel;

namespace ProductInventoryTrackerAPI.Model.ValidationClass
{
    public class SupplierRequestModelValidator : AbstractValidator<SupplierRequestModel>
    {
        public SupplierRequestModelValidator()
        {
            RuleFor(s => s.SupplierName)
                .NotEmpty().WithMessage("Supplier name is required.")
                .MaximumLength(200).WithMessage("Supplier name must not exceed 200 characters.");

            RuleFor(s => s.ContactEmail)
                .EmailAddress().WithMessage("Invalid email address.")
                .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
                .When(s => !string.IsNullOrEmpty(s.ContactEmail));

            RuleFor(s => s.Phone)
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

            RuleFor(s => s.Address)
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");
        }
    }
}
