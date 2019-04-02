using System;
using System.Linq;
using System.Threading.Tasks;
using Cimpress.Stereotype.Exceptions;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class StereotypeRequest : IStereotypeRequest
    {
        private const string StereotypeUrl = "https://stereotype.trdlnk.cimpress.io";
        
        private readonly ILogger<StereotypeClient> _logger;
        
        private readonly IStereotypeClientOptions _stereotypeClientOptions;
        
        private readonly string _accessToken;
        
        private readonly IRestClient _restClient;
        
        private string _templateId;
        
        private string _acceptHeader;

        public StereotypeRequest(string accessToken, IStereotypeClientOptions options, ILogger<StereotypeClient> logger, IRestClient restClient)
        {
            _stereotypeClientOptions = options;
            _accessToken = accessToken;
            _logger = logger;
            _restClient = restClient;
        }
        
        public StereotypeRequest(string accessToken, IStereotypeClientOptions options, ILogger<StereotypeClient> logger) : this(accessToken, options, logger,  new RestClient(options.ServiceBaseUrl))
        {
           
        }

        public IStereotypeRequest SetTemplateId(string templateId)
        {
            _templateId = templateId;
            return this;
        }
        
        public IStereotypeRequest SetAcceptHeader(string acceptHeader)
        {
            _acceptHeader = acceptHeader;
            return this;
        }

        public async Task<IMaterializationResponse> Materialize<TO>(TO payload)
        {
            var request = new RestRequest("/v1/templates/{templateId}/materializations", Method.POST);
            request.JsonSerializer = new JsonSerializer();
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Content-type", "application/json");
            if (!string.IsNullOrEmpty(_acceptHeader))
            {
                request.AddHeader("Accept", _acceptHeader);
            }
            request.AddUrlSegment("templateId", _templateId);
            _logger?.LogInformation($">> POST /v1/templates/{_templateId}/materializations");
            
            request.AddJsonBody(payload);
            var response = await _restClient.ExecuteTaskAsync(request);
            _logger?.LogInformation($"<< POST /v1/templates/{_templateId}/materializations :: {response.StatusCode}");
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.Accepted:
                    var locationHeaderValue = response
                        .Headers
                        .Where(x => x.Name == "Location")
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    if (locationHeaderValue == null)
                    {
                        throw new StereotypeException($"Materialization without location", null);
                    }
                    
                    return new MaterializationResponse(
                        _accessToken, 
                        new Uri(_stereotypeClientOptions.ServiceBaseUrl + locationHeaderValue.ToString()),
                        (response.StatusCode == System.Net.HttpStatusCode.Created)
                            ? response.RawBytes
                            : null,
                        _logger,
                        _restClient);
                case System.Net.HttpStatusCode.NotFound:
                    return null;
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new AuthenticationException("Incorrect authentication");
                case System.Net.HttpStatusCode.Forbidden:
                    throw new AuthorizationException("Insufficient permission level to access materialization");
                default:
                    throw new StereotypeException($"Unexpected status code {response.StatusCode}", null);
            }            
        }
    }
}
