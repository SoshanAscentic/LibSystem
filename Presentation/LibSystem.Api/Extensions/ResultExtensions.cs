using LibSystem.Api.Common;
using LibSystem.Application.Common.Enums;
using LibSystem.Application.Common.Models;

namespace LibSystem.Api.Extensions
{
    public static class ResultExtensions
    {

        public static IResult ToHttpResult<T>(this Result<T> result, string? successMessage = null)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(new ApiResponse<T>
                {
                    Success = true,
                    Data = result.Value,
                    Message = successMessage ?? "Operation completed successfully"
                });
            }

            return result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(CreateErrorResponse<T>(result.Error)),
                ErrorType.Validation => Results.BadRequest(CreateErrorResponse<T>(result.Error)),
                ErrorType.Conflict => Results.Conflict(CreateErrorResponse<T>(result.Error)),
                ErrorType.Unauthorized => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                ),
                ErrorType.Forbidden => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden"
                ),
                _ => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error"
                )
            };
        }

        public static IResult ToHttpResult(this Result result, string? successMessage = null)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(new ApiResponse
                {
                    Success = true,
                    Message = successMessage ?? "Operation completed successfully"
                });
            }

            return result.Error.Type switch
            {
                ErrorType.NotFound => Results.NotFound(CreateErrorResponse(result.Error)),
                ErrorType.Validation => Results.BadRequest(CreateErrorResponse(result.Error)),
                ErrorType.Conflict => Results.Conflict(CreateErrorResponse(result.Error)),
                ErrorType.Unauthorized => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                ),
                ErrorType.Forbidden => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden"
                ),
                _ => Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error"
                )
            };
        }

        public static IResult ToCreatedResult<T>(this Result<T> result, string location, string? successMessage = null)
        {
            if (result.IsSuccess)
            {
                return Results.Created(location, new ApiResponse<T>
                {
                    Success = true,
                    Data = result.Value,
                    Message = successMessage ?? "Resource created successfully"
                });
            }

            return result.ToHttpResult();
        }


        public static IResult ToNoContentResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            return result.ToHttpResult();
        }

        private static ApiResponse<T> CreateErrorResponse<T>(Error error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = error.Code,
                    Message = error.Message,
                    Type = error.Type.ToString()
                }
            };
        }

        private static ApiResponse CreateErrorResponse(Error error)
        {
            return new ApiResponse
            {
                Success = false,
                Error = new ApiError
                {
                    Code = error.Code,
                    Message = error.Message,
                    Type = error.Type.ToString()
                }
            };
        }
    }


    public static class ErrorResponseExtensions
    {

        public static ApiResponse<T> WithValidationErrors<T>(this ApiResponse<T> response, Dictionary<string, string[]> validationErrors)
        {
            if (response.Error != null)
            {
                response.Error.ValidationErrors = validationErrors;
            }
            return response;
        }

        public static ApiResponse<T> WithDetails<T>(this ApiResponse<T> response, Dictionary<string, object> details)
        {
            if (response.Error != null)
            {
                response.Error.Details = details;
            }
            return response;
        }


        public static ApiResponse WithDetails(this ApiResponse response, Dictionary<string, object> details)
        {
            if (response.Error != null)
            {
                response.Error.Details = details;
            }
            return response;
        }
    }
}
