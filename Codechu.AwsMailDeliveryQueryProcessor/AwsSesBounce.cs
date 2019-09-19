using System;
using System.Collections.Generic;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents meta data for the bounce notification from Amazon SES.</summary>
    class AwsSesBounce
    {
        public string BounceType { get; set; }
        public string BounceSubType { get; set; }
        public DateTime Timestamp { get; set; }
        public List<AwsSesBouncedRecipient> BouncedRecipients { get; set; }
    }
}
