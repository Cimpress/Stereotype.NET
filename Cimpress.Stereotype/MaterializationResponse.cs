using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cimpress.Stereotype.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class MaterializationResponse : IMaterializationResponse
    {
        private readonly string _accessToken;
        private readonly byte[] _rawResponse;
        private readonly ILogger<StereotypeClient> _logger;
        private readonly IRestClient _restClient;


        public MaterializationResponse(string accessToken, Uri uri, ILogger<StereotypeClient> logger) : this(accessToken, uri, null, logger,
            new RestClient())
        {
            
        }

        public MaterializationResponse(string accessToken, Uri uri, byte[] rawResponse, ILogger<StereotypeClient> logger, IRestClient restClient)
        {
            Uri = uri;
            _rawResponse = rawResponse;
            _accessToken = accessToken;
            _logger = logger;
            _restClient = restClient;
        }

        public async Task<byte[]> FetchBytes()
        {
            if (_rawResponse != null)
            {
                return _rawResponse;
            }
            
            var request = new RestRequest(Uri, Method.GET);
            request.JsonSerializer = new JsonSerializer();
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            _logger?.LogDebug($">> GET {Uri}");
            var response = await _restClient.ExecuteTaskAsync(request);
            _logger?.LogDebug($"<< GET {Uri} :: {response.StatusCode}");
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

        public async Task<string> FetchString()
        {
            var bytes = await FetchBytes();
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<T> FetchJson<T>()
        {
            var bytes = await FetchBytes();
            var bytesAsString = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(bytesAsString);
        }
       
    }

}