using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; protected set; }
        public List<string> Errors { get; protected set; } = new();

        protected Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
            if (!string.IsNullOrEmpty(error))
            {
                Errors.Add(error);
            }
        }

        protected Result(bool isSuccess, List<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? new List<string>();
            Error = errors?.FirstOrDefault() ?? string.Empty;
        }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);
        public static Result Failure(List<string> errors) => new(false, errors);

        public static implicit operator Result(string error) => Failure(error);
    }

    public class Result<T> : Result
    {
        public T Value { get; private set; }

        protected Result(bool isSuccess, T value, string error) : base(isSuccess, error)
        {
            Value = value;
        }

        protected Result(bool isSuccess, T value, List<string> errors) : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, string.Empty);
        public static Result<T> Failure(string error) => new(false, default, error);
        public static Result<T> Failure(List<string> errors) => new(false, default, errors);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(string error) => Failure(error);
    }
}
}
