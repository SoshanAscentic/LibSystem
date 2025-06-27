using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Borrowing.GetMemberBorrowingStatus
{
    public record GetMemberBorrowingStatusQuery(int MemberId) : IRequest<Result<BorrowingStatusDto>>;

}
