using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.GetBooksByCategory
{
    public record GetBooksByCategoryQuery(string Category) : IRequest<Result<IReadOnlyList<BookDto>>>;

}
