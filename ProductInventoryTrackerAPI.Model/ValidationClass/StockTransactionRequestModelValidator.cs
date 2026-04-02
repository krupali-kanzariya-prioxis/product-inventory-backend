using FluentValidation;
using ProductInventoryTrackerAPI.Model.RequestModel;

namespace ProductInventoryTrackerAPI.Model.ValidationClass
{
    public class StockTransactionRequestModelValidator : AbstractValidator<StockTransactionRequestModel>
    {
        public StockTransactionRequestModelValidator()
        {
            RuleFor(s => s.ProductSid)
                .NotEmpty().WithMessage("Product SID is required.");

            RuleFor(s => s.TransactionType)
                .NotEmpty().WithMessage("Transaction type is required.")
                .MaximumLength(10).WithMessage("Transaction type must not exceed 10 characters.")
                .Must(x => x == "IN" || x == "OUT").WithMessage("Transaction type must be either 'IN' or 'OUT'.");

            RuleFor(s => s.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(s => s.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
        }
    }
}
