using LibSystem.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Contracts.Identity
{
    public interface IMemberSyncService
    {
        Task<Result<int?>> GetMemberIdForUserAsync(int userId);
        Task<Result> SyncUserMemberAsync(int userId, string fullName, string role);
        Task<Result<int>> CreateMemberForUserAsync(int userId, string fullName, string role);
    }
}