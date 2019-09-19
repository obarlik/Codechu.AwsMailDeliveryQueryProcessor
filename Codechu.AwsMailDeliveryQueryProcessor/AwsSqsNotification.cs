namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents the bounce or complaint notification stored in Amazon SQS.</summary>
    class AwsSqsNotification
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
