//using FluentValidation;
//using ProductInventoryTrackerAPI.Model.RequestModel;
//using System;

//namespace ProductInventoryTrackerAPI.Model.ValidationClass
//{
//    public class AnnouncementRequestModelValidator
//        : AbstractValidator<AnnouncementRequestModel>
//    {
//        public AnnouncementRequestModelValidator()
//        {
//            RuleFor(a => a.Title)
//                .NotEmpty().WithMessage("Title must not be empty.")
//                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

//            RuleFor(a => a.Body)
//                .MaximumLength(5000).WithMessage("Body must not exceed 5000 characters.");

//            RuleFor(a => a.StartDate)
//                .NotEmpty().WithMessage("Start date is required.")
//                .LessThan(a => a.EndDate)
//                .WithMessage("Start date must be earlier than end date.");

//            RuleFor(a => a.EndDate)
//                .NotEmpty().WithMessage("End date is required.")
//                .GreaterThan(a => a.StartDate)
//                .WithMessage("End date must be later than start date.");
//        }
//    }
//}