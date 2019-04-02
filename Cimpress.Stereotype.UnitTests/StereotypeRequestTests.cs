using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;
using Xunit;

namespace Cimpress.Stereotype.UnitTests
{
    public class StereotypeRequestTests
    {
        private IStereotypeRequest _stereotypeRequest;

        [Fact]
        public void MaterializeTriggersMaterialization()
        {
            var mockedLogger = new Mock<Microsoft.Extensions.Logging.ILogger<StereotypeClient>>();
            mockedLogger.Setup(a => a.Log<object>(   
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), 
                It.IsAny<EventId>(), 
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()));
            
            var mockedRestClient = new Mock<IRestClient>();
            var mockedRestResponse = new Mock<IRestResponse>();
            var bytes = new byte[] {(byte) 'T', (byte) 'E', (byte) 'S', (byte) 'T'};
            mockedRestResponse.Setup(a => a.Headers).Returns(new List<Parameter>() { new Parameter("Location", "/materialization/1122", ParameterType.HttpHeader)});
            mockedRestResponse.Setup(a => a.StatusCode).Returns(HttpStatusCode.OK);

            mockedRestClient.Setup(a => a.ExecuteTaskAsync(It.IsAny<IRestRequest>())).ReturnsAsync(mockedRestResponse.Object);
            _stereotypeRequest = new StereotypeRequest(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZS",
                new StereotypeClientOptions(), 
                mockedLogger.Object,
                mockedRestClient.Object);

            var materialization = _stereotypeRequest.SetTemplateId("test-template").Materialize<string>("something");
            Assert.Equal("https://stereotype.trdlnk.cimpress.io/materialization/1122", materialization.Result.Uri.ToString());
        }
    }
}
