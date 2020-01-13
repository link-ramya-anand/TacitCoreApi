using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace TacitCoreDemo.Services
{
    internal class CustomErrorFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            ErrorDetail error;
           
            if (context.Exception.Message.Contains("No Menu items found"))
            {
                LoggerManager.InfoLog(context.Exception.Message);
                context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                error = new ErrorDetail()
                {
                    StatusCode = "ERR-404",
                    Message = context.Exception.Message
                };
            }
            else if (context.Exception.Message.Contains("End of Page"))
            {
                LoggerManager.InfoLog(context.Exception.Message);
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest ;
                error = new ErrorDetail()
                {
                    StatusCode = "ERR-400",
                    Message = context.Exception.Message
                };
            }
            else
            {
                LoggerManager.ErrorLog(context.Exception.Message);
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                error = new ErrorDetail()
                {
                    StatusCode = "ERR-500",
                    Message = "Internal Server Error. Please Contact Administrator."
                };
            }

            context.Result = new JsonResult(error);
                       
            return Task.CompletedTask;
        }
        
    }
}