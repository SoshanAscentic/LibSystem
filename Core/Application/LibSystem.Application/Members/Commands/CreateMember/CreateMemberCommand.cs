using LibSystem.Application.Common.Models;
using LibSystem.Application.Members.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Members.Commands.CreateMember
{
    public record CreateMemberCommand(
        string Name,
        int MemberType
    ) : IRequest<Result<MemberDto>>;
}
