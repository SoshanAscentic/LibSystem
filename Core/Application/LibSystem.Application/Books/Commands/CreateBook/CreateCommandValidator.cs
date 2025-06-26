using FluentValidation;
using LibSystem.Domain.Entities.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Commands.CreateBook
{
    public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
    {
        public CreateBookCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .MinimumLength(1).WithMessage("Title cannot be empty.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.")
                .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.")
                .MinimumLength(1).WithMessage("Author cannot be empty.");

            RuleFor(x => x.PublicationYear)
                .GreaterThanOrEqualTo(1450).WithMessage("Publication year must be 1450 or later.")
                .LessThanOrEqualTo(DateTime.Now.Year).WithMessage($"Publication year cannot be in the future.");

            RuleFor(x => x.Category)
                .Must(BeAValidCategory).WithMessage("Category must be 0 (Fiction), 1 (History), or 2 (Child).");
        }

        private static bool BeAValidCategory(int category)
        {
            return Enum.IsDefined(typeof(Book.BookCategory), category);
        }
    }
}
