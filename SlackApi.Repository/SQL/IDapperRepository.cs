using System.Data.Common;

namespace SlackApi.Repository.SQL
{
    public interface IDapperRepository
    {
        DbConnection GetDbConnection();
        Task<bool> DeleteAsync<T>(T dto) where T : class;
        Task<int?> InsertAsync<T>(T dto, int? itemId = null) where T : class;
        bool Update<T>(T dto) where T : class;
        Task<bool> UpdateAsync<T>(T model) where T : class;
        T GetById<T>(int id) where T : class;
        Task<T> GetByIdAsync<T>(int id) where T : class;
        IEnumerable<T> Query<T>(string query) where T : class;
        IEnumerable<T> Query<T>(string query, object queryParameters) where T : class;
        Task<IEnumerable<T>> QueryAsync<T>(string query) where T : class;
        Task<IEnumerable<T>> QueryAsync<T>(string query, object queryParameters);
        IEnumerable<T> ExecuteStoredProcedure<T>(string storedProcedureName, object queryParameters);
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, object queryParameters);
        void ExecuteStoredProcedure(string storedProcedureName,
            object queryParameters);
        IEnumerable<T> ExecuteTableValuedStoredProcedureForFetch<T>(
            string storedProcedureName,
            object filters);
        IEnumerable<T> QueryStruct<T>(string query, object queryParameters) where T : struct;
        int ExecuteQuery(
            string query,
            object queryParameters);
    }
}