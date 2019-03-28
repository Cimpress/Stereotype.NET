using Cimpress.Stereotype;

namespace InvoiceDataStore.BL.Clients.Stereotype
{
    public class StereotypeClientOptions : IStereotypeClientOptions
    {
        public string ServiceBaseUrl { get; set; }
        
        public string AccessToken { get; set; }
    }
}