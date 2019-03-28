using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class StereotypeClient : IStereotypeClient
    {
        private const string StereotypeUrl = "https://stereotype.trdlnk.cimpress.io";
        
        private readonly ILogger _logger;
                
        private readonly string _accessToken;
        
        private readonly IStereotypeClientOptions _stereotypeClientOptions;

        public StereotypeClient(string accessToken, ILogger logger = null) : this(accessToken, new StereotypeClientOptions(), logger)
        {
            
        }

        public StereotypeClient(string accessToken, IStereotypeClientOptions options, ILogger logger = null)
        {
            _accessToken = accessToken;
            _stereotypeClientOptions = options;
            _logger = logger ?? new LoggerFactory().CreateLogger("StereotypeClient");
        }

        public IStereotypeRequest Request()
        {
            return new StereotypeRequest(_accessToken, _stereotypeClientOptions, _logger);
        }
    }
}
