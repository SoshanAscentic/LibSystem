using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Members.DTOs;
using LibSystem.Domain.Repositories;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Members.Queries.GetMembersById
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
                    return Result<MemberDto>.Failure("Member ID must be positive.");
                }

                logger.LogInformation("Retrieving member with ID: {MemberId}", request.MemberId);

                var memberId = MemberId.Create(request.MemberId);
                var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

                if (member == null)
                {
                    var error = $"Member with ID {request.MemberId} was not found.";
                    logger.LogWarning(error);
                    return Result<MemberDto>.Failure(error);
                }

                var memberDto = mapper.Map<MemberDto>(member);

                logger.LogInformation("Successfully retrieved member: {Name}", member.Name.Value);

                return Result<MemberDto>.Success(memberDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving member with ID: {MemberId}", request.MemberId);
                return Result<MemberDto>.Failure("An error occurred while retrieving the member.");
            }
        }
    }
}

