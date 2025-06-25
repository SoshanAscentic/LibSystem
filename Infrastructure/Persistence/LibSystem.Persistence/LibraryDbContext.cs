using LibSystem.Application.Common.Interfaces;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Entities.Borrowing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence
{
    public class LibraryDbContext : DbContext, IApplicationDbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        private IDbContextTransaction _currentTransaction;

        public LibraryDbContext(DbContextOptions<LibraryDbContext> options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
        }

        // DbSets for all domain entities
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<BorrowingRecord> BorrowingRecords => Set<BorrowingRecord>();
    }
