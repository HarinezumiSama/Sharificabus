using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Newtonsoft.Json;
using Omnifactotum.Annotations;
using Owin;
using Sharificabus.HostApplication.Api;
using Swashbuckle.Application;

namespace Sharificabus.HostApplication
{
    internal static class ApiBuilderExtensions
    {
        private const string XForwardedForHeaderName = "X-Forwarded-For";
        private static readonly string[] EmptyXForwardedForAddresses = new string[0];

        public static IAppBuilder UseNoCaching(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Use(
                async (context, next) =>
                {
                    context.Response.OnSendingHeaders(
                        state =>
                        {
                            var responseHeaders = context.Response.Headers;

                            responseHeaders.SetCommaSeparatedValues(
                                "Cache-Control",
                                "no-cache",
                                "no-store",
                                "must-revalidate");

                            responseHeaders.Set("Pragma", "no-cache");
                            responseHeaders.Set("Expires", "0");
                        },
                        null);

                    await next();
                });
        }

        public static IAppBuilder UseLogging(this IAppBuilder builder, string componentName)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrWhiteSpace(componentName))
            {
                throw new ArgumentException(
                    @"The value can be neither empty nor whitespace-only string nor null.",
                    nameof(componentName));
            }

            var prefix = $@"[{componentName}]";

            return builder.Use(
                async (context, next) =>
                {
                    if (!Constants.Logger.IsDebugEnabled)
                    {
                        await next();
                        return;
                    }

                    var request = context.Request;

                    var requestDetailsBuilder = new StringBuilder(
                        $@"{request.Protocol} {request.Method} '{request.Uri}' from '{request.RemoteIpAddress}'");

                    var forwardedFor = ParseXForwardedForAddresses(request);
                    if (forwardedFor.Count != 0)
                    {
                        var forwardedForString = forwardedFor.Join("', '");
                        requestDetailsBuilder.Append($@" ({XForwardedForHeaderName}: '{forwardedForString}')");
                    }

                    var requestDetails = requestDetailsBuilder.ToString();

                    Constants.Logger.Debug($@"{prefix} IN: {requestDetails}");

                    await next();

                    var response = context.Response;

                    Constants.Logger.Debug(
                        $@"{prefix} OUT: {requestDetails} => {nameof(response.StatusCode)} = {
                            response.StatusCode} {(HttpStatusCode)response.StatusCode}, {
                            nameof(response.ContentLength)} = {response.ContentLength.ToUIString()}, {
                            nameof(response.ContentType)} = {response.ContentType.ToUIString()}");
                });
        }

        private static IList<string> ParseXForwardedForAddresses(IOwinRequest request)
        {
            var forwardedFor = request?.Headers?.GetCommaSeparatedValues(XForwardedForHeaderName);
            if (forwardedFor == null || forwardedFor.Count == 0)
            {
                return EmptyXForwardedForAddresses;
            }

            var result = forwardedFor.ToArray();
            return result;
        }

        public static IAppBuilder UseSharificabusApi(
            [NotNull] this IAppBuilder builder,
            [NotNull] IDependencyResolver dependencyResolver)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (dependencyResolver == null)
            {
                throw new ArgumentNullException(nameof(dependencyResolver));
            }

            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();

            var formatters = configuration.Formatters;
            formatters.Clear();
            formatters.Add(new JsonMediaTypeFormatter());

            var serializerSettings = formatters.JsonFormatter.SerializerSettings;
            serializerSettings.AssignApplicationWideSettings();
            serializerSettings.Formatting = Formatting.Indented;

            configuration.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());

            configuration.DependencyResolver = dependencyResolver;

            configuration
                .EnableSwagger(
                    config =>
                    {
                        config.RootUrl(
                            message =>
                                message.RequestUri.GetLeftPart(UriPartial.Authority)
                                    + message.GetRequestContext().VirtualPathRoot.TrimEnd('/'));

                        config.SingleApiVersion("v1", "Sharificabus API");
                    })
                .EnableSwaggerUi();

            builder
                .UseNoCaching()
                .UseLogging("API")
                .UseWebApi(configuration);

            configuration.EnsureInitialized();

            return builder;
        }
    }
}