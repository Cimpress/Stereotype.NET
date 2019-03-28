using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cimpress.TagliatelleNetCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

namespace Cimpress.Stereotype.UnitTests
{
    public class MaterializationResponseTests
    {
        private IMaterializationResponse<JObject> _materializationResponse;

        [Fact]
        public void eFetchesMaterializationBytesFromUrl()
        {
            var mockedLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
            mockedLogger.Setup(a => a.Log<object>(   
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(), 
                It.IsAny<EventId>(), 
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()));
            
            var mockedRestClient = new Mock<IRestClient>();
            var mockedRestResponse = new Mock<IRestResponse>();
            var bytes = new byte[] {(byte) 'T', (byte) 'E', (byte) 'S', (byte) 'T'};
            mockedRestResponse.Setup(a => a.RawBytes).Returns(bytes);
            mockedRestResponse.Setup(a => a.StatusCode).Returns(HttpStatusCode.OK);

            mockedRestClient.Setup(a => a.ExecuteTaskAsync(It.IsAny<IRestRequest>())).ReturnsAsync(mockedRestResponse.Object);
            _materializationResponse = new MaterializationResponse<JObject>(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZS",
                new Uri("https://some.site/materialization.json"),
                mockedLogger.Object,
                mockedRestClient.Object);

            Assert.Equal<byte>(bytes, _materializationResponse.Fetch().Result);
        }
    }
}
