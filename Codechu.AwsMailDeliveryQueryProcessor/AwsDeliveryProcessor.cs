using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
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
        public string BounceQueueUrl { get; }
        public string ComplaintQueueUrl { get; }
        public string DeliveryQueueUrl { get; }
        IAwsLogger Logger { get; }
        public AmazonSQSClient SqsClient { get; }

        public AwsDeliveryProcessor(
            string awsAccessKeyId,
            string awsSecretAccessKey,
            string bounceQueueUrl,
            string complaintQueueUrl,
            string deliveryQueueUrl,
            IAwsLogger logger, 
            IAwsMailListManager mailListManager)
        {
            MailListManager = mailListManager;
            AwsAccessKeyId = awsAccessKeyId;
            AwsSecretAccessKey = awsSecretAccessKey;
            BounceQueueUrl = bounceQueueUrl;
            ComplaintQueueUrl = complaintQueueUrl;
            DeliveryQueueUrl = deliveryQueueUrl;
            Logger = logger;

            SqsClient = new AmazonSQSClient(AwsAccessKeyId, AwsSecretAccessKey);
        }


        public void Process(System.Threading.CancellationToken token)
        {
            ProcessMessages("Complaint", SqsClient, ComplaintQueueUrl, ProcessQueuedComplaint, token);
            ProcessMessages("Delivery", SqsClient, DeliveryQueueUrl, ProcessQueuedDelivery, token);
            ProcessMessages("Bounce", SqsClient, BounceQueueUrl, ProcessQueuedBounce, token);
        }


        void ProcessMessages(string category, AmazonSQSClient client, string queueUrl, Action<AmazonSQSClient, Message> processor, System.Threading.CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                foreach (var message in client.ReceiveMessage(queueUrl).Messages)
                    try
                    {
                        token.ThrowIfCancellationRequested();

                        processor(client, message);
                        client.DeleteMessage(queueUrl, message.ReceiptHandle);
                    }
                    catch(OperationCanceledException)
                    {
                        Logger.LogInfo(3, $"Cancelled processing {category} message. MessageId: {message.MessageId}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(1, $"Error while processing {category} message. MessageId: {message.MessageId}", ex);
                    }
            }
            catch (OperationCanceledException)
            {
                Logger.LogInfo(4, $"Cancelled processing {category} messages.");
            }
            catch (Exception ex)
            {
                Logger.LogError(2, $"Error while receiving {category} messages.", ex);
            }
        }


        private void ProcessQueuedDelivery(AmazonSQSClient client, Message message)
        {
            // First, convert the Amazon SNS message into a JSON object.
            var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSqsNotification>(message.Body);

            // Now access the Amazon SES bounce notification.
            var delivery = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSesDeliveryNotification>(notification.Message);

            MailListManager.ProcessMailDelivery(
                true,
                "Delivery",
                delivery.Delivery.Timestamp,
                delivery.Mail.CommonHeaders.MessageId,
                delivery.Mail.CommonHeaders.Subject,
                delivery.Delivery.Recipients);
        }



        /// <summary>Process bounces received from Amazon SES via Amazon SQS.</summary>
        /// <param name="response">The response from the Amazon SQS bounces queue 
        /// to a ReceiveMessage request. This object contains the Amazon SES  
        /// bounce notification.</param> 
        private void ProcessQueuedBounce(AmazonSQSClient client, Message message)
        {
            // First, convert the Amazon SNS message into a JSON object.
            var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSqsNotification>(message.Body);

            // Now access the Amazon SES bounce notification.
            var bounce = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSesBounceNotification>(notification.Message);

            var messageId = bounce.Mail.Headers
                            .Where(h => h.Name == "X-BizMailId")
                            .Select(h => h.Value)
                            .FirstOrDefault()
                         ?? bounce.Mail.CommonHeaders.MessageId;

            switch (bounce.Bounce.BounceType)
            {
                case "Permanent":
                    switch (bounce.Bounce.BounceSubType)
                    {
                        case "General":
                        case "NoEmail":
                            // Remove all recipients that generated a permanent bounce 
                            // or an unknown bounce.
                            MailListManager.RemoveFromMailingList(
                                $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                                bounce.Bounce.Timestamp,
                                messageId,
                                bounce.Mail.CommonHeaders.Subject,
                                bounce.Bounce.BouncedRecipients.Select(r => r.EmailAddress));
                            break;

                        default:
                            MailListManager.ManuallyReviewEvent(
                                $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                                bounce.Bounce.Timestamp,
                                bounce.Mail.CommonHeaders.MessageId,
                                bounce.Mail.CommonHeaders.Subject,
                                bounce.Bounce.BouncedRecipients.Select(r => r.EmailAddress));
                            break;
                    }
                    break;


                default:
                    MailListManager.ManuallyReviewEvent(
                        $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                        bounce.Bounce.Timestamp,
                        bounce.Mail.CommonHeaders.MessageId,
                        bounce.Mail.CommonHeaders.Subject,
                        bounce.Bounce.BouncedRecipients.Select(r => r.EmailAddress));
                    break;
            }

            MailListManager.ProcessMailDelivery(
                false,
                $"Bounce/{bounce.Bounce.BounceType}/{bounce.Bounce.BounceSubType}",
                bounce.Bounce.Timestamp,
                bounce.Mail.CommonHeaders.MessageId,
                bounce.Mail.CommonHeaders.Subject,
                bounce.Bounce.BouncedRecipients.Select(r => r.EmailAddress));
        }

        /// <summary>Process complaints received from Amazon SES via Amazon SQS.</summary>
        /// <param name="response">The response from the Amazon SQS complaint queue 
        /// to a ReceiveMessage request. This object contains the Amazon SES 
        /// complaint notification.</param>
        private void ProcessQueuedComplaint(AmazonSQSClient client, Message message)
        {
            // First, convert the Amazon SNS message into a JSON object.
            var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSqsNotification>(message.Body);

            // Now access the Amazon SES complaint notification.
            var complaint = Newtonsoft.Json.JsonConvert.DeserializeObject<AwsSesComplaintNotification>(notification.Message);

            // Remove the email address that complained from our mailing list.
            MailListManager.RemoveFromMailingList(
                "Complaint",             
                complaint.Complaint.Timestamp,
                complaint.Mail.CommonHeaders.MessageId,
                complaint.Mail.CommonHeaders.Subject,
                complaint.Complaint.ComplainedRecipients.Select(r => r.EmailAddress));

            MailListManager.ProcessMailDelivery(
                false,
                "Complaint",
                complaint.Complaint.Timestamp,
                complaint.Mail.CommonHeaders.MessageId,
                complaint.Mail.CommonHeaders.Subject,
                complaint.Complaint.ComplainedRecipients.Select(r => r.EmailAddress));
        }

    }
}
