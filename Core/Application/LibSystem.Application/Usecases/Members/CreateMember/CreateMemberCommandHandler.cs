using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.Contracts.UoW;
using LibSystem.Application.DTOs;
using LibSystem.Domain.Entities.Members;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.CreateMember
{
    public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<MemberDto>>
    {
        private readonly IMemberRepository memberRepository;
        private readonly IUnitOfWork unitOfWork; 
        private readonly IMapper mapper;
        private readonly ILogger<CreateMemberCommandHandler> logger;

        public CreateMemberCommandHandler(
            IMemberRepository memberRepository,
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            ILogger<CreateMemberCommandHandler> logger)
        {
            this.memberRepository = memberRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<MemberDto>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Creating member: {Name}, Type: {MemberType}", request.Name, request.MemberType);

                // Create member using factory method
                var member = CreateMemberByType(request.Name.Trim(), request.MemberType);

                // Add to repository (this just stages the change)
                await memberRepository.AddAsync(member, cancellationToken);

                // Save through UnitOfWork (this persists everything)
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                var memberDto = mapper.Map<MemberDto>(member);

                logger.LogInformation("Successfully created member with ID: {MemberId}", member.MemberId.Value);

                return Result<MemberDto>.Success(memberDto);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid member data provided");
                return Result<MemberDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating member: {Name}, Type: {MemberType}", request.Name, request.MemberType);
                return Result<MemberDto>.Failure("An error occurred while creating the member.");
            }
        }

        private static Member CreateMemberByType(string name, int memberType)
        {
            return memberType switch
            {
                0 => new RegularMember(name),
                1 => new MinorStaff(name),
                2 => new ManagementStaff(name),
                _ => throw new ArgumentException($"Invalid member type: {memberType}")
            };
        }
    }
}
