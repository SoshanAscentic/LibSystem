// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingProfile.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Common.Mappings
{
    using AutoMapper;
    using LibSystem.Application.DTOs.Book;
    using LibSystem.Application.DTOs.Borrowing;
    using LibSystem.Application.DTOs.Member;
    using LibSystem.Domain.Entities.Borrowing;
    using LibSystem.Domain.Entities.Members;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.CreateBookMappings();
            this.CreateMemberMappings();
            this.CreateBorrowingMappings();
        }

        private void CreateBookMappings()
        {
            // Book entity to DTO Mapping
            this.CreateMap<Book, BookDto>()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId.Value))
                .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationYear.Value))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

            // CreateBookDto to Book Factory Parameters
            this.CreateMap<CreateBookDto, Book>()
                .ConstructUsing(src => Book.Create(src.Title, src.Author, src.PublicationYear, (Book.BookCategory)src.Category));
        }

        private void CreateMemberMappings()
        {
            // Member entity to MemberDto with computed properties
            this.CreateMap<Member, MemberDto>()
                .ForMember(dest => dest.MemberID, opt => opt.MapFrom(src => src.MemberId.Value))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.MemberType, opt => opt.MapFrom(src => src.GetMemberType()))
                .ForMember(dest => dest.BorrowedBooksCount, opt => opt.MapFrom(src => src.BorrowedBooksCount))
                .ForMember(dest => dest.CanBorrowBooks, opt => opt.MapFrom(src => src.CanBorrowBooks()))
                .ForMember(dest => dest.CanViewBooks, opt => opt.MapFrom(src => src.CanViewBooks()))
                .ForMember(dest => dest.CanViewMembers, opt => opt.MapFrom(src => src.CanViewMembers()))
                .ForMember(dest => dest.CanManageBooks, opt => opt.MapFrom(src => src.CanManageBooks()));

            // Member to BorrowingStatusDto
            this.CreateMap<Member, BorrowingStatusDto>()
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId.Value))
                .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.MemberType, opt => opt.MapFrom(src => src.GetMemberType()))
                .ForMember(dest => dest.BorrowedBooksCount, opt => opt.MapFrom(src => src.BorrowedBooksCount))
                .ForMember(dest => dest.CanBorrowBooks, opt => opt.MapFrom(src => src.CanBorrowBooks()))
                .ForMember(dest => dest.CanBorrowMoreBooks, opt => opt.MapFrom(src => src.CanBorrowMoreBooks()))
                .ForMember(dest => dest.BorrowedBooks, opt => opt.Ignore()); // Populated separately
        }

        private void CreateBorrowingMappings()
        {
            // BorrowingRecord to BorrowedBookDto (requires Book information)
            this.CreateMap<BorrowingRecord, BorrowedBookDto>()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId.Value))
                .ForMember(dest => dest.BorrowedAt, opt => opt.MapFrom(src => src.BorrowedAt))
                .ForMember(dest => dest.DaysBorrowed, opt => opt.MapFrom(src => src.DaysBorrowed))
                .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
                .ForMember(dest => dest.Title, opt => opt.Ignore()) // Populated separately
                .ForMember(dest => dest.Author, opt => opt.Ignore()); // Populated separately
        }
    }
}
