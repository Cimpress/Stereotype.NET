using System.IO;
using Newtonsoft.Json;
using RestSharp.Serializers;

namespace Cimpress.Stereotype
{
	public class JsonSerializer : ISerializer
	{
	    private readonly Newtonsoft.Json.JsonSerializer _serializer;
        
		public JsonSerializer() {
			ContentType = "application/json";
            _serializer = new Newtonsoft.Json.JsonSerializer {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include
            };
		}       
		
		public string Serialize(object obj) {
			using (var stringWriter = new StringWriter()) {
				using (var jsonTextWriter = new JsonTextWriter(stringWriter)) {
					jsonTextWriter.Formatting = Formatting.Indented;
					jsonTextWriter.QuoteChar = '"';

					_serializer.Serialize(jsonTextWriter, obj);

					var result = stringWriter.ToString();
					return result;
				}
			}
		}
		public string ContentType { get; set; }
	}
}