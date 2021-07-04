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
                var setMyPublicIpRequest = JsonConvert.DeserializeObject<SetMyPublicIpRequest>(await new StreamReader(req.Body).ReadToEndAsync());

                ValidateSetMyPublicIpRequest(setMyPublicIpRequest);

                var recordSet = await SetRecordSet(setMyPublicIpRequest);

                return new OkObjectResult(recordSet);
            }
            catch (Exception ex)
            {
                _log.LogCritical($"SetMyPublicIp: Exception: {ex.Message}, Stacktrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Validates all properties of the extended request object, throwing an <c>ArgumentException</c> exception of not present, else logging the value if it is.
        /// </summary>
        public static void ValidateSetMyPublicIpRequest(SetMyPublicIpRequest setMyPublicIpRequest)
        {
            if (setMyPublicIpRequest != null)
            {
                _log.LogInformation("SetMyPublicIp: Validate SetMyPublicIpRequest...");

                var properties = typeof(SetMyPublicIpRequest).GetProperties();
                foreach (var property in properties)
                {
                    if (string.IsNullOrEmpty(property.GetValue(setMyPublicIpRequest).ToString()))
                        throw new ArgumentException("Argument cannot be null or empty", property.Name);

                    _log.LogDebug($"{property.Name}: {property.GetValue(setMyPublicIpRequest)}");
                }
            }
            else
            {
                throw new ArgumentException("Argument cannot be null or empty", typeof(SetMyPublicIpRequest).ToString());
            }
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
