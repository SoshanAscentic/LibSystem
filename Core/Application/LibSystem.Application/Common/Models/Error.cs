// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Error.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Common.Models
{
    using System;
    using System.Collections.Generic;
    using LibSystem.Application.Common.Enums;

    public sealed class Error : IEquatable<Error>
    {
        public static readonly Error None = new (string.Empty, string.Empty, ErrorType.None);
        public static readonly Error NullValue = new ("Error.NullValue", "Null value was provided", ErrorType.Failure);

        public Error(string code, string message, ErrorType type)
        {
            this.Code = code;
            this.Message = message;
            this.Type = type;
        }

        public string Code { get; }

        public string Message { get; }

        public ErrorType Type { get; }

        public static bool operator ==(Error? left, Error? right) =>
            EqualityComparer<Error>.Default.Equals(left, right);

        public static bool operator !=(Error? left, Error? right) =>
            !(left == right);

        public static Error Failure(string code, string message) =>
            new (code, message, ErrorType.Failure);

        public static Error Validation(string code, string message) =>
            new (code, message, ErrorType.Validation);

        public static Error NotFound(string code, string message) =>
            new (code, message, ErrorType.NotFound);

        public static Error Conflict(string code, string message) =>
            new (code, message, ErrorType.Conflict);

        public static Error Unauthorized(string code, string message) =>
            new (code, message, ErrorType.Unauthorized);

        public static Error Forbidden(string code, string message) =>
            new (code, message, ErrorType.Forbidden);

        public bool Equals(Error? other)
        {
            return other is not null &&
                   this.Code == other.Code &&
                   this.Message == other.Message &&
                   this.Type == other.Type;
        }

        public override bool Equals(object? obj) =>
            obj is Error error && this.Equals(error);

        public override int GetHashCode() =>
            HashCode.Combine(this.Code, this.Message, this.Type);

        public override string ToString() => $"[{this.Type}] {this.Code}: {this.Message}";
    }
}
