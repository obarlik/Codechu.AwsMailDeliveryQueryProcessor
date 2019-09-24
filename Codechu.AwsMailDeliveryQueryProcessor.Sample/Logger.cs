using System;
using System.Collections.Generic;
using System.Text;

namespace Codechu.AwsMailDeliveryQueryProcessor.Sample
{
    class Logger : IAwsLogger
    {
        public Logger()
        {
        }

        int LogCount = 0;


        void Emit(string category, int eventId, string message)
        {
            if (++LogCount == 1)
            {
                Console.WriteLine("Time                    Category    EventId  Message");
                Console.WriteLine("----------------------  ----------  -------  -------------------------");
            }

            Console.WriteLine($"{DateTime.Now,22}  {category,10}  {eventId,7}  {message}");
        }


        public void LogCritical(int eventId, string message)
        {
            Emit("Critical", eventId, message);
        }

        public void LogDebug(int eventId, string message, Exception exception)
        {
            Emit("Debug", eventId, message + $" Exception: {exception}");
        }

        public void LogError(int eventId, string message)
        {
            Emit("Error", eventId, message);
        }

        public void LogInfo(int eventId, string message)
        {
            Emit("Info", eventId, message);
        }

        public void LogTrace(int eventId, string message)
        {
            Emit("Trace", eventId, message);
        }

        public void LogWarning(int eventId, string message)
        {
            Emit("Warning", eventId, message);
        }
    }
}
