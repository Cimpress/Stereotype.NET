﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        private readonly IList<string> _expectations = new List<string>();
        
        private readonly IList<string> _whiteListRels = new List<string>();
        
        private readonly IList<string> _blackListRels = new List<string>();
        
        private readonly string _accessToken;

        private ResponseMode _responseMode = ResponseMode.RespondSync;
        
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

        public IStereotypeRequest SetPreferRespondMode(ResponseMode responseMode)
        {
            _responseMode = responseMode;
            return this;
        }

        public async Task<IMaterializationResponse> Materialize<TO>(TO payload)
        {
            var request = new RestRequest("/v1/templates/{templateId}/materializations", Method.POST);
            request.JsonSerializer = new JsonSerializer();
            request.AddUrlSegment("templateId", _templateId);
            request.AddJsonBody(payload);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Content-type", "application/json");

            if (_responseMode == ResponseMode.RespondAsync)
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
            
            if (_expectations.Count > 0)
            {
                request.AddHeader("Accept", string.Join(", ", _expectations.ToArray()));
            }

            _logger?.LogInformation($">> POST /v1/templates/{_templateId}/materializations");            
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
