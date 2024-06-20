﻿using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using SupplierManagement.Domain.Exceptions;

namespace SupplierManagement.Infrastructure.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = StatusCodes.Status500InternalServerError; // 500 if unexpected
        var result = string.Empty;

        switch (exception)
        {
            case DbUpdateException e when e.InnerException is MySqlException { Number: 1062 }:
                code = StatusCodes.Status400BadRequest;
                result = "Supplier email already exists";
                break;
            case HttpException httpException:
                code = httpException.StatusCode;
                result = httpException.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;
        var response = new { StatusCode = code, Message = result };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}