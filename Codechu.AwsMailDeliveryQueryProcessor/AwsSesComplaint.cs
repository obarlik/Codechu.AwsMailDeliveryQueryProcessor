using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor
{        /// <summary>Represents meta data for the complaint notification from Amazon SES.</summary>
    class AwsSesComplaint
    {
        public List<AwsSesComplainedRecipient> ComplainedRecipients { get; set; }
        public DateTime Timestamp { get; set; }
        public string MessageId { get; set; }
    }
}
