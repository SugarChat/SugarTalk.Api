using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Api.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var statusCode = context.Exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Result = new OkObjectResult(new SugarTalkResponse()
        {
            Code = statusCode,
            Msg = context.Exception.Message
        });

        context.ExceptionHandled = true;
    }
}