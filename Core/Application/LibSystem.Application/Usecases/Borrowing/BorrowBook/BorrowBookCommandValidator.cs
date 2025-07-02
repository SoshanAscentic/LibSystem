// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorrowBookCommandValidator.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LibSystem.Application.Usecases.Borrowing.BorrowBook
{
    using FluentValidation;

    public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
    {
        public BorrowBookCommandValidator()
        {
            this.RuleFor(x => x.BookId)
                .GreaterThan(0).WithMessage("Book ID must be positive.");

            this.RuleFor(x => x.MemberID)
                .GreaterThan(0).WithMessage("Member ID must be positive.");
        }
    }
}
