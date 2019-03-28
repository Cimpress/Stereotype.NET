using System;
using System.Threading.Tasks;
using Cimpress.Stereotype.Exceptions;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class MaterializationResponse<T> : IMaterializationResponse<T>
    {
        private readonly string _accessToken;
        private readonly ILogger _logger;
        private readonly IRestClient _restClient;

        public MaterializationResponse(string accessToken, Uri uri, ILogger logger, IRestClient restClient)
        {
            Uri = uri;
            _accessToken = accessToken;
            _logger = logger;
            _restClient = restClient;
        }
        
        public MaterializationResponse(string accessToken, Uri uri, ILogger logger) : this(accessToken, uri, logger, new RestClient())
        {
           
        }

        public async Task<byte[]> Fetch()
        {
            var request = new RestRequest(Uri, Method.GET);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Content-type", "application/json");
            _logger.LogDebug($">> GET {Uri}");
            var response = await _restClient.ExecuteTaskAsync(request);
            _logger.LogDebug($"<< GET {Uri} :: {response.StatusCode}");
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.Accepted:
                    return response.RawBytes;

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

        public Uri Uri { get; }
    }

}