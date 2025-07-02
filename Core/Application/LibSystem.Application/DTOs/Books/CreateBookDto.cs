// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBookDto.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.DTOs.Book
{
    using System.ComponentModel.DataAnnotations;

    public class CreateBookDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Author { get; set; } = string.Empty;

        [Range(1450, 2024)]
        public int PublicationYear { get; set; }

        [Required]
        [Range(0, 2)]
        public int Category { get; set; }
    }
}
