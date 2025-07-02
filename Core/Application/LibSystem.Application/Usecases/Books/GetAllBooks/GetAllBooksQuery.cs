// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAllBooksQuery.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetAllBooks
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Book;
    using MediatR;

    public record GetAllBooksQuery() : IRequest<Result<IReadOnlyList<BookDto>>>;

}
