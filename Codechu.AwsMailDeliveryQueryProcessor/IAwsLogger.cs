using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    public interface IAwsLogger
    {
        void LogError(int eventId, string message, Exception exception);

        void LogWarning(int eventId, string message);

        void LogInfo(int eventId, string message);

        void LogTrace(int eventId, string message);

        void LogDebug(int eventId, string message);

        void LogCritical(int eventId, string message);
    }
}
