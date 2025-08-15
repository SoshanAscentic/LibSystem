using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Member;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.GetAllMembers
{
    public record GetAllMembersQuery() : IRequest<Result<IReadOnlyList<MemberDto>>>;

}
