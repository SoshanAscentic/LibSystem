// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteBookCommandValidator.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.DeleteBook
{
    using FluentValidation;

    public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
    {
        public DeleteBookCommandValidator()
        {
            this.RuleFor(x => x.bookId)
                .GreaterThan(0).WithMessage("Book ID must be a positive integer.");
        }
    }
}
