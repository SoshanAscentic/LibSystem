using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Borrowing.Commands.ReturnBook
{
    public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
    {
        public ReturnBookCommandValidator()
        {
            RuleFor(x => x.BookId)
                .GreaterThan(0).WithMessage("Book ID must be positive.");

            RuleFor(x => x.MemberID)
                .GreaterThan(0).WithMessage("Member ID must be positive.");
        }
    }
}
