namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents an Amazon SES bounce notification.</summary>
    class AwsSesBounceNotification
    {
        public string NotificationType { get; set; }

        public AwsSesMail Mail { get; set; }

        public AwsSesBounce Bounce { get; set; }
    }
}
