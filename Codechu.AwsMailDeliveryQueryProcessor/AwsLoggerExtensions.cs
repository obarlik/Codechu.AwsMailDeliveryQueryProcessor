using System;
using System.Collections.Generic;
using System.Text;

namespace Codechu.AwsMailDeliveryQueryProcessor
{
    public static class AwsLoggerExtensions
    {
        public static void TraceCall(this IAwsLogger logger, string name, Action action, bool critical = false)
        {
            logger.LogTrace(0, $"Calling {name}...");
            try
            {
                action();
                logger.LogTrace(0, $"Returned from {name}.");
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo(0, $"Cancelled {name}.");
            }
            catch (Exception ex)
            {
                logger.LogDebug(0, $"Exception while calling {name}. Error: {ex.Message}", ex);

                if (critical)
                    logger.LogCritical(0, $"Error while calling {name}. Error: {ex.Message}");
                else
                    logger.LogError(0, $"Error while calling {name}. Error: {ex.Message}");
            }
        }
    }
}
