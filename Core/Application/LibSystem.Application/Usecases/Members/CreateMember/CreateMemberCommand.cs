using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Member;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.CreateMember
{
    public record CreateMemberCommand(
        string Name,
        int MemberType
    ) : IRequest<Result<MemberDto>>;
}
