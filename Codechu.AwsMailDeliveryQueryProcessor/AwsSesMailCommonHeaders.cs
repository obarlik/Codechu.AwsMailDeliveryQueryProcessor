using System;
using System.Collections.Generic;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    class AwsSesMailCommonHeaders
    {

        public string[] From { get; set; }

        public string[] ReplyTo { get; set; }

        public string Date { get; set; }

        public string[] To { get; set; }

        public string MessageId { get; set; }

        public string Subject { get; set; }

        public Dictionary<string, string[]> Tags { get; set; }

        public string[] Cc { get; set; }

        public string[] Bcc { get; set; }

    }
}