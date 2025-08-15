// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBookByIdQuery.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetBookById
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Book;
    using MediatR;

    public record GetBookByIdQuery(int bookId)
    : IRequest<Result<BookDto>>;
}
