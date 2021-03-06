using System;
using System.Threading.Tasks;

namespace Cimpress.Stereotype
{
    public interface IStereotypeRequest
    {
        IStereotypeRequest SetTemplateId(string templateId);

        IStereotypeRequest SetExpectation(string contentType, decimal probability = 1m);

        IStereotypeRequest SetRetentionPeriod(TimeSpan retentionDuration);

        IStereotypeRequest SetTimeout(TimeSpan timeout);
        
        IStereotypeRequest WithWhitelistedRelation(string whiteListEntry);
        
        IStereotypeRequest WithBlacklistedRelation(string blackListEntry);
        
        IStereotypeRequest SetResponseMode(ResponseMode responseMode);

        Task<IMaterializationResponse> Materialize<TO>(TO payload);
    }
}