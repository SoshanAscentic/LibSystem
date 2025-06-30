using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Common.Models
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Success result cannot have an error");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("Failure result must have an error");

            IsSuccess = isSuccess;
            Error = error;
        }

        protected Result(bool isSuccess, Error[] errors)
        {
            if (isSuccess && errors.Any(e => e != Error.None))
                throw new InvalidOperationException("Success result cannot have errors");

            if (!isSuccess && !errors.Any())
                throw new InvalidOperationException("Failure result must have at least one error");

            IsSuccess = isSuccess;
            Errors = errors;
            Error = errors.FirstOrDefault() ?? Error.None;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; } = Error.None;
        public Error[] Errors { get; } = Array.Empty<Error>();

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);

        public static Result Failure(Error[] errors) => new(false, errors);

        public static Result Failure(string code, string message) =>
            new(false, Error.Failure(code, message));

        // Validation-specific methods
        public static Result ValidationFailure(string code, string message) =>
            new(false, Error.Validation(code, message));

        public static Result NotFound(string code, string message) =>
            new(false, Error.NotFound(code, message));

        public static Result Conflict(string code, string message) =>
            new(false, Error.Conflict(code, message));

        public static Result Unauthorized(string code, string message) =>
            new(false, Error.Unauthorized(code, message));

        public static Result Forbidden(string code, string message) =>
            new(false, Error.Forbidden(code, message));

        public static implicit operator Result(Error error) => Failure(error);

        // Method to get first error message (for backward compatibility)
        public string GetErrorMessage() => Error.Message;

        // Method to get all error messages
        public string[] GetErrorMessages() => Errors.Select(e => e.Message).ToArray();
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        protected Result(T? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        protected Result(T? value, bool isSuccess, Error[] errors)
            : base(isSuccess, errors)
        {
            _value = value;
        }

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result");

        public static Result<T> Success(T value) => new(value, true, Error.None);

        public static Result<T> Failure(Error error) => new(default, false, error);

        public static Result<T> Failure(Error[] errors) => new(default, false, errors);

        public static Result<T> Failure(string code, string message) =>
            new(default, false, Error.Failure(code, message));

        public static Result<T> ValidationFailure(string code, string message) =>
            new(default, false, Error.Validation(code, message));

        public static Result<T> NotFound(string code, string message) =>
            new(default, false, Error.NotFound(code, message));

        public static Result<T> Conflict(string code, string message) =>
            new(default, false, Error.Conflict(code, message));

        public static Result<T> Unauthorized(string code, string message) =>
            new(default, false, Error.Unauthorized(code, message));

        public static Result<T> Forbidden(string code, string message) =>
            new(default, false, Error.Forbidden(code, message));

        public static implicit operator Result<T>(T value) =>
            value is not null ? Success(value) : Failure(Error.NullValue);

        public static implicit operator Result<T>(Error error) => Failure(error);

        // Extension methods for better usability
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return IsSuccess ? Result<TNew>.Success(mapper(Value)) : Result<TNew>.Failure(Error);
        }

        public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
        {
            return IsSuccess ? Result<TNew>.Success(await mapper(Value)) : Result<TNew>.Failure(Error);
        }

        public Result<T> Tap(Action<T> action)
        {
            if (IsSuccess)
                action(Value);
            return this;
        }

        public async Task<Result<T>> TapAsync(Func<T, Task> action)
        {
            if (IsSuccess)
                await action(Value);
            return this;
        }
    }
}

