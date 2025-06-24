using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Queries.GetBookById
{
    public record GetBookByIdQuery(int BookId) : IRequest<Result<BookDto>>;

}
