using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Identity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Identity.GetAllUsers
{
    public record GetAllUsersQuery() : IRequest<Result<IReadOnlyList<UserDto>>>;
    public record GetUserByIdQuery(int UserId) : IRequest<Result<UserDto>>;

}
