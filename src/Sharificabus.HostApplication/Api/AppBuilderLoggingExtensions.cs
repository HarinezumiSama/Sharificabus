using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Owin;
using Omnifactotum.Annotations;
using Owin;

namespace Sharificabus.HostApplication.Api
{
    internal static class AppBuilderLoggingExtensions
    {
        private const string ThisTypeName = nameof(AppBuilderLoggingExtensions);

        private const string XForwardedForHeaderName = "X-Forwarded-For";
        private const string ContentTypeHeaderName = "Content-Type";

        private const string OwinRequestIdKey = "owin.RequestId";

        private static readonly string[] EmptyXForwardedForAddresses = new string[0];

        private static readonly HashSet<string> LoggableMediaTypes = new HashSet<string>(
            new[]
            {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml",
                "text/plain"
            },
            StringComparer.Ordinal);

        private static readonly KeyValuePair<string, bool>[] LoggableHeaderNames =
        {
            KeyValuePair.Create(ContentTypeHeaderName, true),
            KeyValuePair.Create("Content-Length", false)
        };

        public static IAppBuilder UseLogging(
            [NotNull] this IAppBuilder builder,
            [NotNull] ILog log,
            [NotNull] string componentName,
            bool captureBody)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var prefix = $@"[{componentName}]";

            return builder.Use(
                (context, next) => SafelyLogRequestAsync(context, next, log, prefix, captureBody));
        }

        private static async Task SafelyLogRequestAsync(
            [NotNull] IOwinContext context,
            [NotNull] Func<Task> next,
            [NotNull] ILog log,
            [NotNull] string prefix,
            bool captureBody)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            try
            {
                await LogRequestAsync(context, next, log, prefix, captureBody);
            }
            catch (Exception ex)
                when (!ex.IsFatal())
            {
                log.Error($@"[{ThisTypeName}] Error has occurred.", ex);
                throw;
            }
        }

        private static async Task LogRequestAsync(
            [NotNull] IOwinContext context,
            [NotNull] Func<Task> next,
            [NotNull] ILog log,
            [NotNull] string prefix,
            bool captureBody)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (!log.IsDebugEnabled)
            {
                await next();
                return;
            }

            var request = context.Request;
            var response = context.Response;
            var requestIdMarker = context.GetRequestIdMarker();

            using (MemoryStream requestBody = captureBody ? new MemoryStream() : null,
                responseBody = captureBody ? new MemoryStream() : null)
            {
                var requestDetailsBuilder = new StringBuilder(
                    $@"{request.Protocol} {request.Method} '{request.Uri}' from '{request
                        .RemoteIpAddress}'");

                var forwardedFor = request.ParseXForwardedForAddresses();
                if (forwardedFor.Count != 0)
                {
                    var forwardedForString = forwardedFor.Join("', '");
                    requestDetailsBuilder.Append($@" ({XForwardedForHeaderName}: '{forwardedForString}')");
                }

                var requestDetails = requestDetailsBuilder.ToString();

                if (requestBody != null)
                {
                    await request.Body.CopyToAsync(requestBody);
                    requestBody.Position = 0;
                    request.Body = requestBody;
                }

                var originalResponseBody = response.Body;
                if (responseBody != null)
                {
                    response.Body = responseBody;
                }

                var requestBodyInfo = await GetBodyInfoAsync(request.Headers, requestBody);
                var formattedRequestHeaders = request.Headers.GetFormattedHeaders();

                log.Debug(
                    $@"{prefix} IN{requestIdMarker}: {requestDetailsBuilder}{formattedRequestHeaders}{
                        requestBodyInfo}");

                var stopwatch = Stopwatch.StartNew();
                await next();
                stopwatch.Stop();

                var responseBodyInfo = await GetBodyInfoAsync(response.Headers, responseBody);

                log.Debug(
                    $@"{prefix} OUT{requestIdMarker}: {requestDetails} => [Elapsed: {stopwatch.Elapsed}] {
                        nameof(response.StatusCode)} = {response.StatusCode} {(HttpStatusCode)response.StatusCode}, {
                        nameof(response.ContentLength)} = {response.ContentLength.ToUIString()}, {
                        nameof(response.ContentType)} = {response.ContentType.ToUIString()}{
                        responseBodyInfo}");

                responseBody?.WriteTo(originalResponseBody);
            }
        }

        private static IReadOnlyCollection<string> ParseXForwardedForAddresses(this IOwinRequest request)
        {
            var forwardedFor = request?.Headers?.GetCommaSeparatedValues(XForwardedForHeaderName);
            if (forwardedFor == null || forwardedFor.Count == 0)
            {
                return EmptyXForwardedForAddresses;
            }

            var result = forwardedFor.ToArray();
            return result;
        }

        //// ReSharper disable once SuggestBaseTypeForParameter - Enforcing MemoryStream to ensure that the original OWIN stream is NOT used
        private static async Task<string> GetBodyInfoAsync(IHeaderDictionary headers, MemoryStream bodyStream)
        {
            if (bodyStream == null)
            {
                return string.Empty;
            }

            var size = bodyStream.Length;

            var resultBuilder = new StringBuilder();
            resultBuilder.Append($" :: Body size = {size}");

            var contentType = headers[ContentTypeHeaderName];
            if (size == 0 || string.IsNullOrWhiteSpace(contentType))
            {
                return resultBuilder.ToString();
            }

            var parsedContentType = new ContentType(contentType);
            if (!LoggableMediaTypes.Contains(parsedContentType.MediaType))
            {
                return resultBuilder.ToString();
            }

            var encoding = string.IsNullOrWhiteSpace(parsedContentType.CharSet)
                ? Encoding.Default
                : Encoding.GetEncoding(parsedContentType.CharSet);

            bodyStream.Position = 0;
            var reader = new StreamReader(bodyStream, encoding);
            var body = await reader.ReadToEndAsync();
            resultBuilder.Append($" :: Body:{Environment.NewLine}{body}");
            bodyStream.Position = 0;

            return resultBuilder.ToString();
        }

        private static string GetFormattedHeaders([NotNull] this IHeaderDictionary headers)
        {
            var formattedHeaders = LoggableHeaderNames
                .Select(
                    pair =>
                    {
                        var value = headers[pair.Key];
                        var formattedValue = value != null && pair.Value ? value.ToUIString() : value;
                        return KeyValuePair.Create(pair.Key, formattedValue);
                    })
                .Where(pair => pair.Value != null)
                .Select(pair => $@"{pair.Key} = {pair.Value}")
                .Join(", ");

            return string.IsNullOrWhiteSpace(formattedHeaders) ? null : $@" :: {formattedHeaders}";
        }

        private static string GetRequestIdMarker([NotNull] this IOwinContext context)
        {
            var requestId = context.Get<string>(OwinRequestIdKey);
            return string.IsNullOrWhiteSpace(requestId) ? null : $@" [{requestId}]";
        }
    }
}