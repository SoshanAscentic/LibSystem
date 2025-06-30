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

namespace LibSystem.Application.Usecases.Members.GetMembersById
{
    public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, Result<MemberDto>>
    {
        private readonly IMemberRepository memberRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetMemberByIdQueryHandler> logger;

        public GetMemberByIdQueryHandler(
            IMemberRepository memberRepository,
            IMapper mapper,
            ILogger<GetMemberByIdQueryHandler> logger)
        {
            this.memberRepository = memberRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<MemberDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.MemberId <= 0)
                {
                    return Result<MemberDto>.Failure(DomainErrors.General.InvalidId("Member"));
                }

                logger.LogInformation("Retrieving member with ID: {MemberId}", request.MemberId);

                var memberId = MemberId.Create(request.MemberId);
                var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

                if (member == null)
                {
                    logger.LogWarning("Member not found: {MemberId}", request.MemberId);
                    return Result<MemberDto>.Failure(DomainErrors.Member.NotFound(request.MemberId));
                }

                var memberDto = mapper.Map<MemberDto>(member);

                logger.LogInformation("Successfully retrieved member: {Name}", member.Name.Value);

                return Result<MemberDto>.Success(memberDto);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID") || ex.Message.Contains("positive"))
            {
                logger.LogWarning(ex, "Invalid member ID provided: {MemberId}", request.MemberId);
                return Result<MemberDto>.Failure(DomainErrors.General.InvalidId("Member"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving member with ID: {MemberId}", request.MemberId);
                return Result<MemberDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}

