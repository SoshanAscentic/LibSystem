// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBookCommand.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.CreateBook
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Book;
    using MediatR;

    public record CreateBookCommand(
        string title,
        string author,
        int publicationYear,
        int category)
        : IRequest<Result<BookDto>>;
}
