namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents an Amazon SES complaint notification.</summary>
    class AwsSesComplaintNotification
    {
        public string NotificationType { get; set; }

        public AwsSesMail Mail { get; set; }

        public AwsSesComplaint Complaint { get; set; }
    }
}
