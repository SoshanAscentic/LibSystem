// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetMemberBorrowingStatusQuery.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Borrowing.GetMemberBorrowingStatus
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Borrowing;
    using MediatR;
    public record GetMemberBorrowingStatusQuery(int MemberId) : IRequest<Result<BorrowingStatusDto>>;
}
