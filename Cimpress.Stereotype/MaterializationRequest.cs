using System.Collections.Generic;

namespace Cimpress.Stereotype
{
    public class MaterializationRequest
    {
        public string TemplateId { get; set; }

        public object Payload { get; set; }

        private Dictionary<string,string> AdditionalHeaders { get; }

        /// <inheritdoc />
        public MaterializationRequest()
        {
            AdditionalHeaders = new Dictionary<string, string>();
        }
    }
}