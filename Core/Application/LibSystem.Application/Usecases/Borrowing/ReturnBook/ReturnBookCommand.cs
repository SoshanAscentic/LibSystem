using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Borrowing.ReturnBook
{
    public record ReturnBookCommand(
        int BookId,
        int MemberID
    ) : IRequest<Result<string>>;
}
