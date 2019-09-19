namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents an Amazon SES delivery notification.</summary>
    class AwsSesDeliveryNotification
    {
        public string EventType { get; set; }

        public AwsSesMail Mail { get; set; }

        public AwsSesDelivery Delivery { get; set; }
    }
}
