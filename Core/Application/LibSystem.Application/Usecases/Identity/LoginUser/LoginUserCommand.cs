using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Identity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Identity.LoginUser
{
    public record LoginUserCommand(
        string Email,
        string Password,
        bool RememberMe = false
    ) : IRequest<Result<AuthenticationResponse>>;
}
