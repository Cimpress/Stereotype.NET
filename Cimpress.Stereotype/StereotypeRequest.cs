using System;
using System.Linq;
using Cimpress.Stereotype.Exceptions;
using InvoiceDataStore.BL.Clients.Stereotype;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class StereotypeRequest : IStereotypeRequest
    {
        private const string StereotypeUrl = "https://stereotype.trdlnk.cimpress.io";
        
        private readonly ILogger _logger;

        private readonly RestClient _client;
        
        private readonly IStereotypeClientOptions _stereotypeClientOptions;
        
        private string _accessToken;
        
        private RestClient _restClient;
        
        private string _templateId;

        public StereotypeRequest(string accessToken, IStereotypeClientOptions options, ILogger logger)
        {
            _stereotypeClientOptions = options;
            _accessToken = accessToken;
            _logger = logger;
            _restClient = new RestClient(options.ServiceBaseUrl);
        }

        public IStereotypeRequest SetTemplateId(string templateId)
        {
            _templateId = templateId;
            return this;
        }

        public IMaterializationResponse<TI> Materialize<TI, TO>(TO payload)
        {
            var request = new RestRequest("/v1/templates/{templateId}/materializations", Method.POST);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Content-type", "application/json");
            request.AddUrlSegment("templateId", _templateId);

            var json = JsonConvert.SerializeObject(payload);
            _logger.LogInformation($">> POST /v1/templates/{_templateId}/materializations :: {json}");
            request.AddJsonBody(json);
            var response = _client.Execute(request);
            _logger.LogInformation($"<< POST /v1/templates/{_templateId}/materializations :: {json} :: {response.StatusCode}");
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
                    return new MaterializationResponse<TI>(_accessToken, new Uri(locationHeaderValue.ToString()),
                        _logger);
                default:
                    throw new StereotypeException($"Unexpected status code {response.StatusCode}", null);
            }            
        }
    }
}
