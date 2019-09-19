using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    /// <summary>Represents the email address of recipients that bounced
    /// when sending from Amazon SES.</summary>
    class AwsSesBouncedRecipient
    {
        public string EmailAddress { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }

        public string DiagnosticCode { get; set; }
    }
}
