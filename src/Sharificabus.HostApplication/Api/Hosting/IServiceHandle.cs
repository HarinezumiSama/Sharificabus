namespace Sharificabus.HostApplication.Api.Hosting
{
    public interface IServiceHandle
    {
        string Description
        {
            get;
        }

        IHostingSettingsProvider HostingSettingsProvider
        {
            get;
        }

        string ServiceSuffix
        {
            get;
        }

        string ServiceRegistrationUrl
        {
            get;
        }

        string ServiceUrl
        {
            get;
        }
    }
}