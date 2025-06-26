using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Borrowing.Commands.BorrowBook
{
    public record BorrowBookCommand(
        int BookId,
        int MemberID
    ) : IRequest<Result<string>>;
}
