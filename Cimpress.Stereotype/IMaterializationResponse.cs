using System;
using System.Threading.Tasks;

namespace Cimpress.Stereotype
{
    public interface IMaterializationResponse
    {
        Task<byte[]> FetchBytes();
        
        Uri Uri { get; }
        
        Task<string> FetchString();

        Task<T> FetchJson<T>();
    }
}