using MedicareApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;

namespace MedicareApi.Middleware
{
    /// <summary>
    /// Middleware to handle JWT authentication errors with standardized responses.
    /// </summary>
    public static class JwtAuthenticationConfiguration
    {
        /// <summary>
        /// Configure JWT authentication with custom error responses.
        /// </summary>
        /// <param name="options">JWT Bearer options</param>
        public static void ConfigureJwtEvents(JwtBearerOptions options)
        {
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    // Skip default challenge to return custom response
                    context.HandleResponse();

                    var errorResponse = new ApiErrorResponse
                    {
                        ErrorCode = ErrorCodes.UNAUTHORIZED_ACCESS,
                        Message = "Authentication required",
                        Details = "Please provide a valid authentication token"
                    };

                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                },

                OnAuthenticationFailed = async context =>
                {
                    var errorResponse = new ApiErrorResponse();

                    // Check the type of authentication failure
                    if (context.Exception.Message.Contains("expired", StringComparison.OrdinalIgnoreCase))
                    {
                        errorResponse.ErrorCode = ErrorCodes.TOKEN_EXPIRED;
                        errorResponse.Message = "Authentication token has expired";
                        errorResponse.Details = "Please log in again to obtain a new token";
                    }
                    else if (context.Exception.Message.Contains("signature", StringComparison.OrdinalIgnoreCase))
                    {
                        errorResponse.ErrorCode = ErrorCodes.TOKEN_INVALID;
                        errorResponse.Message = "Authentication token is invalid";
                        errorResponse.Details = "The provided token is malformed or has been tampered with";
                    }
                    else
                    {
                        errorResponse.ErrorCode = ErrorCodes.TOKEN_INVALID;
                        errorResponse.Message = "Authentication token is invalid";
                        errorResponse.Details = "Please provide a valid authentication token";
                    }

                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                },

                OnForbidden = async context =>
                {
                    var errorResponse = new ApiErrorResponse
                    {
                        ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS,
                        Message = "Access forbidden",
                        Details = "You do not have sufficient permissions to access this resource"
                    };

                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
            };
        }
    }
}