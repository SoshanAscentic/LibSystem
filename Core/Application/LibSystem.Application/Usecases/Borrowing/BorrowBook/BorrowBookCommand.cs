// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorrowBookCommand.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Borrowing.BorrowBook
{
    using LibSystem.Application.Common.Models;
    using MediatR;

    public record BorrowBookCommand(
        int BookId,
        int MemberID)
        : IRequest<Result<string>>;
}
