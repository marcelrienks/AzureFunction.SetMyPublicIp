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

        /// <summary>
        /// Runs the SetMyPublicIp function, which creates or updates a recordset, with a specific 'A' record IP address
        /// </summary>
        /// <param name="req">the <c>HttpRequest</c> received from the post call that triggers this function</param>
        /// <param name="log">an <c>ILogger</c> used to log information, and debug logs</param>
        /// <returns></returns>
        [FunctionName("SetMyPublicIp")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            _log = log;
            _log.LogInformation("SetMyPublicIp: function triggered...");

            try
            {
                // Get request
                var request = JsonConvert.DeserializeObject<SetMyPublicIpRequest>(await new StreamReader(req.Body).ReadToEndAsync());

                // Validate and Set an 'A' recordset
                ValidateRequest(request);
                var recordSet = await SetRecordSet(request);

                return new OkObjectResult(recordSet);
            }
            catch (Exception ex)
            {
                _log.LogInformation($"SetMyPublicIp: Exception: {ex.Message}, Stacktrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Validates that all required arguments are present. HttpMethod, QueryStringParamater, and Headers
        /// </summary>
        /// <param name="request">the request from the API Gateway</param>
        /// <returns></returns>
        private static void ValidateRequest(SetMyPublicIpRequest request)
        {
            _log.LogInformation("SetMyPublicIp: Validate SetMyPublicIpRequest...");

            // Validate that request is not null
            if (request == null)
                throw new ArgumentNullException(nameof(request), "The argument cannot be null.");

            // Validate and get query param 'hostedZoneId' is present
            if (string.IsNullOrEmpty(request.ZoneName))
                throw new ArgumentException("No HostedZoneId present.", "request.HostedZoneId");

            _log.LogDebug($"HostedZoneId: {request.ZoneName}");

            // Validate and get query param 'hostedZoneId' is present
            if (string.IsNullOrEmpty(request.Domain))
                throw new ArgumentException("No Domain present.", "request.Domain");

            _log.LogDebug($"DomainName: {request.Domain}");

            // Validate and get query param 'hostedZoneId' is present
            if (string.IsNullOrEmpty(request.PublicIpAddress))
                throw new ArgumentException("No PublicIps present.", "request.PublicIps");

            _log.LogDebug($"PublicIps: {request.PublicIpAddress}");
        }

        /// <summary>
        /// Creates or updates a recordset, with a specific 'A' record IP address
        /// </summary>
        /// <param name="setMyPublicIpRequest">a <c>SetMyPublicIpRequest</c> object containing Domain, ClientId, Secret, and SubscriptionId</param>
        /// <returns>a resulting <c>RecordSet</c></returns>
        private static async Task<RecordSet> SetRecordSet(SetMyPublicIpRequest setMyPublicIpRequest)
        {
            _log.LogInformation("SetMyPublicIp: Set Record Set...");

            // Build the service credentials and DNS management client
            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(setMyPublicIpRequest.Domain, setMyPublicIpRequest.ClientId, setMyPublicIpRequest.Secret);
            var dnsClient = new DnsManagementClient(serviceCreds)
            {
                SubscriptionId = setMyPublicIpRequest.SubscriptionId
            };

            // Create record set parameters
            var recordSetParams = new RecordSet
            {
                TTL = 3600,
                ARecords = new List<ARecord>() { new ARecord(setMyPublicIpRequest.PublicIpAddress) }
            };

            // Create/update the actual record set in Azure DNS
            return await dnsClient.RecordSets.CreateOrUpdateAsync(setMyPublicIpRequest.ResourceGroupName, setMyPublicIpRequest.ZoneName, setMyPublicIpRequest.RecordSetName, RecordType.A, recordSetParams);
        }
    }
}
