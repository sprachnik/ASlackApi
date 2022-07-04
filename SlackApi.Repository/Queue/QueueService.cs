using Azure.Storage.Queues;
using SlackApi.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlackApi.Repository.StorageQueue
{
    public class QueueService : IQueueService
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly ConnectionStrings _connectionStrings;

        public QueueService(ApplicationSettings applicationSettings,
            ConnectionStrings connectionStrings)
        {
            _applicationSettings = applicationSettings;
            _connectionStrings = connectionStrings;
        }

        public QueueClient CreateQueueClient(string queueName)
            => new(_connectionStrings.QueueStorage, queueName);

        public async Task<bool> CreateQueue(string queueName)
        {
            try
            {
                var queueClient = CreateQueueClient(queueName);
                await queueClient.CreateIfNotExistsAsync();

                if (queueClient.Exists())
                {
                    Console.WriteLine($"Queue created: '{queueClient.Name}'");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\n\n");
                Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                return false;
            }
        }

        public async Task InsertMessage(string queueName, string message)
        {
            var queueClient = CreateQueueClient(queueName);
            await queueClient.SendMessageAsync(message);
            Console.WriteLine($"Inserted: {message} into queue {queueName}");
        }

        public async Task InsertMessage<T>(string queueName, T message)
        {
            var queueClient = CreateQueueClient(queueName);
            await queueClient.SendMessageAsync(JsonSerializer.Serialize(message));
        }
    }

    public interface IQueueService
    {
        Task<bool> CreateQueue(string queueName);
        Task InsertMessage(string queueName, string message);
        Task InsertMessage<T>(string queueName, T message);
    }
}
