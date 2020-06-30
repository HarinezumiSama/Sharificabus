using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.ExceptionHandling;
using log4net;
using Newtonsoft.Json;
using Omnifactotum.Annotations;
using Owin;
using Swashbuckle.Application;

namespace Sharificabus.HostApplication.Api
{
    internal static class AppBuilderExtensions
    {
        public static IAppBuilder UseNoCaching([NotNull] this IAppBuilder builder)
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

        public static IAppBuilder UseSharificabusApi(
            [NotNull] this IAppBuilder builder,
            [NotNull] ILog log,
            [NotNull] IDependencyResolver dependencyResolver)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var configuration = new HttpConfiguration
            {
                DependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver))
            };

            configuration.MapHttpAttributeRoutes();

            var formatters = configuration.Formatters;
            formatters.Clear();
            formatters.Add(new JsonMediaTypeFormatter());

            var serializerSettings = formatters.JsonFormatter.SerializerSettings;
            serializerSettings.AssignApplicationWideSettings();
            serializerSettings.Formatting = Formatting.Indented;

            configuration.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler(log));

            configuration
                .EnableSwagger(
                    config =>
                    {
                        config.RootUrl(
                            message =>
                                message.RequestUri.GetLeftPart(UriPartial.Authority)
                                    + message.GetRequestContext().VirtualPathRoot.TrimEnd('/'));

                        config.SingleApiVersion("1", Constants.ApiTitle);
                    })
                .EnableSwaggerUi(
                    config =>
                    {
                        config.DocExpansion(DocExpansion.List);
                        config.DocumentTitle(Constants.ApiTitle);
                    });

            builder
                .UseNoCaching()
                .UseLogging(log, "API", false)
                .UseWebApi(configuration);

            configuration.EnsureInitialized();

            return builder;
        }
    }
}