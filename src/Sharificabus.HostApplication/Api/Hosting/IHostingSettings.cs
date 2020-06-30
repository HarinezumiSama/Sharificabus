namespace Sharificabus.HostApplication.Api.Hosting
{
    public interface IHostingSettings
    {
        bool IsSecure
        {
            get;
        }

        int Port
        {
            get;
        }

        int SecurePort
        {
            get;
        }

        string Suffix
        {
            get;
        }

        IHostingSettings Validate();
    }
}