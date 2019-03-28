namespace Cimpress.Stereotype
{
    public interface IStereotypeRequest
    {
        IStereotypeRequest SetTemplateId(string templateId);

        IMaterializationResponse<TI> Materialize<TI, TO>(TO payload);
    }
}