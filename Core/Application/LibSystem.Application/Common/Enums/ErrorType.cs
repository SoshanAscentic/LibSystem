// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorType.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Common.Enums
{
    public enum ErrorType
    {
        None = 0, // No error
        Failure = 1, // General failure
        Validation = 2, // Validation error
        NotFound = 3, // Resource not found
        Conflict = 4, // Conflict error (e.g., duplicate resource)
        Unauthorized = 5, // Unauthorized access
        Forbidden = 6, // Forbidden action
    }
}
