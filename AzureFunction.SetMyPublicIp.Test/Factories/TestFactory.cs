using AzureFunction.SetMyPublicIp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Functions.Tests.Factories
{
    public class TestFactory
    {
        public static IEnumerable<object[]> Data()
        {
            return new List<object[]>
            {
                new object[] { "name", "Bill" },
                new object[] { "name", "Paul" },
                new object[] { "name", "Steve" }

            };
        }

        public static HttpRequest CreateHttpRequest(string body)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;

            var writer = new StreamWriter(request.Body);
            writer.Write(body);
            writer.Flush();

            return request;
        }

        public static HttpRequest CreateHttpRequest(SetMyPublicIpRequest setMyPublicIpRequest)
        {
            return CreateHttpRequest(JsonConvert.SerializeObject(setMyPublicIpRequest));
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}