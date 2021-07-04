using Microsoft.Extensions.Logging;

namespace AzureFunction.SetMyPublicIp.Models
{
    public class SetMyPublicIpRequest
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string SubscriptionId { get; set; }
        public string PublicIpAddress { get; set; }
        public string ResourceGroupName { get; set; }
        public string ZoneName { get; set; }
        public string RecordSetName { get; set; }
    }
}
