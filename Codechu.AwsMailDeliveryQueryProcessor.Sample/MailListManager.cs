using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codechu.AwsMailDeliveryQueryProcessor.Sample
{
    class MailListManager : IAwsMailListManager
    {
        public void Emit(string description, string category, DateTime timestamp, string messageId, string subject, string emailAddress, string details = "")
        {
            Console.WriteLine(
                $"{description} " +
                $"{{\r\n" +
                $"  Category  : {category}\r\n" +
                $"  Time      : {timestamp}\r\n" +
                $"  MessageId : {messageId}\r\n" +
                $"  Subject   : {subject}\r\n" +
                $"  Address   : {emailAddress}\r\n" +
                $"  Details   : {details}" +
                 "}");
        }

        public void ProcessMailHardBounce(string category, DateTime timestamp, string messageId, string subject, string email, string details)
            => Emit("Hard bounce event to be processed.", category, timestamp, messageId, subject, email, details);

        public void ProcessMailSoftBounce(string category, DateTime timestamp, string messageId, string subject, string email, string details)
            => Emit("Soft bounce event to be processed.", category, timestamp, messageId, subject, email, details);

        public void ProcessMailComplaint(string category, DateTime timestamp, string messageId, string subject, string complainedAddress)
            => Emit("Complaint event to be processed.", category, timestamp, messageId, subject, complainedAddress);

        public void ProcessMailDelivery(string category, DateTime timestamp, string messageId, string subject, string deliveredAddress)
            => Emit("Delivery event to be processed.", category, timestamp, messageId, subject, deliveredAddress);

        public void ProcessMailReview(string category, DateTime timestamp, string messageId, string subject, string eventAddress, string details)
            => Emit("Event to be reviewed.", category, timestamp, messageId, subject, eventAddress, details);

        public void ProcessMailSuppression(string category, DateTime timestamp, string messageId, string subject, string suppressedAddress)
            => Emit("Suppression event to be reviewed.", category, timestamp, messageId, subject, suppressedAddress);
    }
}
