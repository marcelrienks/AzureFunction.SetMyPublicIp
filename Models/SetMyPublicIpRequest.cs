namespace AzureFunction.SetMyPublicIp.Models
{
    public class SetMyPublicIpRequest
    {
        public string HostedZoneId { get; set; }
        public string DomainName { get; set; }
        public string PublicIps { get; set; }
    }
}
