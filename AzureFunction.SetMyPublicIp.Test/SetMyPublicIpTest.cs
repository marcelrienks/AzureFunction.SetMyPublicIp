using AzureFunction.SetMyPublicIp.Models;
using Functions.Tests.Factories;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace AzureFunction.SetMyPublicIp.Test
{
    public class SetMyPublicIpTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void HttpTriggerWithBadRequestObjectShouldReturnException()
        {
            var request = TestFactory.CreateHttpRequest("bad reqeust");

            await Assert.ThrowsAsync<ArgumentException>(typeof(SetMyPublicIpRequest).ToString(), async () => { await SetMyPublicIp.Run(request, logger); });
        }

        [Fact]
        public async void HttpTriggerWithBadRequestPropertyShouldReturnException()
        {
            var request = TestFactory.CreateHttpRequest(new SetMyPublicIpRequest());

            await Assert.ThrowsAsync<ArgumentException>(async () => { await SetMyPublicIp.Run(request, logger); });
        }
    }
}
