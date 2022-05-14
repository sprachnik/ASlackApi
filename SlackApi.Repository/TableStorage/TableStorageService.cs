using Azure.Data.Tables;
using SlackApi.App.Settings;
using SlackApi.Core.Extensions;
using System.Linq.Expressions;

namespace SlackApi.Repository.TableStorage
{
    // https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/tables/Azure.Data.Tables/tests/samples
    // https://devblogs.microsoft.com/azure-sdk/announcing-the-new-azure-data-tables-libraries/
    public class TableStorageService : ITableStorageService, IDisposable
    {
        private readonly TableServiceClient _client;

        public TableStorageService(ConnectionStrings connectionStrings) : this (connectionStrings.TableStorage)
        {
            
        }

        public TableStorageService(string connectionString)
        {
            _client = new TableServiceClient(connectionString);
        }

        public async Task<Azure.Response> Delete<T>(ITableEntity tableEntity, 
            string tableName, bool isExceptionCheck = true) where T : class, ITableEntity, new()
        {
            if (tableEntity == null)
                throw new ArgumentNullException(nameof(tableEntity), "Cannot be null!");

            var table = _client.GetTableClient(tableName);

            try
            {
                return await table.DeleteEntityAsync(tableEntity.PartitionKey, tableEntity.RowKey);
            }
            catch (Exception e)
            {
                if (e.Message.Equals("Precondition Failed") && isExceptionCheck)
                {
                    tableEntity = await GetAsync<T>(tableName, tableEntity.PartitionKey, tableEntity.RowKey);
                    return await Delete<T>(tableEntity, tableName, false);
                }

                throw;
            }
        }

        public async Task<List<Azure.Response>> DeleteByPartitionKey(string tableName, string partitionKey)
        {
            var table = _client.GetTableClient(tableName);

            var partitionTableEntities = table.QueryAsync<TableEntity>(t => t.PartitionKey == partitionKey);

            var results = new List<Azure.Response>();

            await foreach (var item in partitionTableEntities)
            {
                results.Add(table.DeleteEntity(item.PartitionKey, item.RowKey));
            }

            return results;
        }

        public async Task<T?> GetAsync<T>(string tableName, string partitionKey, 
            string rowKey) where T : class, ITableEntity, new()
        {
            var table = _client.GetTableClient(tableName);

            var response = await table.CreateIfNotExistsAsync();

            if (response is not null)
                return default;

            var result = await table.GetEntityAsync<T>(partitionKey, rowKey);

            return result.Value;
        }

        public async Task<PagedTableStorageResponse<T>> GetPagedCollectionAsync<T>(string tableName,
            string partitionKey,
            PagedTableStorageRequest request) where T : class, ITableEntity, new()
        {
            var table = _client.GetTableClient(tableName);

            var tableWasCreated = await table.CreateIfNotExistsAsync();

            if (tableWasCreated is not null)
                return new PagedTableStorageResponse<T> 
                { 
                    Data = new List<T>(), 
                    ContinuationToken = string.Empty 
                };

            var pages = table.QueryAsync<T>(q => q.PartitionKey == partitionKey);
            var data = new List<T>();

            var continuationToken = string.Empty;

            await foreach (var page in pages.AsPages(request.ContinuationToken, request.Take))
            {
                data.AddRange(page.Values);
                continuationToken = page.ContinuationToken;
            }

            return new PagedTableStorageResponse<T>
            {
                Data = data,
                ContinuationToken = continuationToken
            };
        }

        public async Task<Azure.Response> UpsertAsync<T>(T tableEntity, string tableName)
            where T : ITableEntity
        {
            // Retrieve a reference to the table.
            var table = _client.GetTableClient(tableName);
            
            try
            {
                return await table.UpsertEntityAsync(tableEntity);
            }
            catch (Azure.RequestFailedException)
            {
                // Create the table if it doesn't exist.
                await table.CreateIfNotExistsAsync();
                return await table.UpsertEntityAsync(tableEntity);
            }
        }

