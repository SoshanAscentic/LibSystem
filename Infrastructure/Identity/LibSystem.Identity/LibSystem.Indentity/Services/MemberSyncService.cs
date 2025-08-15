using LibSystem.Application.Common.Models;
using LibSystem.Application.Constants;
using LibSystem.Application.Contracts.Identity;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.Contracts.UoW;
using LibSystem.Domain.Entities.Members;
using LibSystem.Domain.ValueObjects;
using LibSystem.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LibSystem.Identity.Services
{
    public class MemberSyncService : IMemberSyncService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMemberRepository memberRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<MemberSyncService> logger;

        public MemberSyncService(
            UserManager<ApplicationUser> userManager,
            IMemberRepository memberRepository,
            IUnitOfWork unitOfWork,
            ILogger<MemberSyncService> logger)
        {
            this.userManager = userManager;
            this.memberRepository = memberRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<int?>> GetMemberIdForUserAsync(int userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result<int?>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                return Result<int?>.Success(user.MemberId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting member ID for user {UserId}", userId);
                return Result<int?>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> SyncUserMemberAsync(int userId, string fullName, string role)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                // If user doesn't have a member, create one
                if (!user.MemberId.HasValue)
                {
                    var createResult = await CreateMemberForUserAsync(userId, fullName, role);
                    return createResult.IsSuccess ? Result.Success() : Result.Failure(createResult.Error);
                }

                // Update existing member if needed
                var member = await memberRepository.GetByIdAsync(MemberId.Create(user.MemberId.Value));
                if (member != null && member.Name.Value != fullName)
                {
                    member.UpdateName(fullName);
                    memberRepository.Update(member);
                    await unitOfWork.SaveChangesAsync();

                    logger.LogInformation("Synced member name for member {MemberId}", member.Id);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing user member for user {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<int>> CreateMemberForUserAsync(int userId, string fullName, string role)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result<int>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                // Create domain member based on role
                var domainMember = CreateDomainMemberByRole(fullName, role);
                await memberRepository.AddAsync(domainMember);
                await unitOfWork.SaveChangesAsync();

                // Update user with member ID
                user.MemberId = domainMember.Id;
                await userManager.UpdateAsync(user);

                logger.LogInformation("Created domain member {MemberId} for user {UserId}",
                    domainMember.Id, userId);

                return Result<int>.Success(domainMember.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating domain member for user {UserId}", userId);
                return Result<int>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        private static Member CreateDomainMemberByRole(string name, string role)
        {
            return role switch
            {
                ApplicationRoles.Member => new RegularMember(name),
                ApplicationRoles.MinorStaff => new MinorStaff(name),
                ApplicationRoles.ManagementStaff => new ManagementStaff(name),
                _ => new RegularMember(name)
            };
        }
    }
}