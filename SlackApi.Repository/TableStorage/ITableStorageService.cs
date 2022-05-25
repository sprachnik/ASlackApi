using Azure.Data.Tables;
using System.Linq.Expressions;

namespace SlackApi.Repository.TableStorage
{
    /// <summary>
    /// Defines a service that manages the uploading of key-values to table storage
    /// </summary>
    public interface ITableStorageService
    {
        /// <summary>
        /// Get a single table entity.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="tableName">The table to use.</param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey)
            where T : class, ITableEntity, new();

        /// <summary>
        /// Get all table entities.
        /// </summary>
        /// <typeparam name="T">The type of the table entity.</typeparam>
        /// <param name="tableName">The table to use.</param>
        /// <returns></returns>
        Task<List<T>> GetAllAsync<T>(string tableName, int depth = 0)
            where T : class, ITableEntity, new();

        /// <summary>
        /// Gets all by partiation key and table name asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns></returns>
        Task<List<T>> GetAllByPartitionKeyAsync<T>(string tableName, string partitionKey, int depth = 0)
            where T : class, ITableEntity, new();

        /// <summary>
        /// Insert or replace table entity to the specified table.
        /// </summary>
        /// <param name="tableEntity">The table entity to insert.</param>
        /// <param name="tableName">The table to use.</param>
        Task<Azure.Response> UpsertAsync<T>(T tableEntity, string tableName)
            where T : ITableEntity;

        /// <summary>
        /// Insert or replace table entities in bulk to the specified table.
        /// </summary>
        /// <param name="tableEntities">The table entities to insert.</param>
        /// <param name="tableName">The table to use.</param>
        Task<List<T>> UpsertCollectionAsync<T>(List<T> tableEntities, string tableName, int depth = 0)
            where T : ITableEntity;

        /// <summary>
        /// Gets all by property query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="entityQuery"></param>
        /// <returns></returns>
        Task<List<T>> GetAllByQuery<T>(string tableName,
            Expression<Func<T, bool>> entityQuery, int depth = 0) where T : class, ITableEntity, new();

        /// <summary>
        /// Deletes a single entity.
        /// </summary>
        /// <param name="tableEntity"></param>
        /// <param name="tableName"></param>
        /// <param name="isExceptionCheck">Mark as false to escape and throw an exception if one occurs, instead of retrying.</param>
        /// <returns></returns>
        Task<Azure.Response> Delete<T>(ITableEntity tableEntity,
           string tableName, bool isExceptionCheck = true, int depth = 0) where T : class, ITableEntity, new();

        Task<List<Azure.Response>> DeleteByPartitionKey(string tableName, string partitionKey);
    }
}
