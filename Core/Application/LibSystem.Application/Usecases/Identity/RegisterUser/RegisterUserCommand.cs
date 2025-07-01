using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Identity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Identity.RegisterUser
{
    public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword,
        string Role
    ) : IRequest<Result<AuthenticationResponse>>;
}
