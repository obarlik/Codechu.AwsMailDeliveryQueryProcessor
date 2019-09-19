using System;
using System.Collections.Generic;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents meta data for the delivery notification from Amazon SES.</summary>
    class AwsSesDelivery
    {
        public DateTime Timestamp { get; set; }
        
        public int ProcessingTimeMillis { get; set; }

        public string[] Recipients { get; set; }

        public string SmtpResponse { get; set; }

        public string ReportingMTA { get; set; }
    }

}