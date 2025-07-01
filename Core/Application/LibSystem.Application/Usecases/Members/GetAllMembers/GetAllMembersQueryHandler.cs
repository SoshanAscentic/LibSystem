using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.DTOs.Member;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.GetAllMembers
{
    public class GetAllMembersQueryHandler : IRequestHandler<GetAllMembersQuery, Result<IReadOnlyList<MemberDto>>>
    {
        private readonly IMemberRepository memberRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetAllMembersQueryHandler> logger;

        public GetAllMembersQueryHandler(
            IMemberRepository memberRepository,
            IMapper mapper,
            ILogger<GetAllMembersQueryHandler> logger)
        {
            this.memberRepository = memberRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<IReadOnlyList<MemberDto>>> Handle(GetAllMembersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Retrieving all members");

                var members = await memberRepository.GetAllAsync(cancellationToken);
                var memberDtos = mapper.Map<IReadOnlyList<MemberDto>>(members);

                logger.LogInformation("Successfully retrieved {Count} members", memberDtos.Count);

                return Result<IReadOnlyList<MemberDto>>.Success(memberDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving all members");
                return Result<IReadOnlyList<MemberDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
