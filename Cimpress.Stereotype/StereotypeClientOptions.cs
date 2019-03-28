namespace Cimpress.Stereotype
{
    public class StereotypeClientOptions : IStereotypeClientOptions
    {
        private string _serviceBaseUrl;
        public string ServiceBaseUrl
        {
            get => _serviceBaseUrl ?? "https://stereotype.trdlnk.cimpress.io";
            set => _serviceBaseUrl = value;
        }
    }
}