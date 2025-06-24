using AutoMapper;
using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Borrowing.DTOs;
using LibSystem.Application.Members.DTOs;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.Entities.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateBookMappings();
            CreateMemberMappings();
            CreateBorrowingMappings();
        }

        private void CreateBorrowingMappings()
        {
            //Book entity to DTO Mapping
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId.Value))
                .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationYear.Value))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

            //CreateBookDto to Book Factory Parameters
            CreateMap<CreateBookDto, Book>()
                .ConstructUsing(src => Book.Create(src.Title, src.Author, src.PublicationYear, (Book.BookCategory)src.Category));
        }

        private void CreateMemberMappings()
        {
            // Member entity to MemberDto with computed properties
            CreateMap<Member, MemberDto>()
                .ForMember(dest => dest.MemberID, opt => opt.MapFrom(src => src.MemberId.Value))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.MemberType, opt => opt.MapFrom(src => src.GetMemberType()))
                .ForMember(dest => dest.CanBorrowBooks, opt => opt.MapFrom(src => src.CanBorrowBooks()))
                .ForMember(dest => dest.CanViewBooks, opt => opt.MapFrom(src => src.CanViewBooks()))
                .ForMember(dest => dest.CanViewMembers, opt => opt.MapFrom(src => src.CanViewMembers()))
                .ForMember(dest => dest.CanManageBooks, opt => opt.MapFrom(src => src.CanManageBooks()));

            // Member to BorrowingStatusDto
            CreateMap<Member, BorrowingStatusDto>()
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId.Value))
                .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.MemberType, opt => opt.MapFrom(src => src.GetMemberType()))
                .ForMember(dest => dest.CanBorrowBooks, opt => opt.MapFrom(src => src.CanBorrowBooks()))
                .ForMember(dest => dest.CanBorrowMoreBooks, opt => opt.MapFrom(src => src.CanBorrowMoreBooks()))
                .ForMember(dest => dest.BorrowedBooks, opt => opt.Ignore());//Populated separately
        }

        private void CreateBookMappings()
        {
            // BorrowingRecord to BorrowedBookDto (requires Book information)
            CreateMap<BorrowingRecord, BorrowedBookDto>()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId.Value))
                .ForMember(dest => dest.BorrowedAt, opt => opt.MapFrom(src => src.BorrowedAt))
                .ForMember(dest => dest.DaysBorrowed, opt => opt.MapFrom(src => src.DaysBorrowed))
                .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
                .ForMember(dest => dest.Title, opt => opt.Ignore()) //Populated separately
                .ForMember(dest => dest.Author, opt => opt.Ignore());//Populated separately
        }
    }
}
