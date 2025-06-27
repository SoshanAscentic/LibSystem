using LibSystem.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.DeleteBook
{
    public record DeleteBookCommand(int BookId) : IRequest<Result>;

}
