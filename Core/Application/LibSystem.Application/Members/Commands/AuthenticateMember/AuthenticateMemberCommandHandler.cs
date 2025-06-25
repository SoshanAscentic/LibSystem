using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Members.DTOs;
using LibSystem.Application.Repositories;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Members.Commands.AuthenticateMember
{
    public class AuthenticateMemberCommandHandler : IRequestHandler<AuthenticateMemberCommand, Result<MemberDto>>
    {
        private readonly IMemberRepository memberRepository;
        private readonly IMapper mapper;
        private readonly ILogger<AuthenticateMemberCommandHandler> logger;

        public AuthenticateMemberCommandHandler(
            IMemberRepository memberRepository,
            IMapper mapper,
            ILogger<AuthenticateMemberCommandHandler> logger)
        {
            this.memberRepository = memberRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<MemberDto>> Handle(AuthenticateMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Authenticating member with ID: {MemberID}", request.MemberID);

                var memberId = MemberId.Create(request.MemberID);
                var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

                if (member == null)
                {
                    var error = "Member not found. Please sign up first.";
                    logger.LogWarning("Authentication failed for member ID: {MemberID}", request.MemberID);
                    return Result<MemberDto>.Failure(error);
                }

                var memberDto = mapper.Map<MemberDto>(member);

                logger.LogInformation("Successfully authenticated member: {Name} (ID: {MemberID})", member.Name.Value, member.MemberId.Value);

                return Result<MemberDto>.Success(memberDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during authentication for member ID: {MemberID}", request.MemberID);
                return Result<MemberDto>.Failure("An error occurred during authentication.");
            }
        }
    }
}
