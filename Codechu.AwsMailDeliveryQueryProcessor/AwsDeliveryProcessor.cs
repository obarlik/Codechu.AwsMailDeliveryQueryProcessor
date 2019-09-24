using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    public class AwsDeliveryProcessor
    {
        public IAwsMailListManager MailListManager { get; }
        public string AwsAccessKeyId { get; }
        public string AwsSecretAccessKey { get; }
        public string AwsRegionName { get; }
        public string AwsSqsUrl { get; }
        public string BounceQueueName { get; }
        public string ComplaintQueueName { get; }
        public string DeliveryQueueName { get; }
        IAwsLogger Logger { get; }

        public AwsDeliveryProcessor(
            string awsAccessKeyId,
            string awsSecretAccessKey,
            string awsRegionName,
            string awsSqsUrl,
            string bounceQueueName,
            string complaintQueueName,
            string deliveryQueueName,
            IAwsLogger logger, 
            IAwsMailListManager mailListManager)
        {
            MailListManager = mailListManager;
            AwsAccessKeyId = awsAccessKeyId;
            AwsSecretAccessKey = awsSecretAccessKey;
            AwsRegionName = awsRegionName;
            AwsSqsUrl = awsSqsUrl;
            BounceQueueName = bounceQueueName;
            ComplaintQueueName = complaintQueueName;
            DeliveryQueueName = deliveryQueueName;
            Logger = logger;

            Logger.LogTrace(0, "AwsDeliveryProcessor created.");
        }



        public void Process(System.Threading.CancellationToken token)
        {
            var sqs = new AmazonSQSClient(
                AwsAccessKeyId,
                AwsSecretAccessKey,
                new AmazonSQSConfig
                {
                    ServiceURL = AwsSqsUrl
                });

            Logger.LogTrace(0, "SQS Client created.");

            using (sqs)
            {            
                Logger.TraceCall("ProcessMessages(Complaint)", () => ProcessMessages("Complaint", sqs, ComplaintQueueName, ProcessQueuedComplaint, token));
                Logger.TraceCall("ProcessMessages(Delivery)", () => ProcessMessages("Delivery", sqs, DeliveryQueueName, ProcessQueuedDelivery, token));
                Logger.TraceCall("ProcessMessages(Bounce)", () => ProcessMessages("Bounce", sqs, BounceQueueName, ProcessQueuedBounce, token));
            }

            Logger.LogTrace(0, "SQS Client released.");
        }


        void ProcessMessages(string category, AmazonSQSClient client, string queueName, Action<AmazonSQSClient, Message> processor, System.Threading.CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var queueUrl = client.GetQueueUrlAsync(queueName).GetAwaiter().GetResult().QueueUrl;

            try
            {
                foreach (var message in client.ReceiveMessageAsync(queueUrl).GetAwaiter().GetResult().Messages)
                {
                    token.ThrowIfCancellationRequested();

                    Logger.TraceCall("Processor", () => processor(client, message));
                    Logger.TraceCall("DeleteMessage", () => client.DeleteMessageAsync(queueUrl, message.ReceiptHandle).Wait());
                }
            }
            catch (AmazonServiceException ex)
            {
                Logger.LogWarning(0, $"Amazon service unavailable, returns: {ex.Message}");
            }
        }


        private void ProcessQueuedDelivery(AmazonSQSClient client, Message message)
        {
            // First, convert the Amazon SNS message into a JSON object.
            var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSqsNotification>(message.Body);

            Logger.LogTrace(0, $"Deserializing delivery object...");

            // Now access the Amazon SES bounce notification.
            var delivery = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSesDeliveryNotification>(notification.Message);

            var messageId = GetMessageId(delivery.Mail);

            Logger.TraceCall($"ProcessMailDelivery(MessageId:{messageId})", () =>
                MailListManager.ProcessMailDelivery(
                    "Delivery",
                    delivery.Delivery.Timestamp,
                    messageId,
                    delivery.Mail.CommonHeaders.Subject,
                    delivery.Delivery.Recipients));
        }


        string GetMessageId(AwsSesMail mail)
        {
            return mail.Headers
                            .Where(h => h.Name == "X-BizMailId")
                            .Select(h => h.Value)
                            .FirstOrDefault()
                ?? mail.CommonHeaders.MessageId;
        }



        /// <summary>Process bounces received from Amazon SES via Amazon SQS.</summary>
        /// <param name="response">The response from the Amazon SQS bounces queue 
        /// to a ReceiveMessage request. This object contains the Amazon SES  
        /// bounce notification.</param> 
        private void ProcessQueuedBounce(AmazonSQSClient client, Message message)
        {
            // First, convert the Amazon SNS message into a JSON object.
            var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSqsNotification>(message.Body);

            Logger.LogTrace(0, "Deserializing bounce object.");
            
            // Now access the Amazon SES bounce notification.
            var bounce = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSesBounceNotification>(notification.Message);

            var messageId = GetMessageId(bounce.Mail);

            switch (bounce.Bounce.BounceType)
            {
                case "Permanent":
                    switch (bounce.Bounce.BounceSubType)
                    {
                        case "General":
                        case "NoEmail":
                            // Remove all recipients that generated a permanent bounce 
                            // or an unknown bounce.

                            Logger.TraceCall($"RemoveFromMailingList(MessageId:{messageId})", () =>
                                MailListManager.RemoveFromMailingList(
                                    $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                                    bounce.Bounce.Timestamp,
                                    messageId,
                                    bounce.Mail.CommonHeaders.Subject,
                                    bounce.Bounce.BouncedRecipients.Select(r => $"{r.EmailAddress} >> Action: {r.Action}, Status: {r.Status}, DiagnosticCode: {r.DiagnosticCode}")));

                            break;

                        default:
                            Logger.TraceCall($"ManuallyReviewEvent(MessageId:{messageId})", () =>
                                MailListManager.ManuallyReviewEvent(
                                    $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                                    bounce.Bounce.Timestamp,
                                    messageId,
                                    bounce.Mail.CommonHeaders.Subject,
                                    bounce.Bounce.BouncedRecipients.Select(r => $"{r.EmailAddress} >> Action: {r.Action}, Status: {r.Status}, DiagnosticCode: {r.DiagnosticCode}")));

                            break;
                    }
                    break;


                default:
                    Logger.TraceCall($"ManuallyReviewEvent(MessageId:{messageId})", () =>
                        MailListManager.ManuallyReviewEvent(
                            $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                            bounce.Bounce.Timestamp,
                            messageId,
                            bounce.Mail.CommonHeaders.Subject,
                            bounce.Bounce.BouncedRecipients.Select(r => $"{r.EmailAddress} >> Action: {r.Action}, Status: {r.Status}, DiagnosticCode: {r.DiagnosticCode}")));

                    break;
            }
        }


        /// <summary>Process complaints received from Amazon SES via Amazon SQS.</summary>
        /// <param name="response">The response from the Amazon SQS complaint queue 
        /// to a ReceiveMessage request. This object contains the Amazon SES 
        /// complaint notification.</param>
        private void ProcessQueuedComplaint(AmazonSQSClient client, Message message)
        {
            // First, convert the Amazon SNS message into a JSON object.
            var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSqsNotification>(message.Body);

            Logger.LogTrace(0, $"Deserializing complaint object.");

            // Now access the Amazon SES complaint notification.
            var complaint = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSesComplaintNotification>(notification.Message);

            var messageId = GetMessageId(complaint.Mail);

            // Remove the email address that complained from our mailing list.
            Logger.TraceCall($"RemoveFromMailingList(MessageId:{messageId})", () =>
                MailListManager.RemoveFromMailingList(
                    "Complaint",
                    complaint.Complaint.Timestamp,
                    messageId,
                    complaint.Mail.CommonHeaders.Subject,
                    complaint.Complaint.ComplainedRecipients.Select(r => r.EmailAddress)));

            Logger.TraceCall($"ProcessMailDelivery(MessageId:{messageId})", () =>
                MailListManager.ProcessMailDelivery(
                    "Complaint",
                    complaint.Complaint.Timestamp,
                    messageId,
                    complaint.Mail.CommonHeaders.Subject,
                    complaint.Complaint.ComplainedRecipients.Select(r => r.EmailAddress)));
        }

    }
}
