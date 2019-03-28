using System.Threading.Tasks;

namespace Cimpress.Stereotype
{
    public interface IStereotypeRequest
    {
        IStereotypeRequest SetTemplateId(string templateId);

        Task<IMaterializationResponse> Materialize<TO>(TO payload);
    }
}