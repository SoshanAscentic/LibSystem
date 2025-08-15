// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMemberSyncService.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.Identity
{
    using LibSystem.Application.Common.Models;

    public interface IMemberSyncService
    {
        Task<Result<int?>> GetMemberIdForUserAsync(int userId);

        Task<Result> SyncUserMemberAsync(int userId, string fullName, string role);

        Task<Result<int>> CreateMemberForUserAsync(int userId, string fullName, string role);
    }
}
