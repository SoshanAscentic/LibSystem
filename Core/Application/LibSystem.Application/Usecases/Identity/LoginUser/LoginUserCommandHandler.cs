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

namespace LibSystem.Application.Usecases.Identity.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthenticationResponse>>
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<LoginUserCommandHandler> logger;

        public LoginUserCommandHandler(
            IAuthenticationService authenticationService,
            ILogger<LoginUserCommandHandler> logger)
        {
            this.authenticationService = authenticationService;
            this.logger = logger;
        }

        public async Task<Result<AuthenticationResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing login request for email: {Email}", request.Email);

            var loginRequest = new LoginRequest
            {
                Email = request.Email,
                Password = request.Password,
                RememberMe = request.RememberMe
            };

            var result = await authenticationService.LoginAsync(loginRequest);

            if (result.IsSuccess)
            {
                logger.LogInformation("User logged in successfully: {Email}", request.Email);
            }
            else
            {
                logger.LogWarning("Login failed for email: {Email}", request.Email);
            }

            return result;
        }
    }
}
