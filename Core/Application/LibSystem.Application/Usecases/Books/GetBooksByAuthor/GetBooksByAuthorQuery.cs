// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBooksByAuthorQuery.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetBooksByAuthor
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Book;
    using MediatR;

    public record GetBooksByAuthorQuery(string author)
    : IRequest<Result<IReadOnlyList<BookDto>>>;
}
