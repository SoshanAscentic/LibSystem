// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BookDto.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.DTOs.Book
{
    public class BookDto
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public int PublicationYear { get; set; }

        public string Category { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}
