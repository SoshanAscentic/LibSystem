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

namespace LibSystem.Application.Usecases.Identity.GetAllUsers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserManagementService userManagementService;
        private readonly ILogger<GetUserByIdQueryHandler> logger;

        public GetUserByIdQueryHandler(
            IUserManagementService userManagementService,
            ILogger<GetUserByIdQueryHandler> logger)
        {
            this.userManagementService = userManagementService;
            this.logger = logger;
        }

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving user with ID: {UserId}", request.UserId);

            var result = await userManagementService.GetUserByIdAsync(request.UserId);

            if (result.IsSuccess)
            {
                logger.LogInformation("Successfully retrieved user with ID: {UserId}", request.UserId);
            }

            return result;
        }
    }
}
