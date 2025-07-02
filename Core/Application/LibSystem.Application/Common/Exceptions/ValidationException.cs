// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationException.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Common.Exceptions
{
    using FluentValidation.Results;

    public class ValidationException : Exception
    {
        public ValidationException()
        : base("One or more validation failures have occurred.")
        {
            this.Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
        {
            this.Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        public ValidationException(string propertyName, string errorMessage)
        : this()
        {
            this.Errors = new Dictionary<string, string[]> { { propertyName, new[] { errorMessage } } };
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
