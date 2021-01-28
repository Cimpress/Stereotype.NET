using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cimpress.Stereotype.Exceptions;
using Iso8601Duration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Cimpress.Stereotype
{
    public class StereotypeRequest : IStereotypeRequest
    {
        private const string StereotypeUrl = "https://stereotype.trdlnk.cimpress.io";
        
        private readonly ILogger<StereotypeClient> _logger;
        
        private readonly IStereotypeClientOptions _stereotypeClientOptions;
        
        private readonly IList<string> _expectations = new List<string>();
        
        private readonly IList<string> _whiteListRels = new List<string>();
        
        private readonly IList<string> _blackListRels = new List<string>();
        
        private readonly string _accessToken;

        private ResponseMode _responseMode = ResponseMode.AsynchronousPoll;
        
        private readonly IRestClient _restClient;
        
        private string _templateId;
        
        private TimeSpan? _retentionDuration;

        private TimeSpan _timeout = TimeSpan.FromSeconds(30);
        
        private PeriodBuilder _periodBuilder;

        public StereotypeRequest(string accessToken, IStereotypeClientOptions options, ILogger<StereotypeClient> logger, IRestClient restClient)
        {
            _stereotypeClientOptions = options;
            _accessToken = accessToken;
            _logger = logger;
            _restClient = restClient;
            _periodBuilder = new PeriodBuilder();
        }
        
        public StereotypeRequest(string accessToken, IStereotypeClientOptions options, ILogger<StereotypeClient> logger) : this(accessToken, options, logger,  new RestClient(options.ServiceBaseUrl))
        {
           
        }

        public IStereotypeRequest SetTemplateId(string templateId)
        {
            _templateId = templateId;
            return this;
        }
        
        public IStereotypeRequest SetExpectation(string contentType, decimal probability = 1m)
        {
            _expectations.Add($"{contentType};q={probability}");
            return this;
        }

        public IStereotypeRequest WithWhitelistedRelation(string whiteListEntry)
        {
            _whiteListRels.Add(whiteListEntry);
            return this;
        }

        public IStereotypeRequest WithBlacklistedRelation(string blackListEntry)
        {
            _blackListRels.Add(blackListEntry);
            return this;
        }

        public IStereotypeRequest SetResponseMode(ResponseMode responseMode)
        {
            _responseMode = responseMode;
            return this;
        }
        
        public IStereotypeRequest SetRetentionPeriod(TimeSpan retentionDuration)
        {
            _retentionDuration = retentionDuration;
            return this;
        }

        private async Task<IMaterializationResponse> GetMaterializationResponse(string location, TimeSpan timeout)
        {
            IRestResponse response = null;
            var startTime = DateTime.UtcNow;

            while (response == null || response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                if (response != null) {
                    await Task.Delay(1000);
                }

                var remainingTimeout = (timeout - (DateTime.UtcNow - startTime)).TotalMilliseconds;
                if (remainingTimeout < 0)
                {
                    throw new TimeoutException("The request to Stereotyped timed out.");
                }

                var request = new RestRequest(location, Method.GET);
                request.AddHeader("Authorization", $"Bearer {_accessToken}");

                if (_expectations.Count > 0)
                {
                    request.AddHeader("Accept", string.Join(", ", _expectations.ToArray()));
                }

                _logger?.LogInformation($">> GET {location}");            

                request.Timeout = (int) remainingTimeout;
                response = await _restClient.ExecuteTaskAsync(request);

                _logger?.LogInformation($"<< GET {location} :: {response.StatusCode}");
            }

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                    return new MaterializationResponse(
                        _accessToken, 
                        new Uri(_stereotypeClientOptions.ServiceBaseUrl + location.ToString()),
                        response.RawBytes,
                        _logger,
                        _restClient);
                case System.Net.HttpStatusCode.NotFound:
                    return null;
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new AuthenticationException("Incorrect authentication");
                case System.Net.HttpStatusCode.Forbidden:
                    throw new AuthorizationException("Insufficient permissions to perform action");
                default:
                    if (response.StatusCode == 0 && response.ErrorMessage?.IndexOf("The request timed-out.") != -1)
                    {
                        throw new TimeoutException("The request to Stereotyped timed out.");
                    }
                    _logger.LogError(response.ErrorException.ToString());
                    throw new StereotypeException($"Unexpected status code {response.StatusCode}", null);
            }
        }

        public async Task<IMaterializationResponse> Materialize<TO>(TO payload)
        {
            var request = new RestRequest("/v1/templates/{templateId}/materializations", Method.POST);
            request.JsonSerializer = new JsonSerializer();
            request.AddUrlSegment("templateId", _templateId);
            request.AddJsonBody(payload);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Content-type", "application/json");
            request.Timeout = (int) _timeout.TotalMilliseconds;

            if (_responseMode == ResponseMode.Asynchronous || _responseMode == ResponseMode.AsynchronousPoll)
            {
                request.AddHeader("prefer", "respond-async");
            }

            if (_whiteListRels.Count > 0)
            {
                request.AddHeader("x-cimpress-rel-whitelist", string.Join(",", _whiteListRels.ToArray())); 
            }
            
            if (_blackListRels.Count > 0)
            {
                request.AddHeader("x-cimpress-rel-blacklist", string.Join(",", _blackListRels.ToArray())); 
            }

            if (_retentionDuration.HasValue)
            {
                request.AddHeader("x-cimpress-retention-duration", _periodBuilder.ToString(_retentionDuration.Value));
            }
            
            if (_expectations.Count > 0)
            {
                request.AddHeader("Accept", string.Join(", ", _expectations.ToArray()));
            }

            _logger?.LogInformation($">> POST /v1/templates/{_templateId}/materializations");            

            var startTime = DateTime.UtcNow;
            var response = await _restClient.ExecuteTaskAsync(request);
            
            _logger?.LogInformation($"<< POST /v1/templates/{_templateId}/materializations :: {response.StatusCode}");

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.Accepted:
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    return null;
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new AuthenticationException("Incorrect authentication");
                case System.Net.HttpStatusCode.Forbidden:
                    throw new AuthorizationException("Insufficient permissions to perform action");
                default:
                    if (response.StatusCode == 0 && response.ErrorMessage?.IndexOf("The request timed-out.") != -1)
                    {
                        throw new TimeoutException("The request to Stereotyped timed out.");
                    }
                    throw new StereotypeException($"Unexpected status code {response.StatusCode}", null);
            }

            var locationHeaderValue = response
                .Headers
                .Where(x => x.Name == "Location")
                .Select(x => x.Value)
                .FirstOrDefault();

            if (_responseMode == ResponseMode.Synchronous || _responseMode == ResponseMode.Asynchronous)
            {
                return new MaterializationResponse(
                    _accessToken, 
                    (locationHeaderValue != null)
                        ? new Uri(_stereotypeClientOptions.ServiceBaseUrl + locationHeaderValue.ToString())
                        : null,
                    (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                        ? response.RawBytes
                        : null,
                    _logger,
                    _restClient);
            }

            if (locationHeaderValue == null)
            {
                throw new StereotypeException($"Materialization without location", null);
            }

            return await GetMaterializationResponse(locationHeaderValue.ToString(), _timeout - (DateTime.UtcNow - startTime));
        }
    }
}
