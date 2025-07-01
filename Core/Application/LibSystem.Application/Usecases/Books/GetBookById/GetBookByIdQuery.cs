using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Book;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.GetBookById
{
    public record GetBookByIdQuery(int BookId) : IRequest<Result<BookDto>>;

}
