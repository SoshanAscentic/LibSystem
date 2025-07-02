// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBooksByCategoryQuery.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetBooksByCategory
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Book;
    using MediatR;

    public record GetBooksByCategoryQuery(string category)
    : IRequest<Result<IReadOnlyList<BookDto>>>;
}
