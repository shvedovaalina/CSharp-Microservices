using Grpc.Core;

namespace Gateway.Middleware;

public class ExceptionFormattingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (RpcException ex)
        {
            string message = $"""
                          Exception occured while processing request, type = {ex.GetType().Name}, message = {ex.Message}";
                          """;
            int status = 0;
            if (ex.StatusCode == StatusCode.NotFound)
            {
                status = StatusCodes.Status404NotFound;
            }
            else if (ex.StatusCode == StatusCode.InvalidArgument)
            {
                status = StatusCodes.Status400BadRequest;
            }

            context.Response.StatusCode = status;

            await context.Response.WriteAsJsonAsync(new { message });
        }
    }
}