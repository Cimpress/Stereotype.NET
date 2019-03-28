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
        
        private readonly IStereotypeClientOptions _stereotypeClientOptions;

        public StereotypeClient(ILogger logger = null) : this(new StereotypeClientOptions(), logger)
        {
            
        }

        public StereotypeClient(IStereotypeClientOptions options, ILogger logger = null)
        {
            _stereotypeClientOptions = options;
            _logger = logger ?? new LoggerFactory().CreateLogger("StereotypeClient");
        }

        public IStereotypeRequest Request(string accessToken)
        {
            return new StereotypeRequest(accessToken, _stereotypeClientOptions, _logger);
        }
    }
}
