using LibSystem.Application.Borrowing.DTOs;
using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Borrowing.Queries.GetMemberBorrowingStatus
{
    public record GetMemberBorrowingStatusQuery(int MemberId) : IRequest<Result<BorrowingStatusDto>>;

}
