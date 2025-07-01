using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Identity;
using LibSystem.Application.DTOs.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Identity.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthenticationResponse>>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<RegisterUserCommandHandler> logger;

        public RegisterUserCommandHandler(
            IAuthenticationService authenticationService,
            ILogger<RegisterUserCommandHandler> logger)
        {
            this.authenticationService = authenticationService;
            this.logger = logger;
        }

        public async Task<Result<AuthenticationResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing registration request for email: {Email}", request.Email);

            var registerRequest = new RegisterRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                Role = request.Role
            };

            var result = await authenticationService.RegisterAsync(registerRequest);

            if (result.IsSuccess)
            {
                logger.LogInformation("User registered successfully: {Email}", request.Email);
            }
            else
            {
                logger.LogWarning("Registration failed for email: {Email}", request.Email);
            }

            return result;
        }
    }
}
