using Dapper;
using Dapper.Contrib.Extensions;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace SlackApi.Repository.SQL
{
    /// <summary>
    /// https://github.com/StackExchange/Dapper/tree/master/Dapper.Contrib
    /// </summary>
    public class DapperRepository : IDapperRepository
    {
        private readonly string _connectionString;

        public DapperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public T GetById<T>(int id) where T : class
        {
            using var conn = GetDbConnection();
            return conn.Get<T>(id);
        }

        public async Task<T> GetByIdAsync<T>(int id) where T : class
        {
            using var conn = GetDbConnection();
            return await conn.GetAsync<T>(id);
        }

        public IEnumerable<T> Query<T>(string query) where T : class
        {
            using var conn = GetDbConnection();
            return conn.Query<T>(query);
        }

        public IEnumerable<T> Query<T>(string query, object queryParameters) where T : class
        {
            using var conn = GetDbConnection();
            return conn.Query<T>(query, param: queryParameters);
        }

        public IEnumerable<T> QueryStruct<T>(string query, object queryParameters) where T : struct
        {
            using var conn = GetDbConnection();
            return conn.Query<T>(query, param: queryParameters);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string query)
        {
            using var conn = GetDbConnection();
            return await conn.QueryAsync<T>(query);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string query, object queryParameters)
        {
            using var conn = GetDbConnection();
            return await conn.QueryAsync<T>(query, param: queryParameters);
        }

        public async Task<bool> UpdateAsync<T>(T dto) where T : class
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            var updated = await unitOfWork.Connection.UpdateAsync(
                dto,
                transaction: unitOfWork.Transaction);

            if (updated)
                unitOfWork.Commit();

            return updated;
        }

        public bool Update<T>(T dto) where T : class
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            var updated = unitOfWork.Connection.Update(
                dto,
                transaction: unitOfWork.Transaction);

            if (updated)
                unitOfWork.Commit();

            return updated;
        }

        public async Task<int?> InsertAsync<T>(T dto, int? itemId = null) where T : class
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            var generatedId = await unitOfWork.Connection.InsertAsync(
                dto,
                transaction: unitOfWork.Transaction);

            if (generatedId != default
                || itemId != null
                || !ObjectHasKey<T>())
                unitOfWork.Commit();

            return itemId ?? generatedId;
        }

        private static bool ObjectHasKey<T>()
        {
            return Attribute.GetCustomAttribute(typeof(T),
                       typeof(KeyAttribute)) != null ||
                   Attribute.GetCustomAttribute(typeof(T),
                       typeof(ExplicitKeyAttribute)) != null;
        }

        public async Task<bool> DeleteAsync<T>(T dto) where T : class
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            var deleted = await unitOfWork.Connection.DeleteAsync(
                dto,
                transaction: unitOfWork.Transaction);

            if (deleted)
                unitOfWork.Commit();

            return deleted;
        }

        public IEnumerable<T> ExecuteStoredProcedure<T>(string storedProcedureName,
            object queryParameters)
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            var result = unitOfWork.Connection.Query<T>(
                storedProcedureName,
                queryParameters,
                commandType: CommandType.StoredProcedure,
                transaction: unitOfWork.Transaction);

            unitOfWork.Commit();

            return result;
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName,
            object queryParameters)
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            var result = await unitOfWork.Connection.QueryAsync<T>(
                storedProcedureName,
                queryParameters,
                commandType: CommandType.StoredProcedure,
                transaction: unitOfWork.Transaction);

            unitOfWork.Commit();

            return result;
        }

        public void ExecuteStoredProcedure(string storedProcedureName,
            object queryParameters)
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            unitOfWork.Connection.Query(
                storedProcedureName,
                queryParameters,
                commandType: CommandType.StoredProcedure,
                transaction: unitOfWork.Transaction);

            unitOfWork.Commit();
        }

        public IEnumerable<T> ExecuteTableValuedStoredProcedureForFetch<T>(
            string storedProcedureName,
            object filters)
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            using var query = unitOfWork.Connection.QueryMultiple(
                storedProcedureName,
                filters,
                unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure);
            return query.Read<T>().ToList();
        }

        public int ExecuteQuery(
            string query,
            object queryParameters)
        {
            using var unitOfWork = new UnitOfWork(_connectionString);
            return unitOfWork.Connection.Execute(
                sql: query,
                param: queryParameters,
                commandTimeout: 30,
                transaction: unitOfWork.Transaction,
                commandType: CommandType.Text);
        }
    }
}