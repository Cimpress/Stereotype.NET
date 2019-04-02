using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class StereotypeClient : IStereotypeClient
    {
        private const string StereotypeUrl = "https://stereotype.trdlnk.cimpress.io";
        
        private readonly ILogger<StereotypeClient> _logger;
        
        private readonly IStereotypeClientOptions _stereotypeClientOptions;

        public StereotypeClient() : this(new StereotypeClientOptions(), null)
        {
            
        }
        
        public StereotypeClient(ILogger<StereotypeClient> logger) : this(new StereotypeClientOptions(), logger)
        {
            
        }

        public StereotypeClient(IStereotypeClientOptions options, ILogger<StereotypeClient> logger)
        {
            _stereotypeClientOptions = options;
            _logger = logger;
        }

        public IStereotypeRequest Request(string accessToken)
        {
            return new StereotypeRequest(accessToken, _stereotypeClientOptions, _logger);
        }
    }
}
