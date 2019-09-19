using System;
using System.Collections.Generic;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents meta data for the bounce notification from Amazon SES.</summary>
    class AwsSesMail
    {
        public DateTime Timestamp { get; set; }

        public string MessageId { get; set; }

        public string Source { get; set; }

        public string SourceArn { get; set; }

        public string SourceIp { get; set; }

        public string SendingAccountId { get; set; }

        public string[] Destination { get; set; }

        public bool HeadersTruncated { get; set; }

        public AwsSesMailHeader[] Headers { get; set; }

        public AwsSesMailCommonHeaders CommonHeaders { get; set; }
    }
}
