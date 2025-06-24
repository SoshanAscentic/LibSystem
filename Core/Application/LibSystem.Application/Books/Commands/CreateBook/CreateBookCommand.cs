using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Commands.CreateBook
{

    public record CreateBookCommand(
        string Title,
        string Author,
        int PublicationYear,
        int Category
    ) : IRequest<Result<BookDto>>;
}
