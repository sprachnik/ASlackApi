using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SlackApi.Core.Settings;
using SlackApi.Repository.StorageQueue;

namespace SlackApi.Functions
{
    public class NotificationFunctions
    {
        private readonly ILogger _logger;
        private readonly ApplicationSettings _applicationSettings;
        private readonly IQueueService _queueService;

        public NotificationFunctions(ILoggerFactory loggerFactory,
            ApplicationSettings applicationSettings,
            IQueueService queueService)
        {
            _logger = loggerFactory.CreateLogger<NotificationFunctions>();
            _applicationSettings = applicationSettings;
            _queueService = queueService;
        }

        [Function("Function1")]
        public void Run([QueueTrigger("usernotifications", Connection = "QueueStorage")] string myQueueItem)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            _queueService.CreateQueue("test");
        }
    }
}
