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
            IEnumerable<string> deliveredAddresses);

        void RemoveFromMailingList(
            string category,
            DateTime timestamp,
            string messageId, 
            string subject,
            IEnumerable<string> bouncedAddresses);

        void ManuallyReviewEvent(
            string category, 
            DateTime timestamp,
            string messageId,
            string subject,
            IEnumerable<string> eventAddresses);
    }
}
