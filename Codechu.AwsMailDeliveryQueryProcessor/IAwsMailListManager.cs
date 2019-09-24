using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    public interface IAwsMailListManager
    {
        void ProcessMailDelivery(
            string category,
            DateTime timestamp,
            string messageId,
            string subject,
            string deliveredAddress);

        void ProcessMailBounce(
            string category,
            DateTime timestamp,
            string messageId,
            string subject,
            string bouncedAddress,
            string details);

        void ProcessMailSuppression(
            string category,
            DateTime timestamp,
            string messageId,
            string subject,
            string suppressedAddress);

        void ProcessMailComplaint(
            string category,
            DateTime timestamp,
            string messageId,
            string subject,
            string complainedAddress);

        void ProcessMailReview(
            string category, 
            DateTime timestamp,
            string messageId,
            string subject,
            string eventAddress,
            string details);
    }
}
