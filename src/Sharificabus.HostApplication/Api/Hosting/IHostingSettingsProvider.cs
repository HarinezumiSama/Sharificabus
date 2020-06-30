namespace Sharificabus.HostApplication.Api.Hosting
{
    public interface IHostingSettingsProvider
    {
        string RootUrl
        {
            get;
        }

        string RootRegistrationUrl
        {
            get;
        }

        string GetServiceUrl(string serviceSuffix);

        string GetServiceRegistrationUrl(string serviceSuffix);
    }
}