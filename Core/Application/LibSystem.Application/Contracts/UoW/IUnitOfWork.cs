// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMemberRepository.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.UoW
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        bool HasActiveTransaction { get; }

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

        Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
    }
}
