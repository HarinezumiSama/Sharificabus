using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using log4net;
using Omnifactotum.Annotations;

namespace Sharificabus.HostApplication.Api
{
    internal sealed class ApiExceptionHandler : ExceptionHandler
    {
        private readonly ILog _log;

        public ApiExceptionHandler([NotNull] ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override void Handle(ExceptionHandlerContext context)
            => context.Result = new ErrorDetailsResult(_log, context);

        private sealed class ErrorDetailsResult : IHttpActionResult
        {
            private readonly ILog _log;
            private readonly ExceptionHandlerContext _context;

            public ErrorDetailsResult([NotNull] ILog log, [NotNull] ExceptionHandlerContext context)
            {
                _log = log ?? throw new ArgumentNullException(nameof(log));
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

                _log.Error("An error occurred in API.", exception);

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