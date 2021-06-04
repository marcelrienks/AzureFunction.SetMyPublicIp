using AzureFunction.SetMyPublicIp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunction.SetMyPublicIp
{
    public static class SetMyPublicIp
    {
        private static ILogger _log;

        [FunctionName("SetMyPublicIp")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            _log = log;
            _log.LogInformation("SetMyPublicIp post triggered.");

            var request = JsonConvert.DeserializeObject<SetMyPublicIpRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            ValidateRequest(request);

            await SetRecordSet("domain", "clientId", "secret", "subscriptionId", "ipAddress");

            return new OkObjectResult("");
        }

        /// <summary>
        /// Validates that all required arguments are present. HttpMethod, QueryStringParamater, and Headers
        /// </summary>
        /// <param name="request">the request from the API Gateway</param>
        /// <returns></returns>
        private static void ValidateRequest(SetMyPublicIpRequest request)
        {
            _log.LogInformation("Validate Request...");

            // Validate that request is not null
            if (request == null)
                throw new ArgumentNullException(nameof(request), "The argument cannot be null.");

            // Validate and get query param 'hostedZoneId' is present
            if (string.IsNullOrEmpty(request.HostedZoneId))
                throw new ArgumentException("No HostedZoneId present.", "request.HostedZoneId");

            _log.LogDebug($"HostedZoneId: {request.HostedZoneId}");

            // Validate and get query param 'hostedZoneId' is present
            if (string.IsNullOrEmpty(request.DomainName))
                throw new ArgumentException("No DomainName present.", "request.DomainName");

            _log.LogDebug($"DomainName: {request.DomainName}");

            // Validate and get query param 'hostedZoneId' is present
            if (string.IsNullOrEmpty(request.PublicIps))
                throw new ArgumentException("No PublicIps present.", "request.PublicIps");

            _log.LogDebug($"PublicIps: {request.PublicIps}");
        }

        private static async Task SetRecordSet(string domain, string clientId, string secret, string subscriptionId, string ipAddress)
        {
            // Build the service credentials and DNS management client
            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(domain, clientId, secret);
            var dnsClient = new DnsManagementClient(serviceCreds);
            dnsClient.SubscriptionId = subscriptionId;

            // Create record set parameters
            var recordSetParams = new RecordSet
            {
                TTL = 3600,
                ARecords = new List<ARecord>() { new ARecord(ipAddress) }
            };

            // Create/update the actual record set in Azure DNS
            var recordSet = await dnsClient.RecordSets.CreateOrUpdateAsync(resourceGroupName, zoneName, recordSetName, RecordType.A, recordSetParams);
        }
    }
}
