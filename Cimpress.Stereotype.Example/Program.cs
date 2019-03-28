using System;
using Cimpress.Stereotype;

namespace Example
{
    public class Data
    {
        public string value1 { get; set; }
        public string value2 { get; set; }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Provide access token as argument");
                return;
            }
            Console.WriteLine("Trying to request a materialization");

            var stereotypeClient = new StereotypeClient();
            var data = new Data()
            {
                value1 = "test-value-1",
                value2 = "test-value-2"
            };
            var response = stereotypeClient.Request(args[0]).SetTemplateId("demo.html").Materialize(data).Result.FetchString();
            
            Console.WriteLine("Response");
            Console.WriteLine();
            Console.WriteLine(response.Result);
        }
    }
}