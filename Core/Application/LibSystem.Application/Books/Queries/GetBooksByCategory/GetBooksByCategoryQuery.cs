using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Queries.GetBooksByCategory
{
    public record GetBooksByCategoryQuery(string Category) : IRequest<Result<IReadOnlyList<BookDto>>>;

}
