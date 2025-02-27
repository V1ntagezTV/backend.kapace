using backend.kapace.BLL.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace backend.kapace.Middlewares;

public class ServiceExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ServiceException serviceException)
        {
            return false;
        }
        
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/json";
        
        var result = JsonConvert.SerializeObject(new
        {
            Message = serviceException.Message,
            ErrorCode = serviceException.ErrorCode.ToString(),
            ErrorDetails = serviceException.ErrorDetails,
            InnerException = serviceException.InnerException
        });

        await httpContext.Response.WriteAsync(result, cancellationToken);
        return true;
    }
}