using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Codechu.AwsMailDeliveryQueryProcessor.Sample
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();

            var config = ConfigurationManager.AppSettings;
            Logger = new Logger(EventLog);

            AmazonDeliveryProcessor = new AwsDeliveryProcessor(
                originalMailIdHeaderName: config["OriginalMailIdHeaderName"],
                awsAccessKeyId: config["AWSAccessKeyId"],
                awsSecretAccessKey: config["AWSSecretAccessKey"],
                awsRegionName: config["AWSRegionName"],
                awsSqsUrl: config["AWSSqsUrl"],
                bounceQueueName: config["AWSBounceQueueName"],
                complaintQueueName: config["AWSComplaintQueueName"],
                deliveryQueueName: config["AWSDeliveryQueueName"],
                Logger,
                new MailListManager(Logger));

            CancelSource = new CancellationTokenSource();
        }

        internal Logger Logger { get; }

        AwsDeliveryProcessor AmazonDeliveryProcessor { get; }

        CancellationTokenSource CancelSource { get; }

        Task ProcessorTask { get; set; }

        protected override void OnStart(string[] args)
        {
            ProcessorTask = Task.Run(() =>
                Logger.TraceCall("Service", () =>
                {
                    while (!CancelSource.Token.IsCancellationRequested)
                    {
                        Logger.TraceCall("Process", () => AmazonDeliveryProcessor.Process(CancelSource.Token));
                        Logger.LogTrace(0, "Sleeping 10 sec...");
                        Thread.Sleep(10000);
                    }
                }, true));
        }

        protected override void OnStop()
        {
            CancelSource.Cancel();
            ProcessorTask.Wait();
        }

        internal void DoStart()
        {
            OnStart(new string[0]);
        }
    }
}
