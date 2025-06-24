using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Queries.GetAllBooks
{
    public record GetAllBooksQuery() : IRequest<Result<IReadOnlyList<BookDto>>>;

}
