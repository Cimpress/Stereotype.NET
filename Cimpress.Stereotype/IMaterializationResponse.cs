using System;
using System.Threading.Tasks;

namespace Cimpress.Stereotype
{
    public interface IMaterializationResponse<T>
    {
        Task<byte[]> Fetch();

        Uri Uri { get; }        
    }
}