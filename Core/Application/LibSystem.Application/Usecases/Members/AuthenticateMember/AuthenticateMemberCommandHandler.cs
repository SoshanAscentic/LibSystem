using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.DTOs;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.AuthenticateMember
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
                if (request.MemberID <= 0)
                {
                    return Result<MemberDto>.Failure(DomainErrors.General.InvalidId("Member"));
                }

                logger.LogInformation("Authenticating member with ID: {MemberID}", request.MemberID);

                var memberId = MemberId.Create(request.MemberID);
                var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

                if (member == null)
                {
                    logger.LogWarning("Authentication failed - member not found: {MemberID}", request.MemberID);
                    return Result<MemberDto>.Failure(DomainErrors.Member.NotFound(request.MemberID));
                }

                var memberDto = mapper.Map<MemberDto>(member);

                logger.LogInformation("Successfully authenticated member: {Name} (ID: {MemberID})", member.Name.Value, member.MemberId.Value);

                return Result<MemberDto>.Success(memberDto);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID") || ex.Message.Contains("positive"))
            {
                logger.LogWarning(ex, "Invalid member ID provided: {MemberID}", request.MemberID);
                return Result<MemberDto>.Failure(DomainErrors.General.InvalidId("Member"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during authentication for member ID: {MemberID}", request.MemberID);
                return Result<MemberDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
