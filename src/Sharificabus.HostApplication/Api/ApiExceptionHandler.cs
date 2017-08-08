using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Sharificabus.HostApplication.Api
{
    internal sealed class ApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
            => context.Result = new ErrorDetailsResult(context);

        private sealed class ErrorDetailsResult : IHttpActionResult
        {
            private readonly ExceptionHandlerContext _context;

            public ErrorDetailsResult(ExceptionHandlerContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
                => Task.FromResult(CreateResponseMessage());

            private HttpResponseMessage CreateResponseMessage()
            {
                var exception = _context.Exception;

                if (exception is ObjectNotFoundException)
                {
                    return CreateErrorResponse(HttpStatusCode.NotFound);
                }

                Constants.Logger.Error("An error occurred in API.", exception);

                if (exception is NotImplementedException)
                {
                    return CreateErrorResponse(HttpStatusCode.NotImplemented);
                }

                if (exception is ArgumentException)
                {
                    return CreateErrorResponse(HttpStatusCode.BadRequest);
                }

                return CreateErrorResponse(HttpStatusCode.InternalServerError);
            }

            private HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode)
                => _context.Request.CreateResponse(statusCode, new HttpError(_context.Exception.Message));
        }
    }
}