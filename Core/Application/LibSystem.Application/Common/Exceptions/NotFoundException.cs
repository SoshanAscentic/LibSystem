// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotFoundException.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        : base()
        {
        }

        public NotFoundException(string message)
        : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
        {
        }

        public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
