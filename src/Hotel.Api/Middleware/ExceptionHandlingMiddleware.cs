using System.Net;
using Hotel.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain error");

            var (status, title) = ex switch
            {
                NotFoundException => ((int)HttpStatusCode.NotFound, "Not Found"),
                ConflictException => ((int)HttpStatusCode.Conflict, "Conflict"),
                ValidationException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
                _ => ((int)HttpStatusCode.BadRequest, "Bad Request")
            };
            
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex.Message
            };

            await ctx.Response.WriteAsJsonAsync(problem);
        }
    }
}