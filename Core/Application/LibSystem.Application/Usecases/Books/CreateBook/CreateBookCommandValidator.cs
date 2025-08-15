// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBookCommandValidator.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.CreateBook
{
    using FluentValidation;

    public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
    {
        public CreateBookCommandValidator()
        {
            this.RuleFor(x => x.title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .MinimumLength(1).WithMessage("Title cannot be empty.");

            this.RuleFor(x => x.author)
                .NotEmpty().WithMessage("Author is required.")
                .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.")
                .MinimumLength(1).WithMessage("Author cannot be empty.");

            this.RuleFor(x => x.publicationYear)
                .GreaterThanOrEqualTo(1450).WithMessage("Publication year must be 1450 or later.")
                .LessThanOrEqualTo(DateTime.Now.Year).WithMessage($"Publication year cannot be in the future.");

            this.RuleFor(x => x.category)
                .Must(BeAValidCategory).WithMessage("Category must be 0 (Fiction), 1 (History), or 2 (Child).");
        }

        private static bool BeAValidCategory(int category)
        {
            return Enum.IsDefined(typeof(Book.BookCategory), category);
        }
    }
}
