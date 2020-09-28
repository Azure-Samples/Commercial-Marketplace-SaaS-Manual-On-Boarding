using CommandCenter.Marketplace;

namespace CommandCenter.Models
{
    public class OperationUpdateViewModel
    {
        public string OperationType { get; set; }

        public WebhookPayload Payload { get; set; }
    }
}