        public async Task<List<T>> UpsertCollectionAsync<T>(List<T> tableEntities, string tableName)
            where T : ITableEntity
        {
            if (tableEntities.IsNullOrEmpty())
                return new List<T>();

            var table = _client.GetTableClient(tableName);

            try
            {
                List<TableTransactionAction> addEntitiesBatch = new();
                addEntitiesBatch.AddRange(tableEntities.Select(e => new TableTransactionAction(TableTransactionActionType.Add, e)));
                var response = await table.SubmitTransactionAsync(addEntitiesBatch).ConfigureAwait(false);

                return tableEntities;
            }
            catch (Azure.RequestFailedException)
            {
                await table.CreateIfNotExistsAsync();
                return await UpsertCollectionAsync(tableEntities, tableName);
            }
        }

        public async Task<List<T>> GetAllAsync<T>(string tableName)
            where T : class, ITableEntity, new()
        {
            var table = _client.GetTableClient(tableName);

            try
            {
                var result = table.QueryAsync<T>(q => q.PartitionKey != null);

                List<T> results = new();

                await foreach (var page in result.AsPages())
                {
                    results.AddRange(page.Values);
                }
                
                return results;
            }
            catch (Azure.RequestFailedException)
            {
                await table.CreateIfNotExistsAsync();
                return await GetAllAsync<T>(tableName);
            }
        }


        public async Task<List<T>> GetAllByPartitionKeyAsync<T>(string tableName, string partitionKey)
            where T : class, ITableEntity, new()
        {
            var table = _client.GetTableClient(tableName);

            try
            {
                var result = table.QueryAsync<T>(q => q.PartitionKey == partitionKey);

                List<T> results = new();

                await foreach (var page in result.AsPages())
                {
                    results.AddRange(page.Values);
                }

                return results;
            }
            catch (Azure.RequestFailedException)
            {
                await table.CreateIfNotExistsAsync();
                return await GetAllByPartitionKeyAsync<T>(tableName, partitionKey);
            }
        }

        public async Task<List<T>> GetAllByQuery<T>(string tableName, string query)
            where T : class, ITableEntity, new()
        {
            var table = _client.GetTableClient(tableName);

            try
            {
                var result = table.QueryAsync<T>(query);

                List<T> results = new();

                await foreach (var page in result.AsPages())
                {
                    results.AddRange(page.Values);
                }

                return results;
            }
            catch (Azure.RequestFailedException)
            {
                await table.CreateIfNotExistsAsync();
                return await GetAllByQuery<T>(tableName, query);
            }
        }

        public async Task<List<T>> GetAllByQuery<T>(string tableName,
            Expression<Func<T, bool>> entityQuery) where T : class, ITableEntity, new()
        {
            var table = _client.GetTableClient(tableName);

            try
            {
                var result = table.QueryAsync(entityQuery);

                List<T> results = new();

                await foreach (var page in result.AsPages())
                {
                    results.AddRange(page.Values);
                }

                return results;
            }
            catch (Azure.RequestFailedException)
            {
                await table.CreateIfNotExistsAsync();
                return await GetAllByQuery(tableName, entityQuery);
            }
        }

        public void Dispose()
        {
        }
    }

    public static class TableStorageFactory
    {
        public static TableStorageService Create(string connectionString) =>
            new(new ConnectionStrings()
            { TableStorage = connectionString });
    }

    public class TelemetryTableStorageService : TableStorageService
    {
        public TelemetryTableStorageService(ConnectionStrings connectionStrings) : base(connectionStrings)
        {
        }

        public TelemetryTableStorageService(string connectionString) : base(connectionString)
        {
        }
    }

    public class PagedTableStorageRequest
    {
        public string? ContinuationToken { get; set; }
        public int Take { get; set; } = 25;
    }

    public class PagedTableStorageResponse<T>
    {
        public string? ContinuationToken { get; set; }
        public List<T>? Data { get; set; }
    }

    /// <summary>
    /// Default column values used in table storage for table storage entities.
    /// </summary>
    public struct TableStorageConstants
    {
        /// <summary>
        /// The timestamp.
        /// </summary>
        public const string Timestamp = "Timestamp";

        /// <summary>
        /// The row key.
        /// </summary>
        public const string RowKey = "RowKey";

        /// <summary>
        /// The partition key.
        /// </summary>
        public const string PartitionKey = "PartitionKey";

        /// <summary>
        /// The success status for the operation logged.
        /// </summary>
        public const string Success = "Success";

        /// <summary>
        /// The event type.
        /// </summary>
        public const string EventType = "EventTypeString";
    }
}