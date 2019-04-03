using System.Threading.Tasks;

namespace Cimpress.Stereotype
{
    public interface IStereotypeRequest
    {
        IStereotypeRequest SetTemplateId(string templateId);

        IStereotypeRequest SetExpectation(string contentType, decimal probability = 1m);

        Task<IMaterializationResponse> Materialize<TO>(TO payload);
    }
}