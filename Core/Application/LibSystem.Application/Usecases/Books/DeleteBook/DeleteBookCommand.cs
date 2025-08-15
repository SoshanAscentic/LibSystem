// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteBookCommand.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.DeleteBook
{
    using LibSystem.Application.Common.Models;
    using MediatR;

    public record DeleteBookCommand(int bookId)
    : IRequest<Result>;
}
