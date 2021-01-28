using System;
using Cimpress.Stereotype;
using Microsoft.Extensions.Logging;

namespace Example
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Provide the following arguments: access token, template ID, payload");
                return;
            }
            Console.WriteLine("Trying to request a materialization");

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var stereotypeClient = new StereotypeClient(new StereotypeClientOptions() { ServiceBaseUrl = "https://stereotype.staging.trdlnk.cimpress.io" }, loggerFactory.CreateLogger<StereotypeClient>());

            var response = stereotypeClient
                .Request(args[0])
                .SetTemplateId(args[1])
                .SetResponseMode(ResponseMode.AsynchronousPoll)
                .Materialize(Newtonsoft.Json.JsonConvert.DeserializeObject(args[2]));
            
            Console.WriteLine("Response");
            Console.WriteLine($"Location header: { response.Result.Uri?.ToString() ?? "not present" }");
            Console.WriteLine($"Body: {System.Text.Encoding.UTF8.GetString(response.Result.FetchBytes().Result)}");
        }
    }
}