using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Queries.GetBooksByAuthor
{
    public record GetBooksByAuthorQuery(string Author) : IRequest<Result<IReadOnlyList<BookDto>>>;

}
