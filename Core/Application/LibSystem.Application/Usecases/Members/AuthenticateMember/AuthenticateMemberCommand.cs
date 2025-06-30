using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.AuthenticateMember
{
    public record AuthenticateMemberCommand(int MemberID) : IRequest<Result<MemberDto>>;

}